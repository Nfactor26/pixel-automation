using Dawn;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Pixel.Automation.Image.Matching.Components;

/// <summary>
/// ImageUI control represents a control image as a control using the bounding box of image on screen.
/// Bounding box is located using image matching.
/// </summary>
public class ImageUIControl : UIControl
{
    private readonly IControlIdentity controlIdentity;

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="controlIdentity">Details of the image control</param>
    /// <param name="imageControl">Bounding box of the image control</param>
    [SetsRequiredMembers]
    public ImageUIControl(IControlIdentity controlIdentity, BoundingBox imageControl)
    {
        Guard.Argument(controlIdentity, nameof(controlIdentity)).NotNull();
        Guard.Argument(imageControl, nameof(imageControl)).NotNull();

        this.controlIdentity = Guard.Argument(controlIdentity).NotNull().Value;          
        this.TargetControl = Guard.Argument(imageControl).NotNull().Value;
        this.ControlName = controlIdentity.Name;
    }

    ///<inheritdoc/>
    public override async Task<BoundingBox> GetBoundingBoxAsync()
    {
        var imageControl = this.TargetControl as BoundingBox;
        return await Task.FromResult(new BoundingBox(imageControl.X, imageControl.Y, imageControl.Width, imageControl.Height));
    }

    ///<inheritdoc/>
    public override async Task<(double,double)> GetClickablePointAsync()
    {
        var boundingBox = await GetBoundingBoxAsync();
        controlIdentity.GetClickablePoint(boundingBox, out double x, out double y);
        return (x, y);
    }

    ///<inheritdoc/>
    public override Task<bool> IsDisabledAsync()
    {
        throw new System.NotSupportedException("Image control doesn't support determinig if a control is disabled.");
    }

    ///<inheritdoc/>
    public override Task<bool> IsEnabledAsync()
    {
        throw new System.NotSupportedException("Image control doesn't support determining if a control is enabled.");
    }

    ///<inheritdoc/>
    public override Task<bool> IsHiddenAsync()
    {
        return Task.FromResult(false);
    }

    ///<inheritdoc/>
    public override Task<bool> IsVisibleAsync()
    {      
        return Task.FromResult(true);
    }

    ///<inheritdoc/>
    public override Task<bool> IsCheckedAsync()
    {
        throw new System.NotSupportedException("Image control doesn't support determining if a control is checked.");
    }
}
