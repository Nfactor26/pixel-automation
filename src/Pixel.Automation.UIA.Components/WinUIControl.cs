extern alias uiaComWrapper;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using System.Drawing;
using uiaComWrapper::System.Windows.Automation;

namespace Pixel.Automation.UIA.Components
{
    public class WinUIControl : UIControl
    {     
       
        private readonly IControlIdentity controlIdentity;
        private readonly AutomationElement automationElement;

        public WinUIControl(IControlIdentity controlIdentity, AutomationElement automationElement)
        {          
            this.controlIdentity = controlIdentity;
            this.automationElement = automationElement;
            this.TargetControl = automationElement;
        }

        public override Rectangle GetBoundingBox()
        {
            var boundingBox = this.automationElement.Current.BoundingRectangle;
            return new Rectangle((int)boundingBox.Left, (int)boundingBox.Top, (int)boundingBox.Width, (int)boundingBox.Height);
        }

        public override void GetClickablePoint(out double x, out double y)
        {
            var boundingBox = GetBoundingBox();
            controlIdentity.GetClickablePoint(boundingBox, out x, out y);
        }
    }
}
