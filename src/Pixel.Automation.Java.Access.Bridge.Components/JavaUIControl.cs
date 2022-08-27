using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using System.Threading.Tasks;
using WindowsAccessBridgeInterop;

namespace Pixel.Automation.Java.Access.Bridge.Components
{
    public class JavaUIControl : UIControl
    {
        private readonly IControlIdentity controlIdentity;
        private readonly AccessibleContextNode controlNode;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="controlIdentity"></param>
        /// <param name="controlNode"></param>
        public JavaUIControl(IControlIdentity controlIdentity, AccessibleContextNode controlNode)
        {
            this.controlIdentity = controlIdentity;
            this.controlNode = controlNode;
            this.TargetControl = controlNode;
        }

        ///<inheritdoc/>
        public override async Task<BoundingBox> GetBoundingBoxAsync()
        {
            var boundingBox = this.controlNode.GetScreenRectangle().Value;
            return await Task.FromResult(new BoundingBox(boundingBox.X, boundingBox.Y, boundingBox.Width, boundingBox.Height));
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
            return Task.FromResult(!this.controlNode.GetInfo().states.Contains("enabled"));
        }

        ///<inheritdoc/>
        public override Task<bool> IsEnabledAsync()
        {
            return Task.FromResult(this.controlNode.GetInfo().states.Contains("enabled"));
        }

        ///<inheritdoc/>
        public override Task<bool> IsHiddenAsync()
        {
            return Task.FromResult(!this.controlNode.GetInfo().states.Contains("visible"));
        }

        ///<inheritdoc/>
        public override Task<bool> IsVisibleAsync()
        {
            return Task.FromResult(this.controlNode.GetInfo().states.Contains("visible"));
        }

        ///<inheritdoc/>
        public override Task<bool> IsCheckedAsync()
        {
            return Task.FromResult(this.controlNode.GetInfo().states.Contains("checked"));
        }

    }
}
