using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using Polly;
using Polly.Retry;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Pixel.Windows.Automation;


namespace Pixel.Automation.UIA.Components;

[DataContract]
[Serializable]
[ToolBoxItem("UIA Locator", "Control Locators", iconSource: null, description: "Identify a UIA based control on screen", tags: new string[] { "Locator" })]
public class UIAControlLocatorComponent : ServiceComponent, IControlLocator, ICoordinateProvider
{
    #region Data Members

    private readonly ILogger logger = Log.ForContext<UIAControlLocatorComponent>();
    private IHighlightRectangle highlightRectangle;
    private readonly RetryPolicy policy;
    private readonly List<TimeSpan> retrySequence = new();
    private int retryAttempts = 5;
    private double retryInterval = 2;

    /// <summary>
    /// Toggle if bounding box is shown during playback on controls.
    /// This can help visually debug the control lookup process in hierarchy
    /// </summary>
    [IgnoreDataMember]
    public bool ShowBoundingBox { get; set; }        

    /// <summary>
    /// Inidicates if the control lookup will be restricted inside Process that was launched or attached to
    /// </summary>
    [DataMember]
    public bool MatchProcessId { get; set; } = true;         

    [IgnoreDataMember]
    [Browsable(false)]
    public WinApplication TargetApplication
    {
        get
        {
            return this.EntityManager.GetOwnerApplication<WinApplication>(this);
        }
    }              

    #endregion Data Members

    public UIAControlLocatorComponent() : base("UIA Control Locator", "UIAControlLocatorComponent")
    {
        foreach (var i in Enumerable.Range(1, retryAttempts))
        {
            retrySequence.Add(TimeSpan.FromSeconds(retryInterval));
        }
        policy = Policy
       .Handle<ElementNotFoundException>()
       .WaitAndRetry(retrySequence,(exception, timeSpan, retryCount, context) =>
       {
           logger.Error(exception, exception.Message); ;
           if(retryCount < retrySequence.Count)
           {
               logger.Debug("Control lookup  will be attempated again.");
           }
       });          
    }

    #region IControlLocator

    public bool CanProcessControlOfType(IControlIdentity controlIdentity)
    {
        return controlIdentity is WinControlIdentity;
    }

    public async Task<UIControl> FindControlAsync(IControlIdentity controlIdentity, UIControl searchRoot)
    {
        Guard.Argument(controlIdentity).NotNull().Compatible<WinControlIdentity>();
           
        AutomationElement currentSearchRoot = searchRoot?.GetApiControl<AutomationElement>() ?? AutomationElement.RootElement;
        IControlIdentity currentControl = controlIdentity;          
       
        while (true)
        {
            WinControlIdentity winControlIdentity = currentControl as WinControlIdentity;
            var foundElement = FindSingleControl(winControlIdentity, currentSearchRoot);          
            if (winControlIdentity.Next != null)
            {
                currentControl = winControlIdentity.Next;
                currentSearchRoot = foundElement;
                continue;
            }
            logger.Debug("{0} has been located", controlIdentity);
            var foundControl = new WinUIControl(controlIdentity, foundElement ??  throw new ElementNotFoundException($"Control : '{controlIdentity}' could not be located"));
            return await Task.FromResult(foundControl);
        }
    }

    public async Task<IEnumerable<UIControl>> FindAllControlsAsync(IControlIdentity controlIdentity, UIControl searchRoot)
    {
        Guard.Argument(controlIdentity).NotNull().Compatible<WinControlIdentity>();
       
        AutomationElement currentSearchRoot = searchRoot?.GetApiControl<AutomationElement>() ?? AutomationElement.RootElement;
        IControlIdentity currentControl = controlIdentity;
        while (true)
        {
            WinControlIdentity winControlIdentity = currentControl as WinControlIdentity;
            if (winControlIdentity.Next != null)
            {
                currentSearchRoot = FindSingleControl(winControlIdentity, currentSearchRoot);
                logger.Debug("{0} has been located", controlIdentity);
                currentControl = winControlIdentity.Next;
                continue;
            }

            switch (winControlIdentity.SearchScope)
            {
                case SearchScope.Children:
                    var childControls = FindAllChildControls(winControlIdentity, currentSearchRoot);
                    logger.Debug("Located {0} matching control for control : {1}", childControls.Count() , winControlIdentity);
                    return await Task.FromResult(childControls.Select(c => new WinUIControl(controlIdentity, c)));
                case SearchScope.Descendants:
                    var descendantControls = FindAllDescendantControls(winControlIdentity, currentSearchRoot);
                    logger.Debug("Located {0} matching control for control : {1}", descendantControls.Count(), winControlIdentity);
                    return await Task.FromResult(descendantControls.Select(c => new WinUIControl(controlIdentity, c)));
                case SearchScope.Sibling:
                    var siblingControls = FindAllSiblingControls(winControlIdentity, currentSearchRoot);
                    logger.Debug("Located {0} matching control for control : {1}", siblingControls.Count(), winControlIdentity);
                    return await Task.FromResult(siblingControls.Select(c => new WinUIControl(controlIdentity, c)));
                case SearchScope.Ancestor:
                    throw new InvalidOperationException("There can be only one ancestor for a given control");
            }

        }
    }

    private AutomationElement FindSingleControl(IControlIdentity controlIdentity, AutomationElement searchRoot)
    {
        WinControlIdentity winControlIdentity = controlIdentity as WinControlIdentity;
        AutomationElement foundElement = default;
        switch (winControlIdentity.SearchScope)
        {
            case SearchScope.Children:
                foundElement = FindChildControl(winControlIdentity, searchRoot);
                break;
            case SearchScope.Descendants:
                foundElement = FindDescendantControl(winControlIdentity, searchRoot);
                break;
            case SearchScope.Sibling:
                foundElement = FindSiblingControl(winControlIdentity, searchRoot);
                break;
            case SearchScope.Ancestor:
                foundElement = FindAncestorControl(winControlIdentity, searchRoot);
                break;
        }

        return foundElement ?? throw new ElementNotFoundException($"Control : '{controlIdentity}' could not be located");
    }

    #endregion IControlLocator

    #region Child Control

    public AutomationElement FindChildControl(IControlIdentity controlIdentity, AutomationElement searchRoot = null)
    {
        Guard.Argument(controlIdentity).NotNull().Compatible<WinControlIdentity>();

        WinControlIdentity winControlIdentity = controlIdentity as WinControlIdentity;
        ConfigureRetryPolicy(controlIdentity);
        Condition lookupCondition = BuildSearchCondition(winControlIdentity);
     
        var currentSearchRoot = searchRoot ?? AutomationElement.RootElement;

        var foundControl = policy.Execute(() =>
        {
            if (winControlIdentity.Index > 1)
            {                 
                var foundControls = currentSearchRoot.FindAll(TreeScope.Children, lookupCondition).ToList();
                if (foundControls.Count() == 0)
                {
                    throw new ElementNotFoundException($"Control : '{controlIdentity}' could not be located");
                }
                return GetElementAtConfiguredIndex(foundControls, winControlIdentity);
            }
            else
            {
                var matchingChildren = currentSearchRoot.FindFirst(TreeScope.Children, lookupCondition)
                     ?? throw new ElementNotFoundException($"Control : '{controlIdentity}' could not be located");
                return matchingChildren;
            }  
          
        });

        HighlightElement(foundControl);
        return foundControl;
    }

    public IEnumerable<AutomationElement> FindAllChildControls(IControlIdentity controlIdentity, AutomationElement searchRoot = null)
    {
        Guard.Argument(controlIdentity).NotNull().Compatible<WinControlIdentity>();

        WinControlIdentity winControlIdentity = controlIdentity as WinControlIdentity;
        ConfigureRetryPolicy(controlIdentity);
        Condition lookupCondition = BuildSearchCondition(winControlIdentity);

        var currentSearchRoot = searchRoot ?? AutomationElement.RootElement;

        var foundControls = policy.Execute(() =>
        {                
            var matchingChildren = currentSearchRoot.FindAll(TreeScope.Children, lookupCondition).ToList();
            if (matchingChildren.Count() == 0)
            {
                throw new ElementNotFoundException($"Control : '{controlIdentity}' could not be located");
            }
            return matchingChildren.ToList();
        });
        return foundControls;
    }

    #endregion Child Control

    #region Descendant Control

    public AutomationElement FindDescendantControl(IControlIdentity controlIdentity, AutomationElement searchRoot = null)
    {
        Guard.Argument(controlIdentity).NotNull().Compatible<WinControlIdentity>();

        WinControlIdentity winControlIdentity = controlIdentity as WinControlIdentity;
        ConfigureRetryPolicy(controlIdentity);
        Condition lookupCondition = BuildSearchCondition(winControlIdentity);

        var currentSearchRoot = searchRoot ?? AutomationElement.RootElement;

        var foundControl = policy.Execute(() =>
        {
            if (winControlIdentity.Index > 1)
            {                   
                var foundControls = currentSearchRoot.FindAll(TreeScope.Children | TreeScope.Descendants, lookupCondition).ToList();
                if (foundControls.Count() == 0)
                {
                    throw new ElementNotFoundException($"Control : '{controlIdentity}' could not be located");
                }
                return GetElementAtConfiguredIndex(foundControls, winControlIdentity);
            }
            else
            {
                var matchingChildren = currentSearchRoot.FindFirst(TreeScope.Children | TreeScope.Descendants, lookupCondition)
                  ?? throw new ElementNotFoundException($"Control : '{controlIdentity}' could not be located");
                return matchingChildren;
            }                  
        });

        HighlightElement(foundControl);
        return foundControl;
    }

    public IEnumerable<AutomationElement> FindAllDescendantControls(IControlIdentity controlIdentity, AutomationElement searchRoot = null)
    {
        Guard.Argument(controlIdentity).NotNull().Compatible<WinControlIdentity>();

        WinControlIdentity winControlIdentity = controlIdentity as WinControlIdentity;
        ConfigureRetryPolicy(controlIdentity);
        Condition lookupCondition = BuildSearchCondition(winControlIdentity);

        var currentSearchRoot = searchRoot ?? AutomationElement.RootElement;

        var foundControls = policy.Execute(() =>
        {                       
            var matchingChildren = currentSearchRoot.FindAll(TreeScope.Children | TreeScope.Descendants, lookupCondition).ToList();
            if (matchingChildren.Count() == 0)
            {
                throw new ElementNotFoundException($"Control : '{controlIdentity}' could not be located");
            }
            return matchingChildren.ToList();
        });
        return foundControls;
    }

    #endregion Descendant Control

    #region Ancestor Control

    public AutomationElement FindAncestorControl(IControlIdentity controlIdentity, AutomationElement searchRoot)
    {
        Guard.Argument(controlIdentity).NotNull().Compatible<WinControlIdentity>();

        WinControlIdentity winControlIdentity = controlIdentity as WinControlIdentity;
        ConfigureRetryPolicy(controlIdentity);
        Condition lookupCondition = BuildSearchCondition(winControlIdentity);

        AutomationElement foundControl = policy.Execute(() =>
        {
            if(searchRoot != AutomationElement.RootElement)
            {
                var current = TreeWalker.RawViewWalker.GetParent(searchRoot);
                while (current != AutomationElement.RootElement)
                {
                    var matchingAncestor = current.FindFirst(TreeScope.Element, lookupCondition);
                    if(matchingAncestor != null)
                    {
                        return matchingAncestor;
                    }
                    current = TreeWalker.RawViewWalker.GetParent(current);
                }
            }
            throw new ElementNotFoundException($"Control : '{controlIdentity}' could not be located");
        });

        HighlightElement(foundControl);
        return foundControl;
    }

    #endregion Ancestor Control

    #region Sibling Control

    public AutomationElement FindSiblingControl(IControlIdentity controlIdentity, AutomationElement searchRoot)
    {
        Guard.Argument(controlIdentity).NotNull().Compatible<WinControlIdentity>();

        WinControlIdentity winControlIdentity = controlIdentity as WinControlIdentity;
        ConfigureRetryPolicy(controlIdentity);
        Condition lookupCondition = BuildSearchCondition(winControlIdentity);

     
        var currentSearchRoot = searchRoot ?? AutomationElement.RootElement;
        AutomationElement parentElement = TreeWalker.RawViewWalker.GetParent(currentSearchRoot);

        var foundControl = policy.Execute(() =>
        {
            if(winControlIdentity.Index > 1)
            {                   
                var foundControls = parentElement.FindAll(TreeScope.Children, lookupCondition).ToList();
                if (foundControls.Count() == 0)
                {
                    throw new ElementNotFoundException($"Control : '{controlIdentity}' could not be located");
                }
                return GetElementAtConfiguredIndex(foundControls, winControlIdentity);
            }
            else
            {
                var matchingChildren = parentElement.FindFirst(TreeScope.Children, lookupCondition)
                                    ?? throw new ElementNotFoundException($"Control : '{controlIdentity}' could not be located");
                return matchingChildren;
            }                             
        });
        HighlightElement(foundControl);
        return foundControl;
    }

    public IEnumerable<AutomationElement> FindAllSiblingControls(IControlIdentity controlIdentity, AutomationElement searchRoot)
    {
        Guard.Argument(controlIdentity).NotNull().Compatible<WinControlIdentity>();

        WinControlIdentity winControlIdentity = controlIdentity as WinControlIdentity;
        ConfigureRetryPolicy(controlIdentity);
        Condition lookupCondition = BuildSearchCondition(winControlIdentity);

        var currentSearchRoot = searchRoot ?? AutomationElement.RootElement;
        AutomationElement parentElement = TreeWalker.RawViewWalker.GetParent(currentSearchRoot);

        var foundControls = policy.Execute(() =>
        {            
            var matchingChildren = parentElement.FindAll(TreeScope.Children, lookupCondition).ToList();
            if (matchingChildren.Count() == 0)
            {
                throw new ElementNotFoundException($"Control : '{controlIdentity}' could not be located");
            }
            return matchingChildren.ToList();
        });
        return foundControls;
    }

    #endregion Sibling Control

    #region Filter

    protected AutomationElement GetElementAtConfiguredIndex(IEnumerable<AutomationElement> foundControls, WinControlIdentity winControlIdentity)
    {
        if (winControlIdentity.Index > 0)
        {
            int index = winControlIdentity.Index - 1;
            if (foundControls.Count() > index)
            {
                var foundControl = foundControls.ElementAt(index);
                HighlightElement(foundControl);
                return foundControl;
            }
            throw new IndexOutOfRangeException($"Found {foundControls.Count()} controls.Desired index : {index} is greater than number of found controls");
        }
        throw new InvalidOperationException($"Index doesn't have value.");
    }

    #endregion Filter

    #region private methods

    private Condition BuildSearchCondition(WinControlIdentity controlIdentity)
    {
        Condition searchCondition = ConditionFactory.FromControlType(controlIdentity.WinControlType.ToUIAControlType());
        if (MatchProcessId)
        {
            searchCondition = searchCondition.AndProcessId(TargetApplication.ProcessId);
        }
        searchCondition = searchCondition.AndPropertyCondition(new PropertyCondition(AutomationElement.IsContentElementProperty, controlIdentity.IsContentElement));
        searchCondition = searchCondition.AndPropertyCondition(new PropertyCondition(AutomationElement.IsControlElementProperty, controlIdentity.IsControlElement));
        
        if (!string.IsNullOrEmpty(controlIdentity.NameProperty))
        {
            searchCondition = searchCondition.AndName(controlIdentity.NameProperty);
        }

        if (!string.IsNullOrEmpty(controlIdentity.AutomationId))
        {
            searchCondition = searchCondition.AndAutomationId(controlIdentity.AutomationId);
        }
        if (!string.IsNullOrEmpty(controlIdentity.ClassName))
        {
            searchCondition = searchCondition.AndClassName(controlIdentity.ClassName);
        }
        if (!string.IsNullOrEmpty(controlIdentity.AccessKey))
        {
            searchCondition = searchCondition.AndAccessKey(controlIdentity.AccessKey);
        }
        if (!string.IsNullOrEmpty(controlIdentity.HelpText))
        {
            searchCondition = searchCondition.AndHelpText(controlIdentity.HelpText);
        }
        if (!string.IsNullOrEmpty(controlIdentity.AcceleratorKey))
        {
            searchCondition = searchCondition.AndAccessKey(controlIdentity.AcceleratorKey);
        }

        return searchCondition;
    }

    private void ConfigureRetryPolicy(IControlIdentity controlIdentity)
    {                       
        if(this.retryAttempts != controlIdentity.RetryAttempts || this.retryInterval != controlIdentity.RetryInterval)
        {
            this.retryAttempts = controlIdentity.RetryAttempts;
            this.retryInterval = controlIdentity.RetryInterval;
            retrySequence.Clear();
            foreach (var i in Enumerable.Range(1, this.retryAttempts))
            {
                retrySequence.Add(TimeSpan.FromSeconds(this.retryInterval));
            }
        }           
    }

    private void HighlightElement(AutomationElement foundControl)
    {         

        if (ShowBoundingBox && foundControl != null)
        {
            if (highlightRectangle == null)
            {
                highlightRectangle = this.EntityManager.GetServiceOfType<IHighlightRectangle>();
            }

            var boundingBox = foundControl.Current.BoundingRectangle;
            if (boundingBox != System.Windows.Rect.Empty)
            {
                highlightRectangle.Visible = true;

                highlightRectangle.Location = new BoundingBox((int)boundingBox.Left, (int)boundingBox.Top, (int)boundingBox.Width, (int)boundingBox.Height);
                Thread.Sleep(500);

                highlightRectangle.Visible = false;

            }
        }

    }

    #endregion private methods

    #region ICoordinateProvider      

    public async Task<(double,double)> GetClickablePoint(IControlIdentity controlDetails)
    {
        var bounds = await GetScreenBounds(controlDetails);
        controlDetails.GetClickablePoint(bounds, out double x, out double y);
        return await Task.FromResult((x, y));
    }

    public async Task<BoundingBox> GetScreenBounds(IControlIdentity controlDetails)
    {
        WinControlIdentity controlIdentity = controlDetails as WinControlIdentity;
        var targetControl = await this.FindControlAsync(controlIdentity, WinUIControl.RootControl);
        var screenBounds = await GetBoundingBox(targetControl);
        return screenBounds;
    }

    public async Task<BoundingBox> GetBoundingBox(object control)
    {
        Guard.Argument(control).NotNull().Compatible<AutomationElement>();

        var automationElement = control as AutomationElement;
        var boundingBox = automationElement.Current.BoundingRectangle;
        return await Task.FromResult(new BoundingBox((int)boundingBox.Left, (int)boundingBox.Top, (int)boundingBox.Width, (int)boundingBox.Height));
    }

    #endregion ICoordinateProvider          
}

