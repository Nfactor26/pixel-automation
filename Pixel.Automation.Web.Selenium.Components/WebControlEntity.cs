using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Core.Components;
using System;
using System.Collections.Generic;

namespace Pixel.Automation.Web.Selenium.Components
{
    public class WebControlEntity : ControlEntity
    {
        protected override void InitializeFilter()
        {
            if (this.Filter == null)
            {
                this.Filter = new PredicateArgument<IWebElement>() { CanChangeMode = false, CanChangeType = false };
            }
        }

        public override T GetTargetControl<T>()
        {
            IWebElement searchRoot = default;
            if (this.SearchRoot.IsConfigured())
            {
                searchRoot = this.ArgumentProcessor.GetValue<UIControl>(this.SearchRoot)?.GetApiControl<IWebElement>();
            }
            else if (this.GetControlDetails().ControlType.Equals(ControlType.Relative))
            {
                searchRoot = (this.Parent as WebControlEntity).GetTargetControl<IWebElement>();
            }

            WebControlLocatorComponent webControlLocator = this.EntityManager.GetControlLocator(this.ControlDetails) as WebControlLocatorComponent;
            IWebElement webElement = default;
            switch (LookupMode)
            {
                case LookupMode.FindSingle:
                    webElement = webControlLocator.FindControl(this.ControlDetails, searchRoot);
                    break;
                case LookupMode.FindAll:
                    var descendantControls = webControlLocator.FindAllControls(this.ControlDetails, searchRoot);
                    switch (FilterMode)
                    {
                        case FilterMode.Index:
                            webElement = GetElementAtIndex(descendantControls);
                            break;
                        case FilterMode.Custom:
                            webElement = GetElementMatchingCriteria(descendantControls);
                            break;
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }

            return (T)webElement;
        }

        public override UIControl GetControl()
        {
            IWebElement foundControl = GetTargetControl<IWebElement>();
            WebControlLocatorComponent webControlLocator = this.EntityManager.GetControlLocator(this.ControlDetails) as WebControlLocatorComponent;
            return new WebUIControl(this.ControlDetails, webControlLocator) { TargetControl = foundControl };
        }    


        public override IEnumerable<UIControl> GetAllControls()
        {
            IWebElement searchRoot = default;
            if (this.SearchRoot.IsConfigured())
            {              
                searchRoot = this.ArgumentProcessor.GetValue<UIControl>(this.SearchRoot)?.GetApiControl<IWebElement>();
            }
            else if (this.GetControlDetails().ControlType.Equals(ControlType.Relative))
            {
                searchRoot = (this.Parent as WebControlEntity).GetTargetControl<IWebElement>();
            }
            WebControlLocatorComponent webControlLocator = this.EntityManager.GetControlLocator(this.ControlDetails) as WebControlLocatorComponent;
            var foundControls = webControlLocator.FindAllControls(this.ControlDetails, searchRoot);
            List<UIControl> foundUIControls = new List<UIControl>();
            foreach(var control in foundControls)
            {
                foundUIControls.Add(new WebUIControl(this.ControlDetails, webControlLocator) { TargetControl = control });
            }
            return foundUIControls;
        }

    }
}
