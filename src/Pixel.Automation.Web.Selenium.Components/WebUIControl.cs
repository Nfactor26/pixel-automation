using Dawn;
using OpenQA.Selenium;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.Web.Selenium.Components;

/// <summary>
/// WebUI control wraps a IWebElement while providing some useful functionality on top of it.
/// </summary>
public class WebUIControl : UIControl
{
    private readonly ICoordinateProvider coordinateProvider; 
    private readonly IControlIdentity controlIdentity;
    private readonly IWebElement webElement;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="controlIdentity">Control details</param>
    /// <param name="webElement">IWebElement</param>
    /// <param name="coordinateProvider">Coordinate provider</param>
    public WebUIControl(IControlIdentity controlIdentity, IWebElement webElement, ICoordinateProvider coordinateProvider)
    {
        Guard.Argument(controlIdentity).NotNull();
        Guard.Argument(webElement).NotNull();
        Guard.Argument(coordinateProvider).NotNull();

        this.controlIdentity = controlIdentity;
        this.coordinateProvider = coordinateProvider;
        this.webElement = webElement;
        this.TargetControl = webElement;
    }

    ///<inheritdoc/>
    public override async Task<BoundingBox> GetBoundingBoxAsync()
    {
        //webdriver imlementation for location doesn't consider browser window titlebar space resulting in to incorrect y-coodrinate.
        //Hence, custom implementation          
        //var boundingBox = new Rectangle(webElement.Location, webElement.Size);
        //return boundingBox;
        var boundingBox = await coordinateProvider.GetBoundingBox(webElement);
        return boundingBox;
    }

    ///<inheritdoc/>
    public override async Task<(double, double)> GetClickablePointAsync()
    {
        var boundingBox = await GetBoundingBoxAsync();
        controlIdentity.GetClickablePoint(boundingBox, out double x, out double y);
        return await Task.FromResult((x, y));
    }

    ///<inheritdoc/>
    public override Task<bool> IsDisabledAsync()
    {
        return Task.FromResult(!this.webElement.Enabled);
    }

    ///<inheritdoc/>
    public override Task<bool> IsEnabledAsync()
    {
        return Task.FromResult(this.webElement.Enabled);
    }

    ///<inheritdoc/>
    public override Task<bool> IsHiddenAsync()
    {
        return Task.FromResult(!this.webElement.Displayed);
    }

    ///<inheritdoc/>
    public override Task<bool> IsVisibleAsync()
    {
        return Task.FromResult(this.webElement.Displayed);
    }

    ///<inheritdoc/>
    public override Task<bool> IsCheckedAsync()
    {
        return Task.FromResult(this.webElement.Selected);
    }
}
