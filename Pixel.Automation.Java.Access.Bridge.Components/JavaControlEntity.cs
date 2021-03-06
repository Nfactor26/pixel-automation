using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using WindowsAccessBridgeInterop;

namespace Pixel.Automation.Java.Access.Bridge.Components
{
    public class JavaControlEntity : ControlEntity
    {
        protected override void InitializeFilter()
        {
            if (this.Filter == null)
            {
                this.Filter = new PredicateArgument<AccessibleContextNode>() { CanChangeMode = false, CanChangeType = false };
            }
        }

        public override T GetTargetControl<T>()
        {
            AccessibleContextNode searchRoot = default;
            if (this.SearchRoot.IsConfigured())
            {
                searchRoot = this.ArgumentProcessor.GetValue<UIControl>(this.SearchRoot)?.GetApiControl<AccessibleContextNode>();
            }
            else if (this.GetControlDetails().ControlType.Equals(ControlType.Relative))
            {
                searchRoot = (this.Parent as JavaControlEntity).GetTargetControl<AccessibleContextNode>();
            }

            JavaControlLocatorComponent controlLocator = this.EntityManager.GetControlLocator(this.ControlDetails) as JavaControlLocatorComponent;
            AccessibleContextNode controlNode = default;
            switch (LookupMode)
            {
                case LookupMode.FindSingle:
                    controlNode = controlLocator.FindControl(this.ControlDetails, searchRoot);
                    break;
                case LookupMode.FindAll:
                    var descendantControls = controlLocator.FindAllControls(this.ControlDetails, searchRoot);
                    switch (FilterMode)
                    {
                        case FilterMode.Index:
                            controlNode = GetElementAtIndex(descendantControls);
                            break;
                        case FilterMode.Custom:
                            controlNode = GetElementMatchingCriteria(descendantControls);
                            break;
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }

            if (controlNode is T result)
            {
                return result;
            }
            throw new Exception($"AccessibleContextNode is not compatible with type {typeof(T)}");

        }

        public override UIControl GetControl()
        {
            AccessibleContextNode foundControl = GetTargetControl<AccessibleContextNode>();
            return new JavaUIControl(this.ControlDetails, foundControl);
        }


        public override IEnumerable<UIControl> GetAllControls()
        {
            AccessibleContextNode searchRoot = default;
            if (this.SearchRoot.IsConfigured())
            {
                searchRoot = this.ArgumentProcessor.GetValue<UIControl>(this.SearchRoot)?.GetApiControl<AccessibleContextNode>();
            }
            else if (this.GetControlDetails().ControlType.Equals(ControlType.Relative))
            {
                searchRoot = (this.Parent as JavaControlEntity).GetTargetControl<AccessibleContextNode>();
            }
            JavaControlLocatorComponent controlLocator = this.EntityManager.GetControlLocator(this.ControlDetails) as JavaControlLocatorComponent;
            var foundControls = controlLocator.FindAllControls(this.ControlDetails, searchRoot);
            List<UIControl> foundUIControls = new List<UIControl>();
            foreach (var foundControl in foundControls)
            {
                foundUIControls.Add(new JavaUIControl(this.ControlDetails, foundControl));
            }
            return foundUIControls;
        }
    }
}
