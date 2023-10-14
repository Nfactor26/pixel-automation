using Dawn;
using Microsoft.Playwright;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using System.Diagnostics.CodeAnalysis;

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
        [SetsRequiredMembers]
        public WebUIControl(IControlIdentity controlIdentity, ILocator locator, ICoordinateProvider coordinateProvider)
        {
            Guard.Argument(controlIdentity, nameof(controlIdentity)).NotNull();
            Guard.Argument(locator, nameof(locator)).NotNull();
            Guard.Argument(coordinateProvider, nameof(coordinateProvider)).NotNull();

            this.controlIdentity = controlIdentity;
            this.coordinateProvider = coordinateProvider;
            this.locator = locator;
            this.TargetControl = locator;
            this.ControlName = controlIdentity.Name;
        }

        ///<inheritdoc/>
        public override async Task<BoundingBox> GetBoundingBoxAsync()
        {           
            var boundingBox = await coordinateProvider.GetBoundingBox(locator);
            return boundingBox;
        }

        ///<inheritdoc/>
        public override async Task<(double, double)> GetClickablePointAsync()
        {
            var boundingBox = await GetBoundingBoxAsync();
            controlIdentity.GetClickablePoint(boundingBox, out double x, out double y);
            return await Task.FromResult((x, y));
        }

        ///<inheritdoc/>
        public override async Task<bool> IsDisabledAsync()
        {
            return await this.locator.IsDisabledAsync();
        }

        ///<inheritdoc/>
        public override async Task<bool> IsEnabledAsync()
        {
            return await this.locator.IsEnabledAsync();
        }

        ///<inheritdoc/>
        public override async Task<bool> IsHiddenAsync()
        {
            return await this.locator.IsHiddenAsync();
        }

        ///<inheritdoc/>
        public override async Task<bool> IsVisibleAsync()
        {
            return await this.locator.IsVisibleAsync();
        }

        ///<inheritdoc/>
        public override async Task<bool> IsCheckedAsync()
        {
            return await this.locator.IsCheckedAsync(new LocatorIsCheckedOptions() 
            { 
                Timeout = this.controlIdentity.RetryInterval * this.controlIdentity.RetryAttempts 
            });
        }

    }
}
