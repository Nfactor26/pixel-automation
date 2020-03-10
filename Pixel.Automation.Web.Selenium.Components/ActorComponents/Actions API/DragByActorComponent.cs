using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;


namespace Pixel.Automation.Web.Selenium.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Drag By", "Selenium", iconSource: null, description: "Drag an element by configured offset ", tags: new string[] { "Drag By", "Web" })]
    public class DragByActorComponent : SeleniumActorComponent
    {
        [DataMember]
        [Description("Drag x-offset")]
        public Argument XOffSet { get; set; } = new InArgument<int>();

        [DataMember]           
        [Description("Drag y-offset")]
        public Argument YOffSet { get; set; } = new InArgument<int>();

        public DragByActorComponent() : base("Drag By", "DragBy")
        {

        }

        public override void Act()
        {
            var arugmentProcessor = this.ArgumentProcessor;
            int xOffsetAmount = arugmentProcessor.GetValue<int>(this.XOffSet);
            int yOffsetAmount = arugmentProcessor.GetValue<int>(this.YOffSet);

            IWebElement control = ControlEntity.GetTargetControl<IWebElement>();
            Actions action = new Actions(ApplicationDetails.WebDriver);
            action.DragAndDropToOffset(control,xOffsetAmount,yOffsetAmount).Perform();
        }

        public override string ToString()
        {
            return "Selenium.DragBy";
        }
    }
}
