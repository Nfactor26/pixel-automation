﻿using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Enums;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WindowsAccessBridgeInterop;

namespace Pixel.Automation.Java.Access.Bridge.Components
{
    public class JavaControlEntity : ControlEntity
    {
        private readonly ILogger logger = Log.ForContext<JavaControlEntity>();

        private UIControl control;

        protected override void InitializeFilter()
        {
            if (this.Filter == null)
            {
                this.Filter = new PredicateArgument<AccessibleContextNode>();
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
                logger.Debug($"Cleared cached AccessibleContextNode for {this.Name}");
            }
            await Task.CompletedTask;
        }


        public override async Task<UIControl> GetControl()
        {
            if (CacheControl && control != null)
            {
                logger.Debug($"Return cached AccessibleContextNode for {this.Name}");
                return control;
            }

            UIControl searchRoot = default;
            if (this.SearchRoot.IsConfigured())
            {
                searchRoot = await this.ArgumentProcessor.GetValueAsync<UIControl>(this.SearchRoot);
            }
            else if (this.ControlDetails.LookupType.Equals(LookupType.Relative))
            {
                searchRoot = await (this.Parent as JavaControlEntity).GetControl();
            }

            JavaControlLocatorComponent controlLocator = this.EntityManager.GetControlLocator(this.ControlDetails) as JavaControlLocatorComponent;
            switch (LookupMode)
            {
                case LookupMode.FindSingle:
                    control = await controlLocator.FindControlAsync(this.ControlDetails, searchRoot);
                    break;
                case LookupMode.FindAll:
                    var descendantControls = await controlLocator.FindAllControlsAsync(this.ControlDetails, searchRoot);
                    switch (FilterMode)
                    {
                        case FilterMode.Index:
                            control = await  GetElementAtIndex(descendantControls);
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
            else if (this.ControlDetails.LookupType.Equals(LookupType.Relative))
            {
                searchRoot = await (this.Parent as JavaControlEntity).GetControl();
            }
            JavaControlLocatorComponent controlLocator = this.EntityManager.GetControlLocator(this.ControlDetails) as JavaControlLocatorComponent;
            var foundControls = await controlLocator.FindAllControlsAsync(this.ControlDetails, searchRoot);
            return foundControls;
        }
    }
}
