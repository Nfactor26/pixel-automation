using Dawn;
using OpenQA.Selenium.Appium;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Pixel.Automation.Appium.Components;

/// <summary>
/// UIControl wrapper for <see cref="AppiumElement"/>
/// </summary>
public class AppiumUIControl : UIControl
{
    protected readonly ICoordinateProvider coordinateProvider;
    protected readonly IControlIdentity controlIdentity;
    protected readonly AppiumElement appiumElement;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="controlIdentity">Control details</param>
    /// <param name="appiumElement">AppiumElement</param>
    /// <param name="coordinateProvider">Coordinate provider</param>
    [SetsRequiredMembers]
    public AppiumUIControl(IControlIdentity controlIdentity, AppiumElement appiumElement, ICoordinateProvider coordinateProvider)
    {
        Guard.Argument(controlIdentity, nameof(controlIdentity)).NotNull();
        Guard.Argument(appiumElement, nameof(appiumElement)).NotNull();
        Guard.Argument(coordinateProvider, nameof(coordinateProvider)).NotNull();

        this.controlIdentity = controlIdentity;
        this.coordinateProvider = coordinateProvider;
        this.appiumElement = appiumElement;
        this.TargetControl = appiumElement;
        this.ControlName = controlIdentity.Name;
    }

    ///<inheritdoc/>
    public override async Task<BoundingBox> GetBoundingBoxAsync()
    {
        var boundingBox = await coordinateProvider.GetBoundingBox(appiumElement);
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
        return Task.FromResult(!this.appiumElement.Enabled);
    }

    ///<inheritdoc/>
    public override Task<bool> IsEnabledAsync()
    {
        return Task.FromResult(this.appiumElement.Enabled);
    }

    ///<inheritdoc/>
    public override Task<bool> IsHiddenAsync()
    {
        return Task.FromResult(!this.appiumElement.Displayed);
    }

    ///<inheritdoc/>
    public override Task<bool> IsVisibleAsync()
    {
        return Task.FromResult(this.appiumElement.Displayed);
    }

    ///<inheritdoc/>
    public override Task<bool> IsCheckedAsync()
    {
        return Task.FromResult(this.appiumElement.Selected);
    }
}