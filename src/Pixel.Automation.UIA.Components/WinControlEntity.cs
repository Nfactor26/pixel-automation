extern alias uiaComWrapper;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Enums;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using uiaComWrapper::System.Windows.Automation;

namespace Pixel.Automation.UIA.Components
{
    public class WinControlEntity : ControlEntity
    {
        private readonly ILogger logger = Log.ForContext<WinControlEntity>();

        private UIControl control;
        
        protected override void InitializeFilter()
        {
            if (this.Filter == null)
            {
                this.Filter = new PredicateArgument<AutomationElement>();
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
                logger.Debug($"Cleared cached AutomationElement for {this.Name}");
            }
            await Task.CompletedTask;
        }


        public override async Task<UIControl> GetControl()
        {
            if (CacheControl && control != null)
            {
                logger.Debug($"Return cached AutomationElement for {this.Name}");
                return control;
            }

            UIControl searchRoot = default;
            if (this.SearchRoot.IsConfigured())
            {
                searchRoot = await this.ArgumentProcessor.GetValueAsync<UIControl>(this.SearchRoot);
            }
            else if (this.Parent is WinControlEntity controlEntity)
            {
                searchRoot = await controlEntity.GetControl();
            }

            UIAControlLocatorComponent uiaControlLocator = this.EntityManager.GetControlLocator(this.ControlDetails) as UIAControlLocatorComponent;
            switch (LookupMode)
            {
                case LookupMode.FindSingle:
                    control = await uiaControlLocator.FindControlAsync(this.ControlDetails, searchRoot);
                    break;
                case LookupMode.FindAll:
                    var descendantControls = await uiaControlLocator.FindAllControlsAsync(this.ControlDetails, searchRoot);
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


        public override async Task<IEnumerable<UIControl>> GetAllControls()
        {
            UIControl searchRoot = default;
            if (this.SearchRoot.IsConfigured())
            {
                searchRoot = await this.ArgumentProcessor.GetValueAsync<UIControl>(this.SearchRoot);
            }
            else if (this.Parent is WinControlEntity controlEntity)
            {
                searchRoot = await controlEntity.GetControl();
            }

            UIAControlLocatorComponent uiaControlLocator = this.EntityManager.GetControlLocator(this.ControlDetails) as UIAControlLocatorComponent;
            var foundControls = await uiaControlLocator.FindAllControlsAsync(this.ControlDetails, searchRoot);            
            return foundControls;
        }     

    }
}

