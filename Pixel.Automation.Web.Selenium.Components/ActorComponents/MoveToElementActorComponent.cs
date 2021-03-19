using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Selenium.Components
{
    /// <summary>
    /// Use <see cref="MoveToElementActorComponent"/> to simulate hover on a web control.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Hover", "Selenium", iconSource: null, description: "Perform a hover action on WebElement", tags: new string[] { "MouseOver", "Web" })]

    public class MoveToElementActorComponent : WebElementActorComponent
    {
        private readonly ILogger logger = Log.ForContext<MoveToElementActorComponent>();

        /// <summary>
        /// Optional argument which can be used to specify an offset from the of the <see cref="IWebElement"/> where hover action should be performed.
        /// This can be used to lookup an element say div , however, hover on a neighbor element by specifying appropriate offset.
        /// </summary>
        [DataMember]
        [Display(Name = "Offset", GroupName = "Configuration", Order = 20, Description = "[Optional] Specify an offset relative to center of the control for hover position")]      
        public Argument Offset { get; set; } = new InArgument<Point>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

        /// <summary>
        /// Default constructor
        /// </summary>
        public MoveToElementActorComponent() : base("Mouse Over", "MouseOver")
        {

        }

        /// <summary>
        /// Simulate a mouse over on a <see cref="IWebElement"/>
        /// </summary>
        public override void Act()
        {
            IWebElement control = ControlEntity.GetTargetControl<IWebElement>();
            Actions action = new Actions(ApplicationDetails.WebDriver);
            if(this.Offset.IsConfigured())
            {
                var offSet = this.ArgumentProcessor.GetValue<Point>(this.Offset);
                action.MoveToElement(control, offSet.X, offSet.Y, MoveToElementOffsetOrigin.Center).Perform();
            }
            else
            {
                action.MoveToElement(control).Perform();
            }
            logger.Information($"Hover on control");
        }

        public override string ToString()
        {
            return "Move To Element Actor";
        }
    }
}
