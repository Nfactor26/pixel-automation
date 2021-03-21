using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Core.Components;
using System;
using System.Collections.Generic;
using Serilog;

namespace Pixel.Automation.Web.Selenium.Components
{
    /// <summary>
    /// WebControlEntity wraps a control details required to lookup a control detail.
    /// It also supports caching of the <see cref="IWebElement"/> for subsequent lookup.
    /// </summary>
    public class WebControlEntity : ControlEntity
    {
        private readonly ILogger logger = Log.ForContext<WebControlEntity>();

        private IWebElement webElement;

        protected override void InitializeFilter()
        {
            if (this.Filter == null)
            {
                this.Filter = new PredicateArgument<IWebElement>() { CanChangeMode = false, CanChangeType = false };
            }
        }

        /// <summary>
        /// Clear the located control once entity is processed
        /// </summary>
        public override void OnCompletion()
        {
            if (CacheControl)
            {
                webElement = null;              
                logger.Debug($"Cleared cached WebElement for {this.Name}");
            }          
        }

        /// <summary>
        /// Get first control as <see cref="IWebElement"/> identified using wrapped <see cref="IControlIdentity"/>
        /// </summary>
        /// <typeparam name="T">Should be a IWebElement</typeparam>
        /// <returns>IWebElement</returns>
        /// <exception cref="Exception">Throws Exception if located control is not of type T</exception>
        public override T GetTargetControl<T>()
        {
            if (CacheControl && webElement != null)
            {
                if (webElement is T cachedControl)
                {
                    logger.Debug($"Return cached element for {this.Name}");
                    return cachedControl;
                }
                throw new Exception($"IWebElement is not compatible with type {typeof(T)}");
            }


            IWebElement searchRoot = default;
            if (this.SearchRoot.IsConfigured())
            {
                searchRoot = this.ArgumentProcessor.GetValue<UIControl>(this.SearchRoot)?.GetApiControl<IWebElement>();
            }
            else if (this.GetControlDetails().LookupType.Equals(LookupType.Relative))
            {
                searchRoot = (this.Parent as WebControlEntity).GetTargetControl<IWebElement>();
            }

            WebControlLocatorComponent webControlLocator = this.EntityManager.GetControlLocator(this.ControlDetails) as WebControlLocatorComponent;
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

            if (webElement is T result)
            {
                return result;
            }
            throw new Exception($"IWebElement is not compatible with type {typeof(T)}");
        }

        /// <summary>
        /// Get first control identified using wrapped <see cref="IControlIdentity"/>
        /// </summary>
        /// <returns></returns>
        public override UIControl GetControl()
        {
            IWebElement foundControl = GetTargetControl<IWebElement>();
            WebControlLocatorComponent webControlLocator = this.EntityManager.GetControlLocator(this.ControlDetails) as WebControlLocatorComponent;
            return new WebUIControl(this.ControlDetails, foundControl,  webControlLocator) ;
        }

        /// <summary>
        /// Get all the controls identifed using wrapped <see cref="IControlIdentity"/>
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<UIControl> GetAllControls()
        {
            IWebElement searchRoot = default;
            if (this.SearchRoot.IsConfigured())
            {              
                searchRoot = this.ArgumentProcessor.GetValue<UIControl>(this.SearchRoot)?.GetApiControl<IWebElement>();
            }
            else if (this.GetControlDetails().LookupType.Equals(LookupType.Relative))
            {
                searchRoot = (this.Parent as WebControlEntity).GetTargetControl<IWebElement>();
            }
            WebControlLocatorComponent webControlLocator = this.EntityManager.GetControlLocator(this.ControlDetails) as WebControlLocatorComponent;
            var foundControls = webControlLocator.FindAllControls(this.ControlDetails, searchRoot);
            List<UIControl> foundUIControls = new List<UIControl>();
            foreach(var foundControl in foundControls)
            {
                foundUIControls.Add(new WebUIControl(this.ControlDetails, foundControl,  webControlLocator));
            }
            return foundUIControls;
        }

    }
}
