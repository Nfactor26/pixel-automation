extern alias uiaComWrapper;

using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
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
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using uiaComWrapper::System.Windows.Automation;
using WindowsAccessBridgeInterop;

namespace Pixel.Automation.Java.Access.Bridge.Components
{
    public class JavaControlLocatorComponent : ServiceComponent, IControlLocator<AccessibleContextNode, AccessibleContextNode>, ICoordinateProvider, IDisposable
    {

        private readonly ILogger logger = Log.ForContext<JavaControlLocatorComponent>();

        [NonSerialized]
        bool showBoundingBox;
        /// <summary>
        /// Toggle if bounding box is shown during playback on controls.
        /// This can help visuall debug the control location process in hierarchy
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
        [Browsable(false)]
        [IgnoreDataMember]
        public IApplication TargetApplication
        {
            get
            {
                return this.EntityManager.GetOwnerApplication<IApplication>(this);
            }
        }

        [NonSerialized]
        private IHighlightRectangle highlightRectangle;


        [NonSerialized]
        private RetryPolicy policy;

        [NonSerialized]
        private List<TimeSpan> retrySequence;

        [NonSerialized]
        AccessBridge accessBridge;

        [NonSerialized]
        AccessibleJvm accessibleJvm;

        int jvmId;
     
        public JavaControlLocatorComponent() : base("Java Control Locator", "JavaControlLocator")
        {
            this.retrySequence = new List<TimeSpan>();
            this.policy = Policy.Handle<ElementNotFoundException>()
           .WaitAndRetry(retrySequence, (exception, timeSpan, retryCount, context) =>
           {
               logger.Error(exception, exception.Message); ;
               if (retryCount < retrySequence.Count)
               {
                   logger.Information("Control lookup  will be attempated again.");
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

        public AccessibleContextNode FindControl(IControlIdentity controlIdentity, AccessibleContextNode searchRoot)
        {
            Guard.Argument(controlIdentity).NotNull().Compatible<JavaControlIdentity>();
            logger.Information("Start control lookup.");

            IControlIdentity currentControl = controlIdentity;
            ConfigureRetryPolicy(currentControl);
            var currentSearchRoot = searchRoot ?? GetSearchRoot(ref currentControl);
            while (true)
            {               
                var foundElement = FindSingleControl(currentControl, currentSearchRoot);
                logger.Information($"Located control {currentControl}");
                if (currentControl.Next != null)
                {
                    currentControl = currentControl.Next;
                    currentSearchRoot = foundElement;
                    continue;
                }
                logger.Information("Control lookup completed.");
                return foundElement ?? throw new ElementNotFoundException("No control could be located using specified search criteria");
            }
        }

        public IEnumerable<AccessibleContextNode> FindAllControls(IControlIdentity controlIdentity, AccessibleContextNode searchRoot)
        {
            Guard.Argument(controlIdentity).NotNull().Compatible<JavaControlIdentity>();
            logger.Information("Start control lookup.");
            
            IControlIdentity currentControl = controlIdentity;
            ConfigureRetryPolicy(currentControl);
            var currentSearchRoot = searchRoot ?? GetSearchRoot(ref currentControl);

            while (true)
            {
                JavaControlIdentity javaControlIdentity = currentControl as JavaControlIdentity;
                if (javaControlIdentity.Next != null)
                {
                    currentSearchRoot = FindSingleControl(javaControlIdentity, currentSearchRoot);
                    logger.Information($"Located control {javaControlIdentity}");
                    currentControl = javaControlIdentity.Next;
                    continue;
                }

                try
                {
                    switch (javaControlIdentity.SearchScope)
                    {
                        case SearchScope.Children:
                            var childControls = FindAllChildControls(javaControlIdentity, currentSearchRoot);
                            logger.Information($"Located {childControls.Count()} matching control for {javaControlIdentity}");
                            return childControls;
                        case SearchScope.Descendants:
                            var descendantControls = FindAllDescendantControls(javaControlIdentity, currentSearchRoot);
                            logger.Information($"Located {descendantControls.Count()} matching control for {javaControlIdentity}");
                            return descendantControls;
                        case SearchScope.Sibling:
                            var siblingControls = FindAllSiblingControls(javaControlIdentity, currentSearchRoot);
                            logger.Information($"Located {siblingControls.Count()} matching control for {javaControlIdentity}");
                            return siblingControls;
                        case SearchScope.Ancestor:
                            throw new InvalidOperationException("There can be only one ancestor for a given control");
                    }
                }
                finally
                {
                    logger.Information("Control lookup completed.");
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

            return foundElement ?? throw new ElementNotFoundException("No control could be located using specified search criteria");
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
                        throw new ElementNotFoundException($"{javaControlIdentity} couldn't be located");
                    }
                    return GetElementAtConfiguredIndex(foundControls, javaControlIdentity);
                }
                else
                {
                    var matchingChild = currentSearchRoot.FindFirst(TreeScope.Children, javaControlIdentity)
                         ?? throw new ElementNotFoundException($"{javaControlIdentity} couldn't be located");
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
                    throw new ElementNotFoundException($"{javaControlIdentity} couldn't be located");
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
                return currentSearchRoot.FindNthDescendantControl(javaControlIdentity, javaControlIdentity.Index) ?? throw new ElementNotFoundException($"{javaControlIdentity} couldn't be located");
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
                    throw new ElementNotFoundException($"{javaControlIdentity} couldn't be located");
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
                 ?? throw new ElementNotFoundException($"{javaControlIdentity} couldn't be located");
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
            
            var window  = policy.Execute(() =>
            {
                foreach (var window in accessibleJvm.Windows)
                {
                    if (window.IsMatch(javaControlIdentity))
                    {
                        logger.Information($"Located control {javaControlIdentity}");
                        return window;
                    }
                }
                throw new ElementNotFoundException($"{javaControlIdentity} couldn't be located");
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
            RetryAttempts = controlIdentity.RetryAttempts;
            RetryInterval = controlIdentity.RetryInterval;
        }

        private void HighlightElement(AccessibleContextNode foundControl)
        {

            if (showBoundingBox && foundControl != null)
            {
                if (highlightRectangle == null)
                {
                    highlightRectangle = this.EntityManager.GetServiceOfType<IHighlightRectangle>();
                }

                var boundingBox = foundControl.GetScreenRectangle().Value;
                if (boundingBox != Rectangle.Empty)
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

        public void GetClickablePoint(IControlIdentity controlDetails, out double x, out double y)
        {
            GetScreenBounds(controlDetails, out Rectangle bounds);
            controlDetails.GetClickablePoint(bounds, out x, out y);
        }

        public void GetScreenBounds(IControlIdentity controlIdentity, out Rectangle screenBounds)
        {
            var currentSearchRoot = GetSearchRoot(ref controlIdentity);    
            ConfigureRetryPolicy(controlIdentity);
            AccessibleContextNode targetControl = this.FindControl(controlIdentity, currentSearchRoot);
            screenBounds = GetBoundingBox(targetControl);
        }

        public Rectangle GetBoundingBox(object control)
        {
            Guard.Argument(control).NotNull().Compatible<AccessibleContextNode>();

            var controlNode = control as AccessibleContextNode;
            var boundingBox = controlNode.GetScreenRectangle().Value;
            return  boundingBox;
        }

        #endregion ICoordinateProvider        


        #region IDisposable

        protected virtual void Dispose(bool isDisposing)
        {
            if(isDisposing)
            {
                this.accessBridge?.Dispose();
                logger.Information("Access bridge is disposed now.");
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion IDisposable
    }
}
