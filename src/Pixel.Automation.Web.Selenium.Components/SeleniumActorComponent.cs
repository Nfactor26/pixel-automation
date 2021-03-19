using OpenQA.Selenium;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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

        protected SeleniumActorComponent(string name = "", string tag = "") : base(name, tag)
        {

        }

    }

    public abstract class WebElementActorComponent : SeleniumActorComponent
    {

        [DataMember]
        [Display(Name = "Target Control", GroupName = "Control Details", Order = 10)]
        [Description("[Optional] Specify a WebUIControl that should be clicked")]
        public Argument TargetControl { get; set; } = new InArgument<UIControl>() { Mode = ArgumentMode.DataBound, CanChangeType = false };


        [Browsable(false)]
        public IControlEntity ControlEntity
        {
            get
            {
                return this.Parent as IControlEntity;

            }
        }

        protected WebElementActorComponent(string name = "", string tag = "") : base(name, tag)
        {

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


        protected void ThrowIfMissingControlEntity()
        {
            if (this.ControlEntity == null)
            {
                throw new ConfigurationException($"Component with id : {this.Id} must be child of WebControlEntity");
            }
        }
    }
}
