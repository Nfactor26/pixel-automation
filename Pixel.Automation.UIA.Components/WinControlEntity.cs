using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Core.Components;
using System;
using System.Collections.Generic;
using System.Windows.Automation;
using Pixel.Automation.Core.Enums;

namespace Pixel.Automation.UIA.Components
{
    public class WinControlEntity : ControlEntity
    {       

        protected override void InitializeFilter()
        {
            if (this.Filter == null)
            {
                this.Filter = new PredicateArgument<AutomationElement>() { CanChangeMode = false, CanChangeType = false };
            }
        }

        public override T GetTargetControl<T>()
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
            AutomationElement uiaElement = default;
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
                return result;
            throw new Exception($"AutomationElement is not compatible with type {typeof(T)}");

        }

        public override UIControl GetControl()
        {         
            AutomationElement uiaElement = GetTargetControl<AutomationElement>();
            UIAControlLocatorComponent uiaControlLocator = this.EntityManager.GetControlLocator(this.ControlDetails) as UIAControlLocatorComponent;        
            return new WinUIControl(this.ControlDetails) { TargetControl = uiaElement };
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
                foundUIControls.Add(new WinUIControl(this.ControlDetails) { TargetControl = control });
            }
            return foundUIControls;
        }     

    }
}

