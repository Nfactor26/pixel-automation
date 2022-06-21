extern alias uiaComWrapper;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using System.Drawing;
using System.Threading.Tasks;
using uiaComWrapper::System.Windows.Automation;

namespace Pixel.Automation.UIA.Components
{
    public class WinUIControl : UIControl
    {     
       
        private readonly IControlIdentity controlIdentity;
        private readonly AutomationElement automationElement;
        public static UIControl RootControl { get; private set; } = new WinUIControl(null, AutomationElement.RootElement);

        public WinUIControl(IControlIdentity controlIdentity, AutomationElement automationElement)
        {          
            this.controlIdentity = controlIdentity;
            this.automationElement = automationElement;
            this.TargetControl = automationElement;
        }

        public override async Task<Rectangle> GetBoundingBoxAsync()
        {
            var boundingBox = this.automationElement.Current.BoundingRectangle;
            return await Task.FromResult(new Rectangle((int)boundingBox.Left, (int)boundingBox.Top, (int)boundingBox.Width, (int)boundingBox.Height));
        }

        public override async Task<(double,double)> GetClickablePointAsync()
        {
            var boundingBox = await GetBoundingBoxAsync();
            controlIdentity.GetClickablePoint(boundingBox, out double x, out double y);
            return await Task.FromResult((x, y));
        }
    }
}
