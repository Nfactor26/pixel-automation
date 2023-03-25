using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using System.Threading.Tasks;
using Pixel.Windows.Automation;

namespace Pixel.Automation.UIA.Components
{
    public class WinUIControl : UIControl
    {            
        private readonly IControlIdentity controlIdentity;
        private readonly AutomationElement automationElement;
        public static UIControl RootControl { get; private set; } = new WinUIControl(null, AutomationElement.RootElement);

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="controlIdentity"></param>
        /// <param name="automationElement"></param>
        public WinUIControl(IControlIdentity controlIdentity, AutomationElement automationElement)
        {          
            this.controlIdentity = controlIdentity;
            this.automationElement = automationElement;
            this.TargetControl = automationElement;
        }

        ///<inheritdoc/>
        public override async Task<BoundingBox> GetBoundingBoxAsync()
        {
            var boundingBox = this.automationElement.Current.BoundingRectangle;
            return await Task.FromResult(new BoundingBox((int)boundingBox.Left, (int)boundingBox.Top, (int)boundingBox.Width, (int)boundingBox.Height));
        }

        ///<inheritdoc/>
        public override async Task<(double,double)> GetClickablePointAsync()
        {
            var boundingBox = await GetBoundingBoxAsync();
            controlIdentity.GetClickablePoint(boundingBox, out double x, out double y);
            return await Task.FromResult((x, y));
        }

        ///<inheritdoc/>
        public override Task<bool> IsVisibleAsync()
        {
            return Task.FromResult(!this.automationElement.Current.IsOffscreen);
        }

        ///<inheritdoc/>
        public override Task<bool> IsHiddenAsync()
        {
            return Task.FromResult(this.automationElement.Current.IsOffscreen);
        }

        ///<inheritdoc/>
        public override Task<bool> IsEnabledAsync()
        {
            return Task.FromResult(this.automationElement.Current.IsEnabled);
        }

        ///<inheritdoc/>
        public override Task<bool> IsDisabledAsync()
        {
            return Task.FromResult(!this.automationElement.Current.IsEnabled);
        }

        ///<inheritdoc/>
        public override Task<bool> IsCheckedAsync()
        {
           if(this.automationElement.TryGetCurrentPattern(TogglePattern.Pattern, out object pattern))
           {
                bool isChecked = (pattern as TogglePattern).Current.ToggleState.Equals(ToggleState.On);
                return Task.FromResult(isChecked);
           }
           return Task.FromResult(false);
        }
    }
}
