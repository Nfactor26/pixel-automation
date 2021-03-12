﻿extern alias uiaComWrapper;
using System.Collections.Generic;
using uiaComWrapper::System.Windows.Automation;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Core.Components;
using System;
using Pixel.Automation.Core.Enums;
using System.ComponentModel.DataAnnotations;
using Serilog;

namespace Pixel.Automation.UIA.Components
{
    public class WinControlEntity : ControlEntity
    {
        private readonly ILogger logger = Log.ForContext<WinControlEntity>();

        private AutomationElement uiaElement;
        
        protected override void InitializeFilter()
        {
            if (this.Filter == null)
            {
                this.Filter = new PredicateArgument<AutomationElement>() { CanChangeMode = false, CanChangeType = false };
            }
        }

        /// <summary>
        /// Clear the located control once entity is processed
        /// </summary>
        public override void OnCompletion()
        {
            if (CacheControl)
            {
                uiaElement = null;
                logger.Debug($"Cleared cached AutomationElement for {this.Name}");
            }

        }


        public override T GetTargetControl<T>()
        {
            if(CacheControl && uiaElement != null)
            {
                if(uiaElement is T cachedControl)
                {
                    logger.Debug($"Return cached AutomationElement for {this.Name}");
                    return cachedControl;
                }
                throw new Exception($"AutomationElement is not compatible with type {typeof(T)}");
            }

            AutomationElement searchRoot = default;
            if (this.SearchRoot.IsConfigured())
            {
                searchRoot = this.ArgumentProcessor.GetValue<UIControl>(this.SearchRoot)?.GetApiControl<AutomationElement>();
            }
            else if (this.GetControlDetails().ControlType.Equals(Core.Enums.ControlType.Relative))
            {
                searchRoot = (this.Parent as WinControlEntity).GetTargetControl<AutomationElement>();
            }

            UIAControlLocatorComponent uiaControlLocator = this.EntityManager.GetControlLocator(this.ControlDetails) as UIAControlLocatorComponent;         
            switch (LookupMode)
            {
                case LookupMode.FindSingle:
                    uiaElement = uiaControlLocator.FindControl(this.ControlDetails, searchRoot);
                    break;
                case LookupMode.FindAll:
                    var descendantControls = uiaControlLocator.FindAllControls(this.ControlDetails, searchRoot);
                    switch (FilterMode)
                    {
                        case FilterMode.Index:
                            uiaElement = GetElementAtIndex(descendantControls);
                            break;
                        case FilterMode.Custom:
                            uiaElement = GetElementMatchingCriteria(descendantControls);
                            break;
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }

            if (uiaElement is T result)
            {
                return result;
            }
            throw new Exception($"AutomationElement is not compatible with type {typeof(T)}");

        }

        public override UIControl GetControl()
        {         
            AutomationElement uiaElement = GetTargetControl<AutomationElement>();
            return new WinUIControl(this.ControlDetails, uiaElement);
        }


        public override IEnumerable<UIControl> GetAllControls()
        {
            AutomationElement searchRoot = default;
            if (this.SearchRoot.IsConfigured())
            {         
                searchRoot = this.ArgumentProcessor.GetValue<UIControl>(this.SearchRoot)?.GetApiControl<AutomationElement>();
            }
            else if (this.GetControlDetails().ControlType.Equals(Core.Enums.ControlType.Relative))
            {
                searchRoot = (this.Parent as WinControlEntity).GetTargetControl<AutomationElement>();
            }

            UIAControlLocatorComponent uiaControlLocator = this.EntityManager.GetControlLocator(this.ControlDetails) as UIAControlLocatorComponent;
            var foundControls = uiaControlLocator.FindAllControls(this.ControlDetails, searchRoot);
            List<UIControl> foundUIControls = new List<UIControl>();
            foreach (var control in foundControls)
            {
                foundUIControls.Add(new WinUIControl(this.ControlDetails, control));
            }
            return foundUIControls;
        }     

    }
}

