using Dawn;
using Microsoft.Playwright;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.Web.Playwright.Components
{
    public class WebUIControl : UIControl
    {
        private readonly ICoordinateProvider coordinateProvider;
        private readonly IControlIdentity controlIdentity;
        private readonly ILocator locator;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="controlIdentity">Control details</param>
        /// <param name="locator">IWebElement</param>
        /// <param name="coordinateProvider">Coordinate provider</param>
        public WebUIControl(IControlIdentity controlIdentity, ILocator locator, ICoordinateProvider coordinateProvider)
        {
            Guard.Argument(controlIdentity).NotNull();
            Guard.Argument(locator).NotNull();
            Guard.Argument(coordinateProvider).NotNull();

            this.controlIdentity = controlIdentity;
            this.coordinateProvider = coordinateProvider;
            this.locator = locator;
            this.TargetControl = locator;
        }


        /// <summary>
        /// Get the bounding box
        /// </summary>
        /// <returns></returns>
        public override async Task<BoundingBox> GetBoundingBoxAsync()
        {           
            var boundingBox = await coordinateProvider.GetBoundingBox(locator);
            return boundingBox;
        }

        /// <summary>
        /// Get the clickable point
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public override async Task<(double, double)> GetClickablePointAsync()
        {
            var boundingBox = await GetBoundingBoxAsync();
            controlIdentity.GetClickablePoint(boundingBox, out double x, out double y);
            return await Task.FromResult((x, y));
        }
    }
}
