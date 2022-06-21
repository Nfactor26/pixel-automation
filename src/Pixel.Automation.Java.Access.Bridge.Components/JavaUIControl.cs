using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using System.Drawing;
using System.Threading.Tasks;
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

        public override async Task<Rectangle> GetBoundingBoxAsync()
        {
            var boundingBox = this.controlNode.GetScreenRectangle().Value;
            return await Task.FromResult(boundingBox);
        }

        public override async Task<(double, double)> GetClickablePointAsync()
        {
            var boundingBox = await GetBoundingBoxAsync();
            controlIdentity.GetClickablePoint(boundingBox, out double x, out double y);
            return await Task.FromResult((x, y));
        }
    }
}
