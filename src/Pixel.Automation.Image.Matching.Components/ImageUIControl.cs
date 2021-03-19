using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System.Drawing;

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
        }

        public override Rectangle GetBoundingBox()
        {
            return new Rectangle(imageControl.X, imageControl.Y, imageControl.Width, imageControl.Height);
        }

        public override void GetClickablePoint(out double x, out double y)
        {
            var boundingBox = GetBoundingBox();
            controlIdentity.GetClickablePoint(boundingBox, out x, out y);
        }
    }
}
