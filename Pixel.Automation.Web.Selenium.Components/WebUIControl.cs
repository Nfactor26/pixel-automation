using System.Drawing;
using Dawn;
using OpenQA.Selenium;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;

namespace Pixel.Automation.Web.Selenium.Components
{
    /// <summary>
    /// WebUI control wraps a IWebElement while providing some useful functionality on top of it.
    /// </summary>
    public class WebUIControl : UIControl
    {
        private readonly ICoordinateProvider coordinateProvider; 
        private readonly IControlIdentity controlIdentity;
        private readonly IWebElement webElement;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="controlIdentity">Control details</param>
        /// <param name="webElement">IWebElement</param>
        /// <param name="coordinateProvider">Coordinate provider</param>
        public WebUIControl(IControlIdentity controlIdentity, IWebElement webElement, ICoordinateProvider coordinateProvider)
        {
            Guard.Argument(controlIdentity).NotNull();
            Guard.Argument(webElement).NotNull();
            Guard.Argument(coordinateProvider).NotNull();

            this.controlIdentity = controlIdentity;
            this.coordinateProvider = coordinateProvider;
            this.webElement = webElement;
            this.TargetControl = webElement;
        }


        /// <summary>
        /// Get the bounding box of wrapped <see cref="IWebElement"/>
        /// </summary>
        /// <returns></returns>
        public override Rectangle GetBoundingBox()
        {
            //webdriver imlementation for location doesn't consider browser window titlebar space resulting in to incorrect y-coodrinate.
            //Hence, custom implementation          
            //var boundingBox = new Rectangle(webElement.Location, webElement.Size);
            //return boundingBox;
            var boundingBox = coordinateProvider.GetBoundingBox(webElement);
            return boundingBox;
        }

        /// <summary>
        /// Get the clickable point of wrapped <see cref="IWebElement"/>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public override void GetClickablePoint(out double x, out double y)
        {
            var boundingBox = GetBoundingBox();
            controlIdentity.GetClickablePoint(boundingBox, out x, out y);
        }
    }
}
