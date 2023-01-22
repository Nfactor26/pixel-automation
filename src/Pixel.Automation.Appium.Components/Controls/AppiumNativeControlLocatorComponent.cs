using Dawn;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
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
using System.Runtime.Serialization;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// Implementation of <see cref="IControlLocator"/> and <see cref="ICoordinateProvider"/> to locate a native control using appium.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Appium Native Locator", "Control Locators", iconSource: null, description: "Identify a native mobile control on screen", tags: new string[] { "Locator" })]
public class AppiumNativeControlLocatorComponent : ServiceComponent, IControlLocator, ICoordinateProvider
{
    private readonly ILogger logger = Log.ForContext<AppiumNativeControlLocatorComponent>();
    private readonly RetryPolicy policy;
    private readonly List<TimeSpan> retrySequence = new();    
    private int retryAttempts = 2;
    private double retryInterval = 5;

    /// <summary>
    /// Constructor      
    /// </summary>
    public AppiumNativeControlLocatorComponent() : base("Appium Control Locator", "AppiumControlLocator")
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
                logger.Information("Control lookup  will be attempated again.");
            }
        });
    }

    /// <summary>
    /// Retrieve the <see cref="AppiumDriver"/> from the owner application
    /// </summary>
    /// <returns></returns>
    protected AppiumDriver GetAppiumDriver()
    {
        var ownerApplication = this.EntityManager.GetOwnerApplication<AppiumApplication>(this);
        return ownerApplication.Driver;
    }

    /// <summary>
    /// Create an <see cref="AppiumUIControl"/> that wraps up an <see cref="AppiumElement"/>
    /// </summary>
    /// <param name="controlIdentity"></param>
    /// <param name="appiumElement"></param>
    /// <param name="coordinateProvider"></param>
    /// <returns></returns>
    protected UIControl CreateUIControl(IControlIdentity controlIdentity, AppiumElement appiumElement, ICoordinateProvider coordinateProvider)
    {
        return new AppiumUIControl(controlIdentity, appiumElement, coordinateProvider);
    }

    /// </inheritdoc>
    public bool CanProcessControlOfType(IControlIdentity controlIdentity)
    {
        return controlIdentity is AppiumNativeControlIdentity;
    }

    #region IControlLocator

    /// </inheritdoc>
    public async Task<UIControl> FindControlAsync(IControlIdentity controlIdentity, UIControl searchRoot = null)
    {
        Guard.Argument(controlIdentity).Compatible<AppiumNativeControlIdentity>();

        var appiumControlIdentity = controlIdentity as AppiumNativeControlIdentity;
        ISearchContext currentRoot = searchRoot?.GetApiControl<ISearchContext>() ?? GetAppiumDriver();      
        while (true)
        {
            currentRoot = FindCurrentControl(appiumControlIdentity, currentRoot);

            if (appiumControlIdentity.Next != null)
            {
                appiumControlIdentity = appiumControlIdentity.Next as AppiumNativeControlIdentity;
                continue;
            }

            return await Task.FromResult(CreateUIControl(controlIdentity, currentRoot as AppiumElement, this));
        }

    }

    private ISearchContext FindCurrentControl(AppiumNativeControlIdentity controlIdentity, ISearchContext currentRoot)
    {
        switch (controlIdentity.SearchScope)
        {
            case SearchScope.Descendants:
                if (controlIdentity.Index > 1)
                {
                    var descendantControls = FindAllDescendantControls(controlIdentity, currentRoot);
                    return GetWebElementAtConfiguredIndex(descendantControls, controlIdentity);
                }
                else
                {
                    return FindDescendantControl(controlIdentity, currentRoot);
                }
            case SearchScope.Sibling:              
            case SearchScope.Ancestor:                
            case SearchScope.Children:
            default:
                throw new NotSupportedException("SearchScope.Children is not supported by Web Control Locator");
        }

    }

    /// </inheritdoc>
    public async Task<IEnumerable<UIControl>> FindAllControlsAsync(IControlIdentity controlIdentity, UIControl searchRoot = null)
    {
        Guard.Argument(controlIdentity).NotNull().Compatible<AppiumNativeControlIdentity>();
        ISearchContext currentRoot = searchRoot?.GetApiControl<ISearchContext>() ?? GetAppiumDriver();

        var appiumControlIdentity = controlIdentity as AppiumNativeControlIdentity;      
        while (true)
        {
            if (appiumControlIdentity.Next != null)
            {
                currentRoot = FindCurrentControl(appiumControlIdentity, currentRoot);
                appiumControlIdentity = appiumControlIdentity.Next as AppiumNativeControlIdentity;
                continue;
            }

            try
            {
                IReadOnlyCollection<IWebElement> foundElements;
                switch (appiumControlIdentity.SearchScope)
                {
                    case SearchScope.Descendants:
                        foundElements = FindAllElement(appiumControlIdentity, searchRoot?.GetApiControl<ISearchContext>() ?? GetAppiumDriver());
                        break;
                    case SearchScope.Sibling:                        
                    case SearchScope.Children:
                    case SearchScope.Ancestor:
                    default:
                        throw new NotSupportedException($"Search scope : {appiumControlIdentity.SearchScope} is not supported for FindAllControls");
                }               
                return await Task.FromResult(foundElements.Select(f => CreateUIControl(controlIdentity, f as AppiumElement, this)));
            }
            finally
            {
                logger.Debug("Control lookup completed.");
            }

        }
    }

    #region Descendant Control
   
    /// <summary>
    /// Find a control which is a Descendant of current search context
    /// </summary>
    /// <param name="controlIdentity">Control being searched</param>
    /// <param name="searchRoot">Current root for the search</param>
    /// <returns></returns>
    internal IWebElement FindDescendantControl(AppiumNativeControlIdentity controlIdentity, ISearchContext searchRoot)
    {
        Guard.Argument(controlIdentity).NotNull();

        ConfigureRetryPolicy(controlIdentity);
        IWebElement foundControl = default;
        foundControl = policy.Execute(() =>
        {
            foundControl = FindElement(controlIdentity, searchRoot);
            logger.Information($"{controlIdentity} has been located");
            return foundControl;
        });
        return foundControl;
    }
 
    /// <summary>
    /// Find all controls which are descendant of current search context
    /// </summary>
    /// <param name="controlIdentity">Control being searched</param>
    /// <param name="searchRoot">Current root for the search</param>
    /// <returns></returns>
    internal IReadOnlyCollection<IWebElement> FindAllDescendantControls(AppiumNativeControlIdentity controlIdentity, ISearchContext searchRoot = null)
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
  
    #region Find Control

    /// <summary>
    /// Find element matching given criteria
    /// </summary>
    /// <param name="controlIdentity"></param>
    /// <param name="searchRoot"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    private IWebElement FindElement(AppiumNativeControlIdentity controlIdentity, ISearchContext searchRoot)
    {
        IWebElement foundControl = default;

        string identifier = controlIdentity.Identifier;
        int searchTimeout = controlIdentity.SearchTimeout;

        switch (controlIdentity.FindByStrategy)
        {
            case "Id":
                foundControl = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElement(MobileBy.Id(identifier)); });
                break;
            case "Name":
                foundControl = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElement(MobileBy.Name(identifier)); });
                break;          
            case "ClassName":
                foundControl = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElement(MobileBy.ClassName(identifier)); });
                break;
            case "Accessibility Id":
                foundControl = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElement(MobileBy.AccessibilityId(identifier)); });
                break;
            case "Android UI Automator":
                foundControl = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElement(MobileBy.AndroidUIAutomator(identifier)); });
                break;
            case "Android Data Matcher":
                foundControl = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElement(MobileBy.AndroidDataMatcher(identifier)); });
                break;
            case "Android View Matcher":
                foundControl = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElement(MobileBy.AndroidViewMatcher(identifier)); });
                break;
            case "IOS UI Automation":
                foundControl = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElement(MobileBy.IosUIAutomation(identifier)); });
                break;           
            case "XPath":
                foundControl = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElement(By.XPath(identifier)); });
                break;           
            default:
                throw new NotSupportedException(string.Format("FindBy strategy {0} for locating web control  is not supported", controlIdentity.FindByStrategy.ToString()));
        }
        return foundControl;
    }

    /// <summary>
    /// Find all elements matching given criteria
    /// </summary>
    /// <param name="controlIdentity"></param>
    /// <param name="searchRoot"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    private ReadOnlyCollection<IWebElement> FindAllElement(AppiumNativeControlIdentity controlIdentity, ISearchContext searchRoot)
    {
        ReadOnlyCollection<IWebElement> foundControls = new ReadOnlyCollection<IWebElement>(new List<IWebElement>());

        string identifier = controlIdentity.Identifier;
        int searchTimeout = controlIdentity.SearchTimeout;    

        switch (controlIdentity.FindByStrategy)
        {
            case "Id":
                foundControls = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElements(MobileBy.Id(identifier)); });
                break;
            case "Name":
                foundControls = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElements(MobileBy.Name(identifier)); });
                break;           
            case "ClassName":
                foundControls = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElements(MobileBy.ClassName(identifier)); });
                break;
            case "Accessibility Id":
                foundControls = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElements(MobileBy.AccessibilityId(identifier)); });
                break;
            case "Android UI Automator":
                foundControls = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElements(MobileBy.AndroidUIAutomator(identifier)); });
                break;
            case "Android Data Matcher":
                foundControls = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElements(MobileBy.AndroidDataMatcher(identifier)); });
                break;
            case "Android View Matcher":
                foundControls = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElements(MobileBy.AndroidViewMatcher(identifier)); });
                break;
            case "Ios UI Automation":
                foundControls = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElements(MobileBy.IosUIAutomation(identifier)); });
                break;
            case "XPath":
                foundControls = new WebElementWait(searchRoot, TimeSpan.FromSeconds(searchTimeout)).Until(p => { return p.FindElements(By.XPath(identifier)); });
                break;       
            default:
                throw new NotSupportedException(string.Format("FindBy strategy {0} for locating web control  is not supported", controlIdentity.FindByStrategy.ToString()));
        }

        return foundControls;
    }

    #endregion Find Control

    #endregion IControlLocator

    #region ICoordinateProvider      

    /// </inheritdoc>
    public async Task<(double, double)> GetClickablePoint(IControlIdentity controlDetails)
    {
        var bounds = await GetScreenBounds(controlDetails);
        controlDetails.GetClickablePoint(bounds, out double x, out double y);
        return await Task.FromResult((x, y));
    }

    /// </inheritdoc>
    public async Task<BoundingBox> GetScreenBounds(IControlIdentity controlDetails)
    {
        AppiumNativeControlIdentity controlIdentity = controlDetails as AppiumNativeControlIdentity;
        var targetControl = await this.FindControlAsync(controlIdentity, null);
        var screenBounds = await GetBoundingBox(targetControl.GetApiControl<AppiumElement>());
        return screenBounds;
    }

    /// </inheritdoc>
    public async Task<BoundingBox> GetBoundingBox(object targetControl)
    {
        Guard.Argument(targetControl).NotNull().Compatible<IWebElement>();
        var result = CalculateBoundingBox(targetControl as AppiumElement);
        return await Task.FromResult(result);
    }

    private BoundingBox CalculateBoundingBox(AppiumElement targetControl)
    {
        if (targetControl.Displayed)
        {
            var controlRectangle = targetControl.Rect;
            var screenBounds = new BoundingBox(controlRectangle.Left, controlRectangle.Top, controlRectangle.Width, controlRectangle.Height);
            return screenBounds;
        }
        throw new InvalidOperationException($"{targetControl} is not visible.");
    }

    #endregion ICoordinateProvider    

    #region Filter 

    protected IWebElement GetWebElementAtConfiguredIndex(IReadOnlyCollection<IWebElement> foundControls, AppiumNativeControlIdentity controlIdentity)
    {
        if (controlIdentity.Index > 0)
        {
            int index = controlIdentity.Index - 1;
            if (foundControls.Count() > index)
            {
                var foundControl = foundControls.ElementAt(index);                
                return foundControl;
            }
            throw new IndexOutOfRangeException($"Found {foundControls.Count()} controls. Desired index : {index} is greater than number of found controls");
        }
        throw new InvalidOperationException($"Index doesn't have a valid value.");
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

    #endregion private methods
}
