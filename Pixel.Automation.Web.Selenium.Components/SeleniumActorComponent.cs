using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Exceptions;
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
                return this.EntityManager.GetApplicationDetails<WebApplication>(this);
            }
        }

        [Browsable(false)]
        public WebControlEntity ControlEntity
        {
            get
            {
                return this.Parent as WebControlEntity;            
            
            }
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
