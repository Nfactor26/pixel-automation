using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System.Drawing;
using System.Windows.Automation;

namespace Pixel.Automation.UIA.Components
{
    public class WinUIControl : UIControl
    {     
       
        private readonly IControlIdentity controlIdentity;

        public WinUIControl(IControlIdentity controlIdentity)
        {          
            this.controlIdentity = controlIdentity;
        }

        public override Rectangle GetBoundingBox()
        {
            var boundingBox = (TargetControl as AutomationElement).Current.BoundingRectangle;
            return new Rectangle((int)boundingBox.Left, (int)boundingBox.Top, (int)boundingBox.Width, (int)boundingBox.Height);
        }

        public override void GetClickablePoint(out double x, out double y)
        {
            var boundingBox = GetBoundingBox();
            controlIdentity.GetClickablePoint(boundingBox, out x, out y);
        }
    }
}
