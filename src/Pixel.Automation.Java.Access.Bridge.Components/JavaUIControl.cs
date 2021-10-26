using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using System.Drawing;
using WindowsAccessBridgeInterop;

namespace Pixel.Automation.Java.Access.Bridge.Components
{
    public class JavaUIControl : UIControl
    {
        private readonly IControlIdentity controlIdentity;
        private readonly AccessibleContextNode controlNode;

        public JavaUIControl(IControlIdentity controlIdentity, AccessibleContextNode controlNode)
        {
            this.controlIdentity = controlIdentity;
            this.controlNode = controlNode;
            this.TargetControl = controlNode;
        }

        public override Rectangle GetBoundingBox()
        {
            var boundingBox = this.controlNode.GetScreenRectangle().Value;
            return boundingBox;
        }

        public override void GetClickablePoint(out double x, out double y)
        {
            var boundingBox = GetBoundingBox();
            controlIdentity.GetClickablePoint(boundingBox, out x, out y);
        }
    }
}
