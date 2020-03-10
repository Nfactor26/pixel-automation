extern alias uiaComWrapper;
using System.Collections.Generic;
using uiaComWrapper::System.Windows.Automation;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Components;
using Polly;
using Polly.Retry;
using Serilog;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;


namespace Pixel.Automation.UIA.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("UIA Control Locator", "Control Locators", iconSource: null, description: "Control Locator component for UIA", tags: new string[] { "Control Locator", "UIA" })]
    public class UIAControlLocatorComponent : ServiceComponent, IControlLocator<AutomationElement, AutomationElement>, ICoordinateProvider
    {
        #region Data Members

        [NonSerialized]
        bool showBoundingBox;
        /// <summary>
        /// Toggle if bounding box is shown during playback on controls.
        /// This can help visually debug the control lookup process in hierarchy
        /// </summary>
        public bool ShowBoundingBox
        {
            get
            {
                return showBoundingBox;
            }
            set
            {
                showBoundingBox = value;
            }
        }

        /// <summary>
        /// Inidicates if the control lookup will be restricted inside Process that was launched or attached to
        /// </summary>
        [DataMember]
        public bool MatchProcessId { get; set; } = true;

        int retryAttempts = 5;
        [DataMember]
        public int RetryAttempts
        {
            get
            {
                return retryAttempts;
            }
            set
            {
                retryAttempts = value;
                retrySequence.Clear();
                foreach (var i in Enumerable.Range(1, value))
                {
                    retrySequence.Add(TimeSpan.FromSeconds(retryInterval));
                }
            }
        }

        double retryInterval = 2;
        [DataMember]
        public double RetryInterval
        {
            get
            {
                return retryInterval;
            }
            set
            {
                retryInterval = value;
                retrySequence.Clear();
                foreach (var i in Enumerable.Range(1, retryAttempts))
                {
                    retrySequence.Add(TimeSpan.FromSeconds(value));
                }
            }
        }
      

        [RequiredComponent]
        [IgnoreDataMember]
        [Browsable(false)]
        public WinApplication TargetApplication
        {
            get
            {
                return this.EntityManager.GetApplicationDetails<WinApplication>(this);
            }
        }
        
        [NonSerialized]
        private IHighlightRectangle highlightRectangle;
        

        [NonSerialized]
        private RetryPolicy policy;

        [NonSerialized]
        private List<TimeSpan> retrySequence;

        #endregion Data Members

        public UIAControlLocatorComponent() : base("UIA Control Locator", "UIAControlLocatorComponent")
        {
            retrySequence = new List<TimeSpan>();
            policy = Policy
           .Handle<ElementNotFoundException>()
           .WaitAndRetry(retrySequence,(exception, timeSpan, retryCount, context) =>
           {
               Log.Error(exception, exception.Message); ;
               if(retryCount < retrySequence.Count)
               {
                   Log.Information("Control lookup  will be attempated again.");
               }
           });          
        }

        #region IControlLocator

        public bool CanProcessControlOfType(IControlIdentity controlIdentity)
        {
            return controlIdentity is WinControlIdentity;
        }

        public AutomationElement FindControl(IControlIdentity controlIdentity, AutomationElement searchRoot)
        {
            Guard.Argument(controlIdentity).NotNull().Compatible<WinControlIdentity>();

          
            AutomationElement currentSearchRoot = searchRoot ?? AutomationElement.RootElement;
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
               
                return foundElement ??  throw new ElementNotFoundException("No control could be located using specified search criteria");
            }
        }

        public IEnumerable<AutomationElement> FindAllControls(IControlIdentity controlIdentity, AutomationElement searchRoot)
        {
            Guard.Argument(controlIdentity).NotNull().Compatible<WinControlIdentity>();
            AutomationElement currentSearchRoot = searchRoot ?? AutomationElement.RootElement;
            IControlIdentity currentControl = controlIdentity;
            while (true)
            {
                WinControlIdentity winControlIdentity = currentControl as WinControlIdentity;
                if (winControlIdentity.Next != null)
                {
                    currentSearchRoot = FindSingleControl(winControlIdentity, currentSearchRoot);
                    currentControl = winControlIdentity.Next;
                    continue;
                }

                switch (winControlIdentity.SearchScope)
                {
                    case SearchScope.Children:
                        var childControls = FindAllChildControls(winControlIdentity, currentSearchRoot);
                        return childControls;
                    case SearchScope.Descendants:
                        var descendantControls = FindAllDescendantControls(winControlIdentity, currentSearchRoot);
                        return descendantControls;
                    case SearchScope.Sibling:
                        var siblingControls = FindAllSiblingControls(winControlIdentity, currentSearchRoot);
                        return siblingControls;
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

            return foundElement ?? throw new ElementNotFoundException("No control could be located using specified search criteria");
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
                if (winControlIdentity.Index.HasValue)
                {
                    int index = winControlIdentity.Index.Value;
                    var foundControls = currentSearchRoot.FindAll(TreeScope.Children, lookupCondition).ToList();
                    if (foundControls.Count() == 0)
                        throw new ElementNotFoundException($"{winControlIdentity} couldn't be located");
                    return GetElementAtConfiguredIndex(foundControls, winControlIdentity);
                }
                else
                {
                    var matchingChildren = currentSearchRoot.FindFirst(TreeScope.Children, lookupCondition)
                         ?? throw new ElementNotFoundException($"{winControlIdentity} couldn't be located");
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
                    throw new ElementNotFoundException($"{winControlIdentity} couldn't be located");
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
                if (winControlIdentity.Index.HasValue)
                {
                    int index = winControlIdentity.Index.Value;
                    var foundControls = currentSearchRoot.FindAll(TreeScope.Children | TreeScope.Descendants, lookupCondition).ToList();
                    if (foundControls.Count() == 0)
                        throw new ElementNotFoundException($"{winControlIdentity} couldn't be located");
                    return GetElementAtConfiguredIndex(foundControls, winControlIdentity);
                }
                else
                {
                    var matchingChildren = currentSearchRoot.FindFirst(TreeScope.Children | TreeScope.Descendants, lookupCondition)
                      ?? throw new ElementNotFoundException($"{winControlIdentity} couldn't be located");
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
                    throw new ElementNotFoundException($"{winControlIdentity} couldn't be located");
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
                var matchingAncestor = searchRoot.FindFirst(TreeScope.Ancestors, lookupCondition)
                  ?? throw new ElementNotFoundException($"{winControlIdentity} couldn't be located");
                return matchingAncestor;
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
                if(winControlIdentity.Index.HasValue)
                {
                    int index = winControlIdentity.Index.Value;
                    var foundControls = parentElement.FindAll(TreeScope.Children, lookupCondition).ToList();
                    if (foundControls.Count() == 0)
                        throw new ElementNotFoundException($"{winControlIdentity} couldn't be located");               
                    return GetElementAtConfiguredIndex(foundControls, winControlIdentity);
                }
                else
                {
                    var matchingChildren = parentElement.FindFirst(TreeScope.Children, lookupCondition)
                                        ?? throw new ElementNotFoundException($"{winControlIdentity} couldn't be located");
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
                    throw new ElementNotFoundException($"{winControlIdentity} couldn't be located");
                return matchingChildren.ToList();
            });
            return foundControls;
        }

        #endregion Sibling Control

        #region Filter

        protected AutomationElement GetElementAtConfiguredIndex(IEnumerable<AutomationElement> foundControls, WinControlIdentity winControlIdentity)
        {
            if (winControlIdentity.Index.HasValue)
            {
                int index = winControlIdentity.Index.Value;
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
            Condition searchCondition = ConditionFactory.FromControlType(uiaComWrapper.System.Windows.Automation.ControlType.LookupById(controlIdentity.ControlTypeId));
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
            RetryAttempts = controlIdentity.RetryAttempts;
            RetryInterval = controlIdentity.RetryInterval;
        }

        private void HighlightElement(AutomationElement foundControl)
        {
            if (highlightRectangle == null)
            {
                highlightRectangle = this.EntityManager.GetServiceOfType<IHighlightRectangle>();
            }

            if (showBoundingBox && foundControl != null)
            {
                var boundingBox = foundControl.Current.BoundingRectangle;
                if (boundingBox != System.Windows.Rect.Empty)
                {
                    highlightRectangle.Visible = true;

                    highlightRectangle.Location = new Rectangle((int)boundingBox.Left, (int)boundingBox.Top, (int)boundingBox.Width, (int)boundingBox.Height);
                    Thread.Sleep(500);

                    highlightRectangle.Visible = false;

                }
            }

        }

        #endregion private methods

        #region ICoordinateProvider      

        public void GetClickablePoint(IControlIdentity controlDetails, out double x, out double y)
        {
            GetScreenBounds(controlDetails, out Rectangle bounds);
            controlDetails.GetClickablePoint(bounds, out x, out y);
        }

        public void GetScreenBounds(IControlIdentity controlDetails, out Rectangle screenBounds)
        {
            WinControlIdentity controlIdentity = controlDetails as WinControlIdentity;
            AutomationElement targetControl = this.FindControl(controlIdentity, AutomationElement.RootElement);
            screenBounds = GetBoundingBox(targetControl);
        }

        public Rectangle GetBoundingBox(AutomationElement automationElement)
        {
            var boundingBox = automationElement.Current.BoundingRectangle;
            return new Rectangle((int)boundingBox.Left, (int)boundingBox.Top, (int)boundingBox.Width, (int)boundingBox.Height);
        }

        #endregion ICoordinateProvider          
    }
}

