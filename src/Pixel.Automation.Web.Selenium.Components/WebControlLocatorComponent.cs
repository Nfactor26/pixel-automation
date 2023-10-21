using Dawn;
using OpenQA.Selenium;
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components;

[DataContract]
[Serializable]
[ToolBoxItem("Selenium Locator", "Control Locators", iconSource: null, description: "Identify a web control on screen", tags: new string[] { "Locator" })]
public class WebControlLocatorComponent : ServiceComponent, IControlLocator, ICoordinateProvider
{
    private readonly ILogger logger = Log.ForContext<WebControlLocatorComponent>();
    private readonly RetryPolicy policy;
    private readonly List<TimeSpan> retrySequence = new();
    private IHighlightRectangle highlightRectangle;
    private int retryAttempts = 2;
    private double retryInterval = 5;

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

    /// <summary>
    /// Toggle if bounding box is shown during playback on controls.
    /// This can help visuall debug the control location process in hierarchy
    /// </summary>
    public bool ShowBoundingBox { get; set; }


    /// <summary>
    /// Contrsuctor      
    /// </summary>
    public WebControlLocatorComponent() : base("Selenium Control Locator", "SeleniumControlLocator")
    {
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
                logger.Debug("Control lookup  will be attempated again.");
            }
        });
    }

    public bool CanProcessControlOfType(IControlIdentity controlIdentity)
    {
        return controlIdentity is WebControlIdentity;
    }

    #region IControlLocator

    public async Task<UIControl> FindControlAsync(IControlIdentity controlIdentity, UIControl searchRoot = null)
    {
        Guard.Argument(controlIdentity).Compatible<WebControlIdentity>();

        var webControlIdentity = controlIdentity as WebControlIdentity;
        ISearchContext currentRoot = searchRoot?.GetApiControl<ISearchContext>() ?? WebDriver;
        SwitchToTargetFrame(webControlIdentity); //All nested controls must be in same frame.Hence, switch once at start
        while (true)
        {
            currentRoot = FindCurrentControl(webControlIdentity, currentRoot);

            if (webControlIdentity.Next != null)
            {
                webControlIdentity = webControlIdentity.Next as WebControlIdentity;
                continue;
            }
            logger.Debug("{0} has been located", controlIdentity);
            return await Task.FromResult(new WebUIControl(controlIdentity, currentRoot as IWebElement, this));
        }

    }

    private ISearchContext FindCurrentControl(WebControlIdentity webControlIdentity, ISearchContext currentRoot)
    {
        switch (webControlIdentity.SearchScope)
        {         
            case SearchScope.Descendants:
                if (webControlIdentity.Index > 1)
                {
                    var descendantControls = FindAllDescendantControls(webControlIdentity, currentRoot);
                    return GetWebElementAtConfiguredIndex(descendantControls, webControlIdentity);
                }
                else
                {
                    return FindDescendantControl(webControlIdentity, currentRoot);
                }               
            case SearchScope.Sibling:
                if (webControlIdentity.Index > 1)
                {
                    var siblingControls = FindAllSiblingControls(webControlIdentity, currentRoot);
                    return GetWebElementAtConfiguredIndex(siblingControls, webControlIdentity);
                }
                else
                {
                    return FindSiblingControl(webControlIdentity, currentRoot);
                }               
            case SearchScope.Ancestor:
                if (webControlIdentity.Index > 1)
                {
                    throw new NotSupportedException("There can be only one ancestor for a given control. Index based lookup is invalid in this context");
                }
                else
                {
                    return FindAncestorControl(webControlIdentity, currentRoot);
                }
            case SearchScope.Children:
            default:
                throw new NotSupportedException("SearchScope.Children is not supported by Web Control Locator");
        }

    }

    public async Task<IEnumerable<UIControl>> FindAllControlsAsync(IControlIdentity controlIdentity, UIControl searchRoot = null)
    {
        Guard.Argument(controlIdentity).NotNull().Compatible<WebControlIdentity>();
        ISearchContext currentRoot = searchRoot?.GetApiControl<ISearchContext>() ?? WebDriver;

        var webControlIdentity = controlIdentity as WebControlIdentity;
        SwitchToTargetFrame(webControlIdentity);
        while (true)
        {          
            if (webControlIdentity.Next != null)
            {
                currentRoot = FindCurrentControl(webControlIdentity, currentRoot);               
                webControlIdentity = webControlIdentity.Next as WebControlIdentity;
                continue;
            }

            IReadOnlyCollection<IWebElement> foundElements;
            switch (webControlIdentity.SearchScope)
            {
                case SearchScope.Descendants:
                    foundElements = FindAllElement(webControlIdentity, searchRoot?.GetApiControl<ISearchContext>() ?? WebDriver);
                    break;
                case SearchScope.Sibling:
                    foundElements = FindAllSiblingControls(webControlIdentity, searchRoot?.GetApiControl<ISearchContext>() ?? WebDriver);
                    break;
                case SearchScope.Children:
                case SearchScope.Ancestor:
                default:
                    throw new NotSupportedException($"Search scope : {webControlIdentity.SearchScope} is not supported for FindAllControls");
            }
            if (ShowBoundingBox)
            {
                foreach (var element in foundElements)
                {
                    HighlightElement(element);
                }
            }
            logger.Debug("{0} controls matching {0} has been located", foundElements.Count, controlIdentity);
            return await Task.FromResult(foundElements.Select(f => new WebUIControl(controlIdentity, f, this)));
        }
    }

    internal bool HasShadowRoot(ISearchContext searchContext)
    {
        var arguments = new List<object>
        {
            searchContext
        };
        var result = (this.WebDriver as IJavaScriptExecutor).ExecuteScript("return (arguments[0].shadowRoot != null)", arguments.ToArray());
        return (bool)result;
    }

    #region Descendant Control

    internal IWebElement FindDescendantControl(WebControlIdentity controlIdentity, ISearchContext searchRoot)
    {
        Guard.Argument(controlIdentity).NotNull();

        ConfigureRetryPolicy(controlIdentity);
        IWebElement foundControl = default;
        foundControl = policy.Execute(() =>
        {
            try
            {              
                foundControl = FindElement(controlIdentity, searchRoot);             
                return foundControl;
            }
            finally
            {
                HighlightElement(foundControl);
            }
        });
        return foundControl;
    }

    internal IReadOnlyCollection<IWebElement> FindAllDescendantControls(WebControlIdentity controlIdentity, ISearchContext searchRoot = null)
    {
        Guard.Argument(controlIdentity).NotNull(); ;

        ConfigureRetryPolicy(controlIdentity);       
        var foundControls = policy.Execute(() =>
        {
            return FindAllElement(controlIdentity, searchRoot);
        });
        return foundControls;
    }

    #endregion Descendant Control

    #region Ancestor Control

    internal IWebElement FindAncestorControl(WebControlIdentity controlIdentity, ISearchContext searchRoot)
    {
        Guard.Argument(controlIdentity).NotNull();
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
        }
        finally
        {
            HighlightElement(foundControl);
        }

        return foundControl;
    }

    #endregion Ancestor Control

    #region Sibling Control
    internal IWebElement FindSiblingControl(WebControlIdentity controlIdentity, ISearchContext searchRoot)
    {
        Guard.Argument(controlIdentity).NotNull();

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

    internal IReadOnlyCollection<IWebElement> FindAllSiblingControls(WebControlIdentity controlIdentity, ISearchContext searchRoot = null)
    {
        Guard.Argument(controlIdentity).NotNull();

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

        if (searchRoot is IWebElement webElement && HasShadowRoot(searchRoot))
        {
            searchRoot = webElement.GetShadowRoot();
        }

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

        if (searchRoot is IWebElement webElement && HasShadowRoot(searchRoot))
        {
            searchRoot = webElement.GetShadowRoot();
        }

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

    public async Task<(double, double)> GetClickablePoint(IControlIdentity controlDetails)
    {
        var bounds = await GetScreenBounds(controlDetails);
        controlDetails.GetClickablePoint(bounds, out double x, out double y);
        return await Task.FromResult((x, y));
    }

    public async Task<BoundingBox> GetScreenBounds(IControlIdentity controlDetails)
    {
        WebControlIdentity controlIdentity = controlDetails as WebControlIdentity;
        var targetControl = await this.FindControlAsync(controlIdentity, null);
        var screenBounds = await GetBoundingBox(targetControl.GetApiControl<IWebElement>());
        return screenBounds;
    }

    public async Task<BoundingBox> GetBoundingBox(object targetControl)
    {
        Guard.Argument(targetControl).NotNull().Compatible<IWebElement>();
        var result = CalculateBoundingBox(targetControl as IWebElement);
        return await Task.FromResult(result);
    }

    private BoundingBox CalculateBoundingBox(IWebElement webElement)
    {
        if (webElement.Displayed)
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

            dynamic boundingBox = (WebDriver as IJavaScriptExecutor).ExecuteScript(getScreenCoordinate, webElement);
            var screenBounds = new BoundingBox(Convert.ToInt32(boundingBox["left"]), Convert.ToInt32(boundingBox["top"]), Convert.ToInt32(boundingBox["width"]), Convert.ToInt32(boundingBox["height"]));
            return screenBounds;
        }
        throw new InvalidOperationException($"{webElement} is not visible.");
    }

    #endregion ICoordinateProvider    

    #region Switch To

    public void SwitchToWindow(int windowNumber)
    {
        IWebDriver webDriver = WebDriver;
        webDriver.SwitchTo().Window(webDriver.WindowHandles[windowNumber]);
        logger.Information("Web driver has been switched to window : {0}", windowNumber);
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
        logger.Information("Web driver has been switched to frame : {0}", frame);
    }

    #endregion Swith To

    #region Filter 

    protected IWebElement GetWebElementAtConfiguredIndex(IReadOnlyCollection<IWebElement> foundControls, WebControlIdentity webControlIdentity)
    {
        if (webControlIdentity.Index > 0)
        {
            int index = webControlIdentity.Index - 1;
            if (foundControls.Count() > index)
            {
                var foundControl = foundControls.ElementAt(index);
                HighlightElement(foundControl);
                return foundControl;
            }
            throw new IndexOutOfRangeException($"Found {foundControls.Count()} controls. Desired index : {index} is greater than number of found controls");
        }
        throw new InvalidOperationException($"Index doesn't have a valid value.");
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
                    if(el.toString() === \""[object ShadowRoot]\"")
                    {
                        el = el.parentElement || el.parentNode;
                    }
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
        if(webControlIdentity.FrameHierarchy?.Any() ?? false)
        {
            WebDriver.SwitchTo().DefaultContent();

            logger.Debug($"Web driver switched to default content.");

            foreach (var frame in webControlIdentity.FrameHierarchy)
            {
                SwitchToFrame(frame, webControlIdentity);                   
            }
        }
    }

    private void HighlightElement(IWebElement foundControl)
    {
        if (ShowBoundingBox && foundControl != null)
        {
            if (this.highlightRectangle == null)
            {
                this.highlightRectangle = this.EntityManager.GetServiceOfType<IHighlightRectangle>();
            }

            var boundingBox =  CalculateBoundingBox(foundControl);
            if (!BoundingBox.Empty.Equals(boundingBox))
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

    #endregion private methods
}
