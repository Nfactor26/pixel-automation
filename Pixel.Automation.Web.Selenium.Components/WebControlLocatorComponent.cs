using Dawn;
using OpenQA.Selenium;
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

namespace Pixel.Automation.Web.Selenium.Components
{
    [DataContract]
    [Serializable]
    public class WebControlLocatorComponent : ServiceComponent, IControlLocator<IWebElement, ISearchContext>, ICoordinateProvider
    {
        private readonly ILogger logger = Log.ForContext<WebControlLocatorComponent>();

        [RequiredComponent]
        [IgnoreDataMember]
        [Browsable(false)]
        public WebApplication ApplicationDetails
        {
            get
            {
                return this.EntityManager.GetOwnerApplication<WebApplication>(this);
            }
        }

        [IgnoreDataMember]
        [Browsable(false)]
        IWebDriver WebDriver
        {
            get
            {
                return ApplicationDetails.WebDriver;
            }
        }


        [NonSerialized]
        WebControlIdentity lastControlInteracted;


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

        [NonSerialized]
        private IHighlightRectangle highlightRectangle;

        [NonSerialized]
        int retryAttempts = 2;
        [Browsable(false)]
        [IgnoreDataMember]
        protected int RetryAttempts
        {
            get
            {
                return retryAttempts;
            }
            set
            {
                if (value == retryAttempts)
                {
                    return;
                }
                retryAttempts = value;
                retrySequence.Clear();
                foreach (var i in Enumerable.Range(1, value))
                {
                    retrySequence.Add(TimeSpan.FromSeconds(retryInterval));
                }
            }
        }

        [NonSerialized]
        double retryInterval = 5;
        [DataMember]
        [Browsable(false)]
        [IgnoreDataMember]
        protected double RetryInterval
        {
            get
            {
                return retryInterval;
            }
            set
            {
                if (value == retryInterval)
                {
                    return;
                }
                retryInterval = value;
                retrySequence.Clear();
                foreach (var i in Enumerable.Range(1, retryAttempts))
                {
                    retrySequence.Add(TimeSpan.FromSeconds(value));
                }
            }
        }

        [NonSerialized]
        private RetryPolicy policy;

        [NonSerialized]
        private List<TimeSpan> retrySequence;


        /// <summary>
        /// Contrsuctor      
        /// </summary>
        public WebControlLocatorComponent() : base("Web Control Locator", "SeleniumControlLocator")
        {
            retrySequence = new List<TimeSpan>();
            foreach (var i in Enumerable.Range(1, retryAttempts))
            {
                retrySequence.Add(TimeSpan.FromSeconds(retryInterval));
            }
            policy = Policy.Handle<WebDriverTimeoutException>()
            .Or<NoSuchElementException>().Or<ElementNotFoundException>()
            .WaitAndRetry(retrySequence, (exception, timeSpan, retryCount, context) =>
            {
                logger.Error(exception, exception.Message); ;
                if (retryCount < retrySequence.Count)
                {
                    logger.Information("Control lookup  will be attempated again.");
                }
            });
        }

        public bool CanProcessControlOfType(IControlIdentity controlIdentity)
        {
            return controlIdentity is WebControlIdentity;
        }

        #region IControlLocator

        public IWebElement FindControl(IControlIdentity controlIdentity, ISearchContext searchRoot = null)
        {
            Guard.Argument(controlIdentity).Compatible<WebControlIdentity>();

            IControlIdentity currentControl = controlIdentity;
            ISearchContext currentRoot = searchRoot ?? WebDriver;
            SwitchToTargetFrame(controlIdentity as WebControlIdentity); //All nested controls must be in same frame.Hence, switch once at start
            while (true)
            {
                WebControlIdentity webControlIdentity = currentControl as WebControlIdentity;
                switch (webControlIdentity.SearchScope)
                {
                    case SearchScope.Children:
                        throw new NotSupportedException("SearchScope.Children is not supported by Web Control Locator");
                    case SearchScope.Descendants:
                        if (webControlIdentity.Index.HasValue)
                        {
                            var descendantControls = FindAllDescendantControls(webControlIdentity, currentRoot);
                            currentRoot = GetWebElementAtConfiguredIndex(descendantControls, webControlIdentity);
                        }
                        else
                        {
                            currentRoot = FindDescendantControl(webControlIdentity, currentRoot);
                        }
                        break;
                    case SearchScope.Sibling:
                        if (webControlIdentity.Index.HasValue)
                        {
                            var siblingControls = FindAllSiblingControls(webControlIdentity, currentRoot);
                            currentRoot = GetWebElementAtConfiguredIndex(siblingControls, webControlIdentity);
                        }
                        else
                        {
                            currentRoot = FindSiblingControl(webControlIdentity, currentRoot);
                        }
                        break;
                    case SearchScope.Ancestor:
                        if (webControlIdentity.Index.HasValue)
                        {
                            throw new NotSupportedException("There can be only one ancestor for a given control. Index based lookup is invalid in this context");
                        }
                        else
                        {
                            currentRoot = FindAncestorControl(webControlIdentity, currentRoot);
                        }
                        break;
                }

                if (webControlIdentity.Next != null)
                {
                    currentControl = webControlIdentity.Next;
                    continue;
                }

                return currentRoot as IWebElement;
            }

        }

        public IEnumerable<IWebElement> FindAllControls(IControlIdentity controlIdentity, ISearchContext searchRoot = null)
        {
            Guard.Argument(controlIdentity).Compatible<WebControlIdentity>();

            WebControlIdentity webControlIdentity = controlIdentity as WebControlIdentity;
            SwitchToTargetFrame(webControlIdentity);

            switch (webControlIdentity.SearchScope)
            {
                case SearchScope.Descendants:
                    var foundElements = FindAllElement(controlIdentity as WebControlIdentity, searchRoot ?? WebDriver);
                    return foundElements;
                case SearchScope.Sibling:
                    var siblingElements = FindAllSiblingControls(controlIdentity, searchRoot ?? WebDriver);
                    return siblingElements;
                case SearchScope.Children:
                case SearchScope.Ancestor:
                default:
                    throw new NotSupportedException($"Search scope : {webControlIdentity.SearchScope} is not supported for FindAllControls");
            }

        }

        #region Descendant Control

        public IWebElement FindDescendantControl(IControlIdentity controlIdentity, ISearchContext searchRoot)
        {
            Guard.Argument(controlIdentity).Compatible<WebControlIdentity>();

            ConfigureRetryPolicy(controlIdentity);
            IWebElement foundControl = default;
            foundControl = policy.Execute(() =>
            {
                try
                {
                    WebControlIdentity webControlIdentity = controlIdentity as WebControlIdentity;
                    foundControl = FindElement(webControlIdentity, searchRoot);
                    logger.Information($"{webControlIdentity} has been located");
                    return foundControl;
                }
                finally
                {
                    HighlightElement(foundControl);
                }
            });
            return foundControl;
        }

        public IReadOnlyCollection<IWebElement> FindAllDescendantControls(IControlIdentity controlIdentity, ISearchContext searchRoot = null)
        {
            Guard.Argument(controlIdentity).Compatible<WebControlIdentity>();

            ConfigureRetryPolicy(controlIdentity);
            WebControlIdentity webControlIdentity = controlIdentity as WebControlIdentity;
            Debug.Assert(controlIdentity != null && webControlIdentity != null, "WebControlIdentity is null");
            var foundControls = policy.Execute(() =>
            {
                return FindAllElement(webControlIdentity, searchRoot);
            });
            return foundControls;
        }

        #endregion Descendant Control

        #region Ancestor Control

        public IWebElement FindAncestorControl(IControlIdentity controlIdentity, ISearchContext searchRoot)
        {
            Guard.Argument(controlIdentity).Compatible<WebControlIdentity>();
            ConfigureRetryPolicy(controlIdentity);
            IWebElement foundControl = default;
            try
            {
                WebControlIdentity webControlIdentity = controlIdentity as WebControlIdentity;
                Debug.Assert(controlIdentity != null && webControlIdentity != null, "WebControlIdentity is null");

                string identifier = webControlIdentity.Identifier;
                int searchTimeout = webControlIdentity.SearchTimeout;

                switch (webControlIdentity.FindByStrategy)
                {
                    case "Id":
                    case "ClassName":
                    case "Name":
                    case "CssSelector":
                    case "TagName":
                        string ancestorSelector = ConvertLookupToSelector(webControlIdentity.FindByStrategy, identifier);
                        foundControl = policy.Execute(() =>
                        {
                            IJavaScriptExecutor jsExecutor = WebDriver as IJavaScriptExecutor;
                            return new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).
                             Until(p => { return jsExecutor.ExecuteScript(this.ancestorLookupScript, searchRoot, ancestorSelector) as IWebElement; });
                        });
                        break;
                    case "XPath":
                        foundControl = FindElement(webControlIdentity, searchRoot);
                        break;
                    case "LinkText":
                    case "PartialLinkText":
                    default:
                        throw new InvalidOperationException($"SearchScope : Ancestor is not supported for FindBy modes [LinkText,PartialLinkText]");
                }

                logger.Information($"{webControlIdentity} has been located");
            }
            finally
            {
                HighlightElement(foundControl);
            }

            return foundControl;
        }

        #endregion Ancestor Control

        #region Sibling Control
        public IWebElement FindSiblingControl(IControlIdentity controlIdentity, ISearchContext searchRoot)
        {
            Guard.Argument(controlIdentity).Compatible<WebControlIdentity>();

            IWebElement foundControl = default;
            try
            {
                WebControlIdentity webControlIdentity = controlIdentity as WebControlIdentity;
                Debug.Assert(controlIdentity != null && webControlIdentity != null, "WebControlIdentity is null");
                var foundControls = FindAllSiblingControls(controlIdentity, searchRoot);
                if (foundControls.Count() > 1)
                {
                    throw new ArgumentException("Found more than one Sibling Control.Consider using LookType.FindAll instead of LookType.FindSingle");
                }
                foundControl = foundControls.Single();
            }
            finally
            {
                HighlightElement(foundControl);
            }
            return foundControl;
        }

        public IReadOnlyCollection<IWebElement> FindAllSiblingControls(IControlIdentity controlIdentity, ISearchContext searchRoot = null)
        {
            Guard.Argument(controlIdentity).Compatible<WebControlIdentity>();

            ConfigureRetryPolicy(controlIdentity);
            WebControlIdentity webControlIdentity = controlIdentity as WebControlIdentity;
            string identifier = webControlIdentity.Identifier;
            int searchTimeout = webControlIdentity.SearchTimeout;
            switch (webControlIdentity.FindByStrategy)
            {
                case "Id":
                case "ClassName":
                case "Name":
                case "CssSelector":
                case "TagName":
                    var foundControls = policy.Execute(() =>
                    {
                        string siblingSelector = ConvertLookupToSelector(webControlIdentity.FindByStrategy, identifier);
                        IJavaScriptExecutor jsExecutor = WebDriver as IJavaScriptExecutor;
                        return new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).
                        Until(p =>
                        {
                            return jsExecutor.ExecuteScript(this.siblingLookupScript, searchRoot, siblingSelector)
                                    as IReadOnlyCollection<IWebElement>;
                        });
                    });
                    return foundControls;
                case "XPath":
                    return FindAllElement(webControlIdentity, searchRoot);
                case "LinkText":
                case "PartialLinkText":
                default:
                    throw new NotSupportedException($"SearchScope : Sibling is not supported for FindBy modes [LinkText,PartialLinkText]");
            }
        }

        #endregion Sibling Control

        #region Find Control

        private IWebElement FindElement(WebControlIdentity webControlIdentity, ISearchContext searchRoot)
        {
            IWebElement foundControl = default;

            string identifier = webControlIdentity.Identifier;
            int searchTimeout = webControlIdentity.SearchTimeout;

            switch (webControlIdentity.FindByStrategy)
            {
                case "Id":
                    foundControl = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElement(By.Id(identifier)); });
                    break;
                case "ClassName":
                    foundControl = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElement(By.ClassName(identifier)); });
                    break;
                case "CssSelector":
                    foundControl = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElement(By.CssSelector(identifier)); });
                    break;
                case "LinkText":
                    foundControl = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElement(By.LinkText(identifier)); });
                    break;
                case "Name":
                    foundControl = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElement(By.Name(identifier)); });
                    break;
                case "PartialLinkText":
                    foundControl = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElement(By.PartialLinkText(identifier)); });
                    break;
                case "TagName":
                    foundControl = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElement(By.TagName(identifier)); });
                    break;
                case "XPath":
                    foundControl = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElement(By.XPath(identifier)); });
                    break;
                default:
                    throw new NotSupportedException(string.Format("FindBy strategy {0} for locating web control  is not supported", webControlIdentity.FindByStrategy.ToString()));
            }
            return foundControl;
        }

        private ReadOnlyCollection<IWebElement> FindAllElement(WebControlIdentity webControlIdentity, ISearchContext searchRoot)
        {
            ReadOnlyCollection<IWebElement> foundControls = new ReadOnlyCollection<IWebElement>(new List<IWebElement>());

            string identifier = webControlIdentity.Identifier;
            int searchTimeout = webControlIdentity.SearchTimeout;

            switch (webControlIdentity.FindByStrategy)
            {
                case "Id":
                    foundControls = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElements(By.Id(identifier)); });
                    break;
                case "ClassName":
                    foundControls = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElements(By.ClassName(identifier)); });
                    break;
                case "CssSelector":
                    foundControls = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElements(By.CssSelector(identifier)); });
                    break;
                case "LinkText":
                    foundControls = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElements(By.LinkText(identifier)); });
                    break;
                case "Name":
                    foundControls = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElements(By.Name(identifier)); });
                    break;
                case "PartialLinkText":
                    foundControls = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElements(By.PartialLinkText(identifier)); });
                    break;
                case "TagName":
                    foundControls = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElements(By.TagName(identifier)); });
                    break;
                case "XPath":
                    foundControls = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElements(By.XPath(identifier)); });
                    break;
                default:
                    throw new NotSupportedException(string.Format("FindBy strategy {0} for locating web control  is not supported", webControlIdentity.FindByStrategy.ToString()));
            }

            return foundControls;
        }

        #endregion Find Control

        #endregion IControlLocator

        #region ICoordinateProvider      

        public void GetClickablePoint(IControlIdentity controlDetails, out double x, out double y)
        {
            GetScreenBounds(controlDetails, out Rectangle bounds);
            controlDetails.GetClickablePoint(bounds, out x, out y);
        }

        public void GetScreenBounds(IControlIdentity controlDetails, out Rectangle screenBounds)
        {
            WebControlIdentity controlIdentity = controlDetails as WebControlIdentity;
            IWebElement targetControl = this.FindControl(controlIdentity, WebDriver);
            screenBounds = GetBoundingBox(targetControl);
        }

        public Rectangle GetBoundingBox(object targetControl)
        {
            Guard.Argument(targetControl).NotNull().Compatible<IWebElement>();

            IWebElement webControl = targetControl as IWebElement;
            if (webControl.Displayed)
            {
                string getScreenCoordinate = @"
                       
                            var targetElem = arguments[0];
                            var elemBounds = targetElem.getBoundingClientRect();
                            var screenBounds = {left:elemBounds.left, top : elemBounds.top, width: elemBounds.right- elemBounds.left, height : elemBounds.bottom - elemBounds.top};
                            var currentWindow = window;
                            while(currentWindow!=window.top)
                            {
                               var frameBounds =   currentWindow.frameElement.getBoundingClientRect();
                               screenBounds.left += frameBounds.left;
                               screenBounds.top += frameBounds.top;
                               currentWindow = currentWindow.parent;
                            }
                            screenBounds.left += window.top.screenX == 0 ? window.top.screenX : window.top.screenX + 7;  
                            screenBounds.top += window.top.screenY == 0 ? window.top.screenY : window.screenY - 7;
                            screenBounds.top += (window.top.outerHeight - window.top.innerHeight);
                            return screenBounds;                                  

                    ";

                dynamic boundingBox = (WebDriver as IJavaScriptExecutor).ExecuteScript(getScreenCoordinate, webControl);
                Rectangle screenBounds = new Rectangle(Convert.ToInt32(boundingBox["left"]), Convert.ToInt32(boundingBox["top"]), Convert.ToInt32(boundingBox["width"]), Convert.ToInt32(boundingBox["height"]));
                return screenBounds;
            }
            throw new InvalidOperationException($"{webControl.ToString()} is not visible.");

        }

        #endregion ICoordinateProvider    

        #region Switch To

        public void SwitchToWindow(int windowNumber)
        {
            IWebDriver webDriver = WebDriver;
            webDriver.SwitchTo().Window(webDriver.WindowHandles[windowNumber]);
        }

        public void SwitchToFrame(FrameIdentity frame, WebControlIdentity webControlIdentity)
        {
            IWebDriver webDriver = WebDriver;
            string findBy = frame.FindByStrategy;
            string identity = frame.Identifier;
            switch (findBy)
            {
                case "Id":
                case "Name":
                    webDriver.SwitchTo().Frame(identity);
                    break;
                case "Index":
                    webDriver.SwitchTo().Frame(int.Parse(identity));
                    break;
                case "CssSelector":
                case "XPath":
                    IWebElement frameElement = this.FindDescendantControl(new WebControlIdentity()
                    {
                        FindByStrategy = findBy,
                        Identifier = identity,
                        RetryAttempts = webControlIdentity.RetryAttempts,
                        RetryInterval = webControlIdentity.RetryInterval,
                        SearchTimeout = webControlIdentity.SearchTimeout
                    }, null);
                    webDriver.SwitchTo().Frame(frameElement);
                    break;
                default:
                    throw new ArgumentException($"Find by strategy {findBy} is not supported for frames");
            }
            logger.Information($"Web driver has been switched to frame : {frame}");
        }

        #endregion Swith To

        #region Filter 

        protected IWebElement GetWebElementAtConfiguredIndex(IReadOnlyCollection<IWebElement> foundControls, WebControlIdentity webControlIdentity)
        {
            if (webControlIdentity.Index.HasValue)
            {
                int index = webControlIdentity.Index.Value; ;
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

        private readonly string siblingLookupScript =
             @"function FindSiblings(el, siblingSelector)
              {
                var siblings = [];
                if (!Element.prototype.matches)
                    Element.prototype.matches = Element.prototype.msMatchesSelector || Element.prototype.webkitMatchesSelector;
                var parentElement = el.parentElement || el.parentNode;
                var children = parentElement.children;
                for (c of children)
                {
                    if (c != el && c.matches(siblingSelector))
                        siblings.push(c);
                }
                return siblings;
             }
             return FindSiblings(arguments[0],arguments[1]);";

        private readonly string ancestorLookupScript =
            @"function FindAncestor(el, ancestorSelector)
              {               
                if (!Element.prototype.matches)
                    Element.prototype.matches = Element.prototype.msMatchesSelector || Element.prototype.webkitMatchesSelector;
                do
                {
                    if (el.matches(ancestorSelector)) return el;
                        el = el.parentElement || el.parentNode;
                } 
                while (el !== null && el.nodeType === 1);
                
                return null;
              }
              return FindAncestor(arguments[0],arguments[1]);";

        private string ConvertLookupToSelector(string lookupKey, string lookupValue)
        {
            switch (lookupKey)
            {
                case "Id":
                    return $"#{lookupValue}";
                case "ClassName":
                    return $".{lookupValue}";
                case "Name":
                    return $"[name='{lookupValue}']";
                case "CssSelector":
                case "TagName":
                    return lookupValue;
                case "XPath":
                case "LinkText":
                case "PartialLinkText":
                default:
                    throw new InvalidOperationException($"{lookupKey} with lookup value : {lookupValue} can't be converted to css selector");
            }
        }

        private void SwitchToTargetFrame(WebControlIdentity webControlIdentity)
        {
            var lastControlFrameDetails = lastControlInteracted?.FrameHierarchy;

            //switch to frame only if frame details have changed between last control and current control 
            if (lastControlFrameDetails != null && !webControlIdentity.FrameHierarchy.SequenceEqual(lastControlFrameDetails))
            {
                WebDriver.SwitchTo().DefaultContent();

                Log.Information($"Web driver switched to default content.");

                foreach (var frame in webControlIdentity.FrameHierarchy)
                {
                    SwitchToFrame(frame, webControlIdentity);
                }
            }
            lastControlInteracted = webControlIdentity;
        }

        private void HighlightElement(IWebElement foundControl)
        {
            if (showBoundingBox && foundControl != null)
            {
                if (this.highlightRectangle == null)
                {
                    this.highlightRectangle = this.EntityManager.GetServiceOfType<IHighlightRectangle>();
                }

                Rectangle boundingBox = GetBoundingBox(foundControl);
                if (boundingBox != Rectangle.Empty)
                {
                    highlightRectangle.Visible = true;

                    highlightRectangle.Location = boundingBox;
                    Thread.Sleep(500);

                    highlightRectangle.Visible = false;

                }

            }
        }

        private void ConfigureRetryPolicy(IControlIdentity controlIdentity)
        {
            RetryAttempts = controlIdentity.RetryAttempts;
            RetryInterval = controlIdentity.RetryInterval;
        }

        #endregion private methods
    }
}
