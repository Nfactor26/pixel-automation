using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using System.Threading.Tasks;

namespace Pixel.Automation.Image.Matching.Components
{
    public class ImageUIControl : UIControl
    {
        private readonly IControlIdentity controlIdentity;
        private readonly BoundingBox imageControl;

        public ImageUIControl(IControlIdentity controlIdentity, BoundingBox imageControl)
        {
            this.controlIdentity = controlIdentity;
            this.imageControl = imageControl;        
            this.TargetControl = imageControl;
        }

        public override async Task<BoundingBox> GetBoundingBoxAsync()
        {
            return await Task.FromResult(new BoundingBox(imageControl.X, imageControl.Y, imageControl.Width, imageControl.Height));
        }

        public override async Task<(double,double)> GetClickablePointAsync()
        {
            var boundingBox = await GetBoundingBoxAsync();
            controlIdentity.GetClickablePoint(boundingBox, out double x, out double y);
            return await Task.FromResult((x, y));
        }
    }
}
