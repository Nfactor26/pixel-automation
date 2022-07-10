using Dawn;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
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
    public ImageUIControl(IControlIdentity controlIdentity, BoundingBox imageControl)
    {
        this.controlIdentity = Guard.Argument(controlIdentity).NotNull().Value;          
        this.TargetControl = Guard.Argument(imageControl).NotNull().Value;
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
}
