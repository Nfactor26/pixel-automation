using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Models;
using Serilog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Get Attribute", "Selenium", iconSource: null, description: "Get the value of user defined attribute of a WebElement", tags: new string[] { "Attribute", "Attribute value", "Value", "Get", "Web" })]
    public class GetAttributeActorComponent : SeleniumActorComponent
    {
        [DataMember]
        [DisplayName("Target Control")]
        [Category("Control Details")]        
        [Browsable(true)]
        public Argument TargetControl { get; set; } = new InArgument<UIControl>() { Mode = ArgumentMode.DataBound, CanChangeType = false };

        [DataMember(IsRequired = true)]
        //[ItemsSource(typeof(AttributeItemsSource))]
        public string AttributeName { get; set; }

        [DataMember]       
        [Description("Store the value of attribute")]
        public Argument Result { get; set; } = new OutArgument<string>();
        
        public GetAttributeActorComponent() : base("Get Attribute", "GetAttribute")
        {

        }

        public override void Act()
        {
            IWebElement control = GetTargetControl(this.TargetControl);
            string extractedValue = control.GetAttribute(this.AttributeName);

            ArgumentProcessor.SetValue<string>(Result, extractedValue);

            Log.Information("GetAttribute  completed");          
        }

        public override string ToString()
        {
            return "Selenium.GetAttribute";
        }
    }
}
