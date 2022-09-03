using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Enums;
using Serilog;

namespace Pixel.Automation.Web.Playwright.Components
{
    public class WebControlEntity : ControlEntity
    {
        private readonly ILogger logger = Log.ForContext<WebControlEntity>();

        private UIControl control;

        protected override void InitializeFilter()
        {
            if (this.Filter == null)
            {
                this.Filter = new PredicateArgument<ILocator>();
            }
        }

        /// <summary>
        /// Clear the located control once entity is processed
        /// </summary>
        public override async Task OnCompletionAsync()
        {
            if (CacheControl)
            {
                control = null;
                logger.Debug($"Cleared cached WebElement for {this.Name}");
            }
            await Task.CompletedTask;
        }
     
        /// <summary>
        /// Get first control identified using wrapped <see cref="IControlIdentity"/>
        /// </summary>
        /// <returns></returns>
        public override async  Task<UIControl> GetControl()
        {
            if (CacheControl && control != null)
            {
                logger.Debug($"Return cached element for {this.Name}");
                return control;
            }

            UIControl searchRoot = default;
            if (this.SearchRoot.IsConfigured())
            {
                searchRoot = await this.ArgumentProcessor.GetValueAsync<UIControl>(this.SearchRoot);
            }
            else if (this.Parent is WebControlEntity controlEntity)
            {
                searchRoot = await controlEntity.GetControl();
            }

            WebControlLocatorComponent webControlLocator = this.EntityManager.GetControlLocator(this.ControlDetails) as WebControlLocatorComponent;
            switch (LookupMode)
            {
                case LookupMode.FindSingle:
                    control = await webControlLocator.FindControlAsync(this.ControlDetails, searchRoot);
                    break;
                case LookupMode.FindAll:
                    var descendantControls = await webControlLocator.FindAllControlsAsync(this.ControlDetails, searchRoot);
                    switch (FilterMode)
                    {
                        case FilterMode.Index:
                            control = await GetElementAtIndex(descendantControls);
                            break;
                        case FilterMode.Custom:
                            control = GetElementMatchingCriteria(descendantControls);
                            break;
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }

            return control;
        }

        /// <summary>
        /// Get all the controls identifed using wrapped <see cref="IControlIdentity"/>
        /// </summary>
        /// <returns></returns>
        public override async Task<IEnumerable<UIControl>> GetAllControls()
        {
            UIControl searchRoot = default;
            if (this.SearchRoot.IsConfigured())
            {
                searchRoot = await this.ArgumentProcessor.GetValueAsync<UIControl>(this.SearchRoot);
            }
            else if (this.Parent is WebControlEntity controlEntity)
            {
                searchRoot = await controlEntity.GetControl();
            }
            WebControlLocatorComponent webControlLocator = this.EntityManager.GetControlLocator(this.ControlDetails) as WebControlLocatorComponent;
            var foundControls = await webControlLocator.FindAllControlsAsync(this.ControlDetails, searchRoot);
            return foundControls;
        }

    }
}