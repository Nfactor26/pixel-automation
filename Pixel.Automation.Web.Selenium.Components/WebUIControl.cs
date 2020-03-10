using System.Drawing;
using OpenQA.Selenium;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;

namespace Pixel.Automation.Web.Selenium.Components
{
    public class WebUIControl : UIControl
    {
        private readonly WebControlLocatorComponent webControlLocator; 
        private readonly IControlIdentity controlIdentity;
        public WebUIControl(IControlIdentity controlIdentity, WebControlLocatorComponent webControlLocator)
        {
            this.controlIdentity = controlIdentity;
            this.webControlLocator = webControlLocator;
        }

        public override Rectangle GetBoundingBox()
        {
            //webdriver imlementation for location doesn't consider browser window titlebar space resulting in to incorrect y-coodrinate.
            //Hence, custom implementation
            IWebElement targetControl = GetApiControl<IWebElement>();
            //var boundingBox = new Rectangle(targetControl.Location, targetControl.Size);
            //return boundingBox;
            var boundingBox = webControlLocator.GetBoundingBox(targetControl);
            return boundingBox;
        }

        public override void GetClickablePoint(out double x, out double y)
        {
            var boundingBox = GetBoundingBox();
            controlIdentity.GetClickablePoint(boundingBox, out x, out y);
        }
    }
}
