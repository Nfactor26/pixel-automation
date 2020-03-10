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
    [ToolBoxItem("Get Value", "Selenium", iconSource: null, description: "Get the value attribute of a WebElement", tags: new string[] { "Value", "Get", "Web" })]
    public class GetValueActorComponent : SeleniumActorComponent
    {
        [DataMember]
        [DisplayName("Target Control")]
        [Category("Control Details")]
        [Browsable(true)]
        public Argument TargetControl { get; set; } = new InArgument<UIControl>() { Mode = ArgumentMode.DataBound, CanChangeType = false };

        [DataMember]        
        [Description("Store the value in Result Argument")]
        public Argument Result { get; set; } = new OutArgument<string>();

        public GetValueActorComponent() : base("Get Value", "GetValue")
        {

        }

        public override void Act()
        {
            UIControl targetControl = default;
            if (this.TargetControl.IsConfigured())
            {
                targetControl = ArgumentProcessor.GetValue<UIControl>(this.TargetControl);
            }
            else
            {
                ThrowIfMissingControlEntity();
                targetControl = this.ControlEntity.GetControl();
            }

            IWebElement control = targetControl.GetApiControl<IWebElement>();
            string extractedValue = control.GetAttribute("value");
            if (string.IsNullOrEmpty(extractedValue))
                extractedValue = control.Text;

            ArgumentProcessor.SetValue<string>(Result, extractedValue);

            Log.Information("GetValue interaction completed");
        }

        public override string ToString()
        {
            return "Selenium.GetValue";
        }
    }
}
