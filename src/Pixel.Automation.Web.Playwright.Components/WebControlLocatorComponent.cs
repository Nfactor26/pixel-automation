using Dawn;
using Microsoft.Playwright;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

[DataContract]
[Serializable]
[ToolBoxItem("Playwright Locator", "Control Locators", iconSource: null, description: "Identify a web control on screen", tags: new string[] { "Locator" })]
public class WebControlLocatorComponent : ServiceComponent, IControlLocator, ICoordinateProvider
{
    private readonly ILogger logger = Log.ForContext<WebControlLocatorComponent>();

    [IgnoreDataMember]
    [Browsable(false)]
    public WebApplication ApplicationDetails
    {
        get
        {
            return this.EntityManager.GetOwnerApplication<WebApplication>(this);
        }
    }


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

  
    /// <summary>
    /// Contrsuctor      
    /// </summary>
    public WebControlLocatorComponent() : base("Playwright Control Locator", "PlaywrightControlLocator")
    {
      
    }


    public bool CanProcessControlOfType(IControlIdentity controlIdentity)
    {
        return controlIdentity is WebControlIdentity;
    }

    #region IControlLocator

    public async Task<UIControl> FindControlAsync(IControlIdentity controlIdentity, UIControl searchRoot = null)
    {
        Guard.Argument(controlIdentity).Compatible<WebControlIdentity>();

        IControlIdentity currentControl = controlIdentity;

        var currentRoot = searchRoot?.GetApiControl<ILocator>();
        while (true)
        {
            WebControlIdentity webControlIdentity = currentControl as WebControlIdentity;
            switch (webControlIdentity.SearchScope)
            {
                case SearchScope.Children:
                case SearchScope.Sibling:
                case SearchScope.Ancestor:
                    throw new NotSupportedException("SearchScope.Children is not supported by Web Control Locator");
                case SearchScope.Descendants:
                    if (webControlIdentity.Index > 1)
                    {
                        var descendantControls = await FindAllDescendantControls(controlIdentity, currentRoot);
                        currentRoot = GetWebElementAtConfiguredIndex(descendantControls, webControlIdentity);                       
                    }
                    else
                    {
                        currentRoot = FindDescendantControl(controlIdentity, currentRoot);
                    }
                    break;
            }

            if (webControlIdentity.Next != null)
            {
                currentControl = webControlIdentity.Next;
                continue;
            }
            await HighlightElement(currentRoot);
            return new WebUIControl(controlIdentity, currentRoot, this);
        }
    }

    public async Task<IEnumerable<UIControl>> FindAllControlsAsync(IControlIdentity controlIdentity, UIControl searchRoot = null)
    {
        Guard.Argument(controlIdentity).Compatible<WebControlIdentity>();

        WebControlIdentity webControlIdentity = controlIdentity as WebControlIdentity;
        var currentRoot = searchRoot?.GetApiControl<ILocator>();
        switch (webControlIdentity.SearchScope)
        {
            case SearchScope.Descendants:
                var descendantControls = await FindAllDescendantControls(controlIdentity, currentRoot);
                return await Task.FromResult(descendantControls.Select(f => new WebUIControl(controlIdentity, f, this)));
            case SearchScope.Sibling:          
            case SearchScope.Children:
            case SearchScope.Ancestor:
            default:
                throw new NotSupportedException($"Search scope : {webControlIdentity.SearchScope} is not supported for FindAllControls");
        }
    }

    #endregion IControlLocator

    #region Descendant Control

    public ILocator FindDescendantControl(IControlIdentity controlIdentity, ILocator searchRoot = null)
    {
        Guard.Argument(controlIdentity).Compatible<WebControlIdentity>();
        ILocator foundControl = default;
        WebControlIdentity webControlIdentity = controlIdentity as WebControlIdentity;
        if (searchRoot != null)
        {
            foundControl = searchRoot.Locator(webControlIdentity.Identifier);
        }
        else
        {
            if (webControlIdentity.FrameHierarchy.Any())
            {
                IFrameLocator frameLocator = GetTargetFrame(ApplicationDetails.ActivePage, webControlIdentity);
                foundControl = frameLocator.Locator(webControlIdentity.Identifier);
            }
            else
            {
                foundControl = ApplicationDetails.ActivePage.Locator(webControlIdentity.Identifier);
            }
        }
        logger.Information($"{webControlIdentity} has been located");
        return foundControl;
    }


    public async Task<IReadOnlyCollection<ILocator>> FindAllDescendantControls(IControlIdentity controlIdentity, ILocator searchRoot)
    {
        Guard.Argument(controlIdentity).Compatible<WebControlIdentity>();
       
        WebControlIdentity webControlIdentity = controlIdentity as WebControlIdentity;
        List<ILocator> locators = new List<ILocator>();
        if (searchRoot != null)
        {
            int count = await searchRoot.Locator(webControlIdentity.Identifier).CountAsync();
            for (int i = 0; i < count; i++)
            {
                locators.Add(searchRoot.Locator(webControlIdentity.Identifier).Nth(i));
            }
        }
        else
        {
            if (webControlIdentity.FrameHierarchy.Any())
            {
                IFrameLocator frameLocator = GetTargetFrame(ApplicationDetails.ActivePage, webControlIdentity);
                int count = await frameLocator.Locator(webControlIdentity.Identifier).CountAsync();
                for (int i = 0; i < count; i++)
                {
                    locators.Add(frameLocator.Locator(webControlIdentity.Identifier).Nth(i));
                }
            }
            else
            {
                int count = await ApplicationDetails.ActivePage.Locator(webControlIdentity.Identifier).CountAsync();
                for (int i = 0; i < count; i++)
                {
                    locators.Add(ApplicationDetails.ActivePage.Locator(webControlIdentity.Identifier).Nth(i));
                }
            }
        }
        return locators;       
    }

    #endregion Descendant Control

    #region Filter 

    protected ILocator GetWebElementAtConfiguredIndex(IReadOnlyCollection<ILocator> foundControls, WebControlIdentity webControlIdentity)
    {
        if (webControlIdentity.Index > 0)
        {
            int index = webControlIdentity.Index - 1;
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

    #region ICoordinateProvider      

    public async Task<(double, double)> GetClickablePoint(IControlIdentity controlDetails)
    {
        var bounds = await GetScreenBounds(controlDetails);
        controlDetails.GetClickablePoint(bounds, out double x, out double y);
        return (x, y);
    }

    public async Task<BoundingBox> GetScreenBounds(IControlIdentity controlDetails)
    {
        WebControlIdentity controlIdentity = controlDetails as WebControlIdentity;
        var targetControl = await this.FindControlAsync(controlIdentity, null);
        var screenBounds = await GetBoundingBox(targetControl.GetApiControl<ILocator>());
        return screenBounds;
    }

    public async Task<BoundingBox> GetBoundingBox(object control)
    {
        if (control is ILocator locator)
        {
            var boundingBox = await locator.BoundingBoxAsync(new LocatorBoundingBoxOptions() { Timeout = 10 });
            if (boundingBox != null)
            {
                return new BoundingBox(Convert.ToInt32(boundingBox.X), Convert.ToInt32(boundingBox.Y), Convert.ToInt32(boundingBox.Width), Convert.ToInt32(boundingBox.Height));
            }
            logger.Warning("Bounding box could not be retrieved. Return empty bounding box.");
            return BoundingBox.Empty;
        }
        throw new ArgumentException("control must be of type ILocator");
    }

    #endregion ICoordinateProvider    

    private IFrameLocator GetTargetFrame(IPage page, WebControlIdentity webControlIdentity)
    {
        if (webControlIdentity.FrameHierarchy.Any())
        {
            var frameLocator = page.FrameLocator(webControlIdentity.FrameHierarchy.First().Identifier);
            foreach (var frameIdentity in webControlIdentity.FrameHierarchy.Skip(1))
            {
                frameLocator = frameLocator.FrameLocator(frameIdentity.Identifier);
            }
            return frameLocator;
        }
        throw new ArgumentException($"No frames configured for {webControlIdentity}");
    }


    private async Task HighlightElement(ILocator foundControl)
    {
        if (showBoundingBox && foundControl != null)
        {
            if (this.highlightRectangle == null)
            {
                this.highlightRectangle = this.EntityManager.GetServiceOfType<IHighlightRectangle>();
            }

            var boundingBox = await GetBoundingBox(foundControl);
            if (!BoundingBox.Empty.Equals(boundingBox))
            {
                highlightRectangle.Visible = true;

                highlightRectangle.Location = boundingBox;
                Thread.Sleep(500);

                highlightRectangle.Visible = false;

            }
        }
    }

}
