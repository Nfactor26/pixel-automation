using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Models;
using Serilog;
using System;
using System.Collections.Generic;
using WindowsAccessBridgeInterop;

namespace Pixel.Automation.Java.Access.Bridge.Components
{
    public class JavaControlEntity : ControlEntity
    {
        private readonly ILogger logger = Log.ForContext<JavaControlEntity>();

        private AccessibleContextNode controlNode;

        protected override void InitializeFilter()
        {
            if (this.Filter == null)
            {
                this.Filter = new PredicateArgument<AccessibleContextNode>() { CanChangeMode = false, CanChangeType = false };
            }
        }

        /// <summary>
        /// Clear the located control once entity is processed
        /// </summary>
        public override void OnCompletion()
        {
            if (CacheControl)
            {
                controlNode = null;
                logger.Debug($"Cleared cached AccessibleContextNode for {this.Name}");
            }         
        }


        public override T GetTargetControl<T>()
        {
            if (CacheControl && controlNode != null)
            {
                if (controlNode is T cachedControl)
                {
                    logger.Debug($"Return cached AccessibleContextNode for {this.Name}");                 
                    return cachedControl;
                }
                throw new Exception($"AccessibleContextNode is not compatible with type {typeof(T)}");
            }

            AccessibleContextNode searchRoot = default;
            if (this.SearchRoot.IsConfigured())
            {
                searchRoot = this.ArgumentProcessor.GetValue<UIControl>(this.SearchRoot)?.GetApiControl<AccessibleContextNode>();
            }
            else if (this.GetControlDetails().LookupType.Equals(LookupType.Relative))
            {
                searchRoot = (this.Parent as JavaControlEntity).GetTargetControl<AccessibleContextNode>();
            }

            JavaControlLocatorComponent controlLocator = this.EntityManager.GetControlLocator(this.ControlDetails) as JavaControlLocatorComponent;          
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
            else if (this.GetControlDetails().LookupType.Equals(LookupType.Relative))
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
