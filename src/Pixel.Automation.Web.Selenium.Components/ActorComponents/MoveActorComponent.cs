﻿using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Devices;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Selenium.Components
{
    /// <summary>
    /// Use <see cref="MoveActorComponent"/> to drag move a web control from its current position by a specified offset
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Move", "Selenium", iconSource: null, description: "Drag drop an element by configured offset ", tags: new string[] { "Drag By", "Web" })]
    public class MoveActorComponent : WebElementActorComponent
    {
        private readonly ILogger logger = Log.ForContext<MoveActorComponent>();

        /// <summary>
        /// Offset position from the control's current position.
        /// </summary>
        [DataMember]
        [Display(Name = "OffSet", GroupName = "Configuration", Order = 10, Description = "New position co-ordinates of the control")]
        public Argument OffSet { get; set; } = new InArgument<ScreenCoordinate>() { Mode = ArgumentMode.Default, DefaultValue = new ScreenCoordinate()};

        /// <summary>
        /// Default constructor
        /// </summary>
        public MoveActorComponent() : base("Drag By", "DragBy")
        {

        }

        /// <summary>
        /// Drag drop a <see cref="IWebElement"/> to an offset point from it's current position
        /// </summary>
        public override async Task ActAsync()
        {           
            var positionOffSet = await this.ArgumentProcessor.GetValueAsync<ScreenCoordinate>(this.OffSet);

            IWebElement control = await GetTargetControl();
            Actions action = new Actions(ApplicationDetails.WebDriver);
            action.DragAndDropToOffset(control, positionOffSet.XCoordinate, positionOffSet.YCoordinate).Perform();

            logger.Information($"control was dragged by offset ({positionOffSet.XCoordinate}, {positionOffSet.YCoordinate})");
        }

        public override string ToString()
        {
            return "Drag By Actor";
        }
    }
}
