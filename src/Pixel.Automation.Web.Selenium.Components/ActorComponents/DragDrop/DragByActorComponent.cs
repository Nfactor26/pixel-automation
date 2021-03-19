using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;


namespace Pixel.Automation.Web.Selenium.Components
{
    /// <summary>
    /// Use <see cref="DragByActorComponent"/> to drag drop a web control from its current position to a specified offset
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Drag By", "Selenium", iconSource: null, description: "Drag drop an element by configured offset ", tags: new string[] { "Drag By", "Web" })]
    public class DragByActorComponent : WebElementActorComponent
    {
        private readonly ILogger logger = Log.ForContext<DragByActorComponent>();

        /// <summary>
        /// x-offset from current position of web control
        /// </summary>
        [DataMember]
        [Display(Name = "X-Offset", GroupName = "Configuration", Order = 20, Description = "x-offset from current position of web control")]     
        public Argument XOffSet { get; set; } = new InArgument<int>() { CanChangeType = false, Mode = ArgumentMode.Default, DefaultValue = 0 };

        /// <summary>
        /// y-offset from current position of web control
        /// </summary>
        [DataMember]
        [Display(Name = "Y-Offset", GroupName = "Configuration", Order = 20, Description = "y-offset from current position of web control")]       
        public Argument YOffSet { get; set; } = new InArgument<int>() { CanChangeType = false, Mode = ArgumentMode.Default, DefaultValue = 0 };

        /// <summary>
        /// Default constructor
        /// </summary>
        public DragByActorComponent() : base("Drag By", "DragBy")
        {

        }

        /// <summary>
        /// Drag drop a <see cref="IWebElement"/> to an offset point from it's current position
        /// </summary>
        public override void Act()
        {
            var arugmentProcessor = this.ArgumentProcessor;
            int xOffsetAmount = arugmentProcessor.GetValue<int>(this.XOffSet);
            int yOffsetAmount = arugmentProcessor.GetValue<int>(this.YOffSet);

            IWebElement control = ControlEntity.GetTargetControl<IWebElement>();
            Actions action = new Actions(ApplicationDetails.WebDriver);
            action.DragAndDropToOffset(control, xOffsetAmount, yOffsetAmount).Perform();

            logger.Information($"control was dragged by offset ({xOffsetAmount}, {yOffsetAmount})");
        }

        public override string ToString()
        {
            return "Drag By Actor";
        }
    }
}
