using OpenQA.Selenium;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components
{
    [DataContract]
    [Serializable]
    public abstract class SeleniumActorComponent : ActorComponent
    {
        [RequiredComponent]
        [Browsable(false)]
        public WebApplication ApplicationDetails
        {
            get
            {
                return this.EntityManager.GetOwnerApplication<WebApplication>(this);
            }
        }

        [Browsable(false)]
        public IControlEntity ControlEntity
        {
            get
            {
                return this.Parent as IControlEntity;            
            
            }
        }    
                

        protected virtual IWebElement GetTargetControl(Argument controlArgument)
        {
            UIControl targetControl;
            if (controlArgument.IsConfigured())
            {
                targetControl = ArgumentProcessor.GetValue<UIControl>(controlArgument);
            }
            else
            {
                ThrowIfMissingControlEntity();
                targetControl = this.ControlEntity.GetControl();
            }

            return targetControl.GetApiControl<IWebElement>();
        }
       
        protected SeleniumActorComponent(string name = "", string tag = ""):base(name,tag)
        {

        }

        protected void ThrowIfMissingControlEntity()
        {
            if (this.ControlEntity == null)
            {
                throw new ConfigurationException($"Component with id : {this.Id} must be child of WebControlEntity");
            }
        }
    }
}
