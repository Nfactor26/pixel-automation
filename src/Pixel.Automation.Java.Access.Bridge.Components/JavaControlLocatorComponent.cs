﻿using Dawn;
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
using WindowsAccessBridgeInterop;

namespace Pixel.Automation.Java.Access.Bridge.Components;

[DataContract]
[Serializable]
[ToolBoxItem("JAB Locator", "Control Locators", iconSource: null, description: "Identify a java control on screen", tags: new string[] { "Locator" })]
public class JavaControlLocatorComponent : ServiceComponent, IControlLocator, ICoordinateProvider, IDisposable
{
    private readonly ILogger logger = Log.ForContext<JavaControlLocatorComponent>();       
    private IHighlightRectangle highlightRectangle;       
    private readonly RetryPolicy policy;       
    private readonly List<TimeSpan> retrySequence = new();       
    private readonly AccessBridge accessBridge;
    private AccessibleJvm accessibleJvm;
    private int jvmId = 0;
    private int retryAttempts = 5;
    private double retryInterval = 2;

    /// <summary>
    /// Toggle if bounding box is shown during playback on controls.
    /// This can help visuall debug the control location process in hierarchy
    /// </summary>
    public bool ShowBoundingBox { get; set; }       

    [Browsable(false)]
    [IgnoreDataMember]
    public IApplication TargetApplication
    {
        get
        {
            return this.EntityManager.GetOwnerApplication<IApplication>(this);
        }
    }     

    public JavaControlLocatorComponent() : base("Java Control Locator", "JavaControlLocator")
    {
        foreach (var i in Enumerable.Range(1, retryAttempts))
        {
            retrySequence.Add(TimeSpan.FromSeconds(retryInterval));
        }
        this.policy = Policy.Handle<ElementNotFoundException>()
        .WaitAndRetry(retrySequence, (exception, timeSpan, retryCount, context) =>
        {
            logger.Error(exception, exception.Message); ;
            if (retryCount < retrySequence.Count)
            {
                logger.Debug("Control lookup  will be attempated again.");
            }
        });

        accessBridge = new AccessBridge();
        accessBridge.Initialize();
        logger.Information($"Access Bridge initialize. Version : {accessBridge.LibraryVersion}, IsLegacy : {accessBridge.IsLegacy}");
    }

    #region IControlLocator

    public bool CanProcessControlOfType(IControlIdentity controlIdentity)
    {
        return controlIdentity is JavaControlIdentity;
    }

    public async Task<UIControl> FindControlAsync(IControlIdentity controlIdentity, UIControl searchRoot)
    {
        Guard.Argument(controlIdentity).NotNull().Compatible<JavaControlIdentity>();
       
        IControlIdentity currentControl = controlIdentity;
        ConfigureRetryPolicy(currentControl);
        var currentSearchRoot = searchRoot?.GetApiControl<AccessibleContextNode>() ?? GetSearchRoot(ref currentControl);
        while (true)
        {
            var foundElement = FindSingleControl(currentControl, currentSearchRoot);
            logger.Debug("{0} has been located", controlIdentity);
            if (currentControl.Next != null)
            {
                currentControl = currentControl.Next;
                currentSearchRoot = foundElement;
                continue;
            }           
            return await Task.FromResult(new JavaUIControl(controlIdentity, foundElement ?? throw new ElementNotFoundException($"Control : '{controlIdentity}' could not be located")));
        }
    }

    public async Task<IEnumerable<UIControl>> FindAllControlsAsync(IControlIdentity controlIdentity, UIControl searchRoot)
    {
        Guard.Argument(controlIdentity).NotNull().Compatible<JavaControlIdentity>();
      
        IControlIdentity currentControl = controlIdentity;
        ConfigureRetryPolicy(currentControl);
        var currentSearchRoot = searchRoot?.GetApiControl<AccessibleContextNode>() ?? GetSearchRoot(ref currentControl);

        while (true)
        {
            JavaControlIdentity javaControlIdentity = currentControl as JavaControlIdentity;
            if (javaControlIdentity.Next != null)
            {
                currentSearchRoot = FindSingleControl(javaControlIdentity, currentSearchRoot);
                logger.Debug("{0} has been located", controlIdentity);
                currentControl = javaControlIdentity.Next;
                continue;
            }

            switch (javaControlIdentity.SearchScope)
            {
                case SearchScope.Children:
                    var childControls = FindAllChildControls(javaControlIdentity, currentSearchRoot);
                    logger.Debug("Located {0} matching control for control : {1}", childControls.Count(), javaControlIdentity);
                    return await Task.FromResult(childControls.Select(d => new JavaUIControl(controlIdentity, d)));
                case SearchScope.Descendants:
                    var descendantControls = FindAllDescendantControls(javaControlIdentity, currentSearchRoot);
                    logger.Debug("Located {0} matching control for control : {1}", descendantControls.Count(), javaControlIdentity);
                    return await Task.FromResult(descendantControls.Select(d => new JavaUIControl(controlIdentity, d)));
                case SearchScope.Sibling:
                    var siblingControls = FindAllSiblingControls(javaControlIdentity, currentSearchRoot);
                    logger.Debug("Located {0} matching control for control : {1}", siblingControls.Count(), javaControlIdentity);
                    return await Task.FromResult(siblingControls.Select(d => new JavaUIControl(controlIdentity, d)));
                case SearchScope.Ancestor:
                    throw new InvalidOperationException("There can be only one ancestor for a given control");
            }
        }
    }

    private AccessibleContextNode FindSingleControl(IControlIdentity controlIdentity, AccessibleContextNode searchRoot)
    {
        AccessibleContextNode foundElement = default;
        JavaControlIdentity javaControlIdentity = controlIdentity as JavaControlIdentity;
        switch (javaControlIdentity.SearchScope)
        {
            case SearchScope.Children:
                foundElement = FindChildControl(controlIdentity, searchRoot);
                break;
            case SearchScope.Descendants:
                foundElement = FindDescendantControl(controlIdentity, searchRoot);
                break;
            case SearchScope.Sibling:
                foundElement = FindSiblingControl(controlIdentity, searchRoot);
                break;
            case SearchScope.Ancestor:
                foundElement = FindAncestorControl(controlIdentity, searchRoot);
                break;
        }

        return foundElement ?? throw new ElementNotFoundException($"Control : '{controlIdentity}' could not be located");
    }

    #endregion IControlLocator

    #region Child Control

    public AccessibleContextNode FindChildControl(IControlIdentity controlIdentity, AccessibleContextNode searchRoot = null)
    {
        Guard.Argument(controlIdentity).NotNull().Compatible<JavaControlIdentity>();

        var currentSearchRoot = searchRoot ?? GetSearchRoot(ref controlIdentity);
        JavaControlIdentity javaControlIdentity = controlIdentity as JavaControlIdentity;
        ConfigureRetryPolicy(javaControlIdentity);

        var foundControl = policy.Execute(() =>
        {
            if (javaControlIdentity.Index > 1)
            {
                var foundControls = searchRoot.FindAll(TreeScope.Children, javaControlIdentity);
                if (foundControls.Count() == 0)
                {
                    throw new ElementNotFoundException($"Control : '{controlIdentity}' could not be located");
                }
                return GetElementAtConfiguredIndex(foundControls, javaControlIdentity);
            }
            else
            {
                var matchingChild = currentSearchRoot.FindFirst(TreeScope.Children, javaControlIdentity)
                     ?? throw new ElementNotFoundException($"Control : '{controlIdentity}' could not be located");
                return matchingChild;
            }
        });

        HighlightElement(foundControl);
        return foundControl;
    }

    public IEnumerable<AccessibleContextNode> FindAllChildControls(IControlIdentity controlIdentity, AccessibleContextNode searchRoot = null)
    {
        Guard.Argument(controlIdentity).NotNull().Compatible<JavaControlIdentity>();

        var currentSearchRoot = searchRoot ?? GetSearchRoot(ref controlIdentity);
        JavaControlIdentity javaControlIdentity = controlIdentity as JavaControlIdentity;
        ConfigureRetryPolicy(javaControlIdentity);

        var foundControls = policy.Execute(() =>
        {
            var matchingChildren = currentSearchRoot.FindAll(TreeScope.Children, javaControlIdentity).ToList();
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

    public AccessibleContextNode FindDescendantControl(IControlIdentity controlIdentity, AccessibleContextNode searchRoot = null)
    {
        Guard.Argument(controlIdentity).NotNull().Compatible<JavaControlIdentity>();

        var currentSearchRoot = searchRoot ?? GetSearchRoot(ref controlIdentity);
        JavaControlIdentity javaControlIdentity = controlIdentity as JavaControlIdentity;
        ConfigureRetryPolicy(javaControlIdentity);

        var foundControl = policy.Execute(() =>
        {
            return currentSearchRoot.FindNthDescendantControl(javaControlIdentity, javaControlIdentity.Index) ?? throw new ElementNotFoundException($"Control : '{controlIdentity}' could not be located");
        });

        HighlightElement(foundControl);
        return foundControl;
    }

    public IEnumerable<AccessibleContextNode> FindAllDescendantControls(IControlIdentity controlIdentity, AccessibleContextNode searchRoot = null)
    {
        Guard.Argument(controlIdentity).NotNull().Compatible<JavaControlIdentity>();

        var currentSearchRoot = searchRoot ?? GetSearchRoot(ref controlIdentity);
        JavaControlIdentity javaControlIdentity = controlIdentity as JavaControlIdentity;
        ConfigureRetryPolicy(javaControlIdentity);

        var foundControls = policy.Execute(() =>
        {
            var matchingDescendants = currentSearchRoot.FindAll(TreeScope.Children | TreeScope.Descendants, javaControlIdentity).ToList();
            if (matchingDescendants.Count() == 0)
            {
                throw new ElementNotFoundException($"Control : '{controlIdentity}' could not be located");
            }
            return matchingDescendants;
        });
        return foundControls;
    }

    #endregion Descendant Control

    #region Ancestor Control

    public AccessibleContextNode FindAncestorControl(IControlIdentity controlIdentity, AccessibleContextNode searchRoot)
    {
        Guard.Argument(controlIdentity).NotNull().Compatible<JavaControlIdentity>();

        var currentSearchRoot = searchRoot ?? GetSearchRoot(ref controlIdentity);
        JavaControlIdentity javaControlIdentity = controlIdentity as JavaControlIdentity;
        ConfigureRetryPolicy(javaControlIdentity);

        AccessibleContextNode foundControl = policy.Execute(() =>
        {
            var matchingAncestor = currentSearchRoot.FindFirst(TreeScope.Ancestors, javaControlIdentity)
             ?? throw new ElementNotFoundException($"Control : '{controlIdentity}' could not be located");
            return matchingAncestor;
        });

        HighlightElement(foundControl);
        return foundControl;
    }

    #endregion Ancestor Control

    #region Sibling Control

    public AccessibleContextNode FindSiblingControl(IControlIdentity controlIdentity, AccessibleContextNode searchRoot)
    {
        Guard.Argument(controlIdentity).NotNull().Compatible<JavaControlIdentity>();

        var currentSearchRoot = searchRoot ?? GetSearchRoot(ref controlIdentity);
        var ancestor = currentSearchRoot.GetParent() as AccessibleContextNode;

        var siblingControl = FindChildControl(controlIdentity, ancestor);
        return siblingControl;
    }

    public IEnumerable<AccessibleContextNode> FindAllSiblingControls(IControlIdentity controlIdentity, AccessibleContextNode searchRoot)
    {
        Guard.Argument(controlIdentity).NotNull().Compatible<JavaControlIdentity>();

        var currentSearchRoot = searchRoot ?? GetSearchRoot(ref controlIdentity);
        var ancestor = currentSearchRoot.GetParent() as AccessibleContextNode;

        var siblinControls = FindAllChildControls(controlIdentity, ancestor);
        return siblinControls;
    }

    #endregion Sibling Control

    private AccessibleContextNode GetSearchRoot(ref IControlIdentity controlIdentity)
    {
        if (accessibleJvm == null)
        {
            IntPtr hWnd = TargetApplication.Hwnd;
            var rootWindow = accessBridge.CreateAccessibleWindow(hWnd);
            jvmId = rootWindow.JvmId;
            accessibleJvm = accessBridge.EnumJvms().FirstOrDefault(j => j.JvmId.Equals(jvmId));
        }
        accessibleJvm = accessBridge.EnumJvms().FirstOrDefault(j => j.JvmId.Equals(jvmId));
        JavaControlIdentity javaControlIdentity = controlIdentity as JavaControlIdentity;

        var window = policy.Execute(() =>
        {
            foreach (var window in accessibleJvm.Windows)
            {
                if (window.IsMatch(javaControlIdentity))
                {
                    logger.Debug($"Located control {javaControlIdentity}");
                    return window;
                }
            }
            throw new ElementNotFoundException($"Control : '{javaControlIdentity}' could not be located");
        });

        controlIdentity = controlIdentity.Next;
        return window;
    }

    #region Filter

    protected AccessibleContextNode GetElementAtConfiguredIndex(IEnumerable<AccessibleContextNode> foundControls, JavaControlIdentity javaControlIdentity)
    {
        if (javaControlIdentity.Index > 0)
        {
            int index = javaControlIdentity.Index - 1;
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


    private void ConfigureRetryPolicy(IControlIdentity controlIdentity)
    {
        if (this.retryAttempts != controlIdentity.RetryAttempts || this.retryInterval != controlIdentity.RetryInterval)
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

    private void HighlightElement(AccessibleContextNode foundControl)
    {

        if (ShowBoundingBox && foundControl != null)
        {
            if (highlightRectangle == null)
            {
                highlightRectangle = this.EntityManager.GetServiceOfType<IHighlightRectangle>();
            }

            var controlBounds = foundControl.GetScreenRectangle().Value;
            var boundingBox = new BoundingBox(controlBounds.X, controlBounds.Y, controlBounds.Width, controlBounds.Height);
            if (!BoundingBox.Empty.Equals(boundingBox))
            {
                highlightRectangle.Visible = true;

                highlightRectangle.Location = boundingBox;
                Thread.Sleep(500);

                highlightRectangle.Visible = false;

            }
        }

    }

    #endregion private methods

    #region ICoordinateProvider      

    public async Task<(double, double)> GetClickablePoint(IControlIdentity controlDetails)
    {
        var bounds = await GetScreenBounds(controlDetails);
        controlDetails.GetClickablePoint(bounds, out double x, out double y);
        return await Task.FromResult((x, y));
    }

    public async Task<BoundingBox> GetScreenBounds(IControlIdentity controlIdentity)
    {
        var currentSearchRoot = GetSearchRoot(ref controlIdentity);
        ConfigureRetryPolicy(controlIdentity);
        var targetControl = await this.FindControlAsync(controlIdentity, new JavaUIControl(controlIdentity, currentSearchRoot));
        var screenBounds = await GetBoundingBox(targetControl.GetApiControl<AccessibleContextNode>());
        return screenBounds;
    }

    public async Task<BoundingBox> GetBoundingBox(object control)
    {
        Guard.Argument(control).NotNull().Compatible<AccessibleContextNode>();

        var controlNode = control as AccessibleContextNode;
        var boundingBox = controlNode.GetScreenRectangle().Value;
        return await Task.FromResult(new BoundingBox(boundingBox.X, boundingBox.Y, boundingBox.Width, boundingBox.Height));
    }

    #endregion ICoordinateProvider        

    #region IDisposable

    protected virtual void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            this.accessBridge?.Dispose();
            logger.Debug("Access bridge is disposed now.");
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }

    #endregion IDisposable
}
