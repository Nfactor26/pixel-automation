using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Models;
using Serilog;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Send Key", "Selenium", iconSource: null, description: "Send keys to simulate typing on a  WebElement", tags: new string[] { "SendKey", "Type", "Web" })]
    public class SendKeyActorComponent : SeleniumActorComponent
    {
        [DataMember]
        [DisplayName("Target Control")]
        [Category("Control Details")]          
        [Browsable(true)]
        public Argument TargetControl { get; set; } = new InArgument<UIControl>() { Mode = ArgumentMode.DataBound, CanChangeType = false };

        [DataMember]       
        [Category("Send Key Settings")]      
        [Description("To send text,configure the input value in any of the mode. To send special keys such as Enter,use scripted mode and return desired keys. For Example : Keys.Control + \"A\" or Keys.Enter etc.")]
        public Argument Input { get; set; } = new InArgument<string>();

        [DataMember]
        [DisplayName("Clear Existing Value")]
        [Description("if set to true, value of control will be cleared before trying to set new value")]
        [Category("Send Key Settings")]    
        public bool ClearBeforeSendKeys { get; set; } = false;
            

        public SendKeyActorComponent() : base("Set Text", "SetText")
        {

        }

        public override void Act()
        {
            IWebElement control = GetTargetControl(this.TargetControl);
            string inputForControl = ArgumentProcessor.GetValue<string>(this.Input);
            if (this.ClearBeforeSendKeys)
            {
                Log.Information("Value of Control has been cleared ");
                control.Clear();
            }
            control.SendKeys(inputForControl);
            Log.Information("SendKey interaction completed");

        }

        public override string ToString()
        {
            return "Selenium.SetText";
        }

    }
}
