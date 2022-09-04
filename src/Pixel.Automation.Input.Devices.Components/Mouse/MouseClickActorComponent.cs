using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using MouseButton = Pixel.Automation.Core.Devices.MouseButton;

namespace Pixel.Automation.Input.Devices.Components
{
    /// <summary>
    /// Use <see cref="MouseClickActorComponent"/> to simulate a mouse click operation.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Click", "Input Device", "Mouse", iconSource: null, description: "Perform a click action using mouse at given coordinates", tags: new string[] { "Click" })]
    public class MouseClickActorComponent : InputDeviceActor
    {
        private readonly ILogger logger = Log.ForContext<MouseClickActorComponent>();

        /// <summary>
        /// Mouse button that needs to be clicked.
        /// </summary>
        [DataMember]
        [Display(Name = "Mouse Button", GroupName = "Configuration", Order = 10, Description = "Identifies the mouse button that needs to be clicked")]
        public MouseButton Button { get; set; } = MouseButton.LeftButton;

        /// <summary>
        /// Indicates whether to perform a single or double click
        /// </summary>
        [DataMember]
        [Display(Name = "Click Mode", GroupName = "Configuration", Order = 20, Description = "Indicates whether to perform a single or double click")]
        public ClickMode ClickMode { get; set; } = ClickMode.SingleClick;

        /// <summary>
        /// Controls how the mouse moves between two points
        /// </summary>
        [DataMember]
        [Display(Name = "Smooth Mode", GroupName = "Configuration", Order = 30, Description = "Controls how the mouse moves between two points")]
        public SmoothMode SmootMode { get; set; } = SmoothMode.Interpolated;


        Target target = Target.Control;
        /// <summary>
        /// Specify whether to perform click on a target control or arbitrary co-ordinates
        /// </summary>
        [DataMember]
        [Display(Name = "Target", GroupName = "Configuration", Order = 40, Description = "Indicates whether to click a control or an arbitrary co-ordinates")]
        [RefreshProperties(RefreshProperties.Repaint)]
        public Target Target
        {
            get
            {
                switch (this.target)
                {
                    case Target.Control:
                        this.SetDispalyAttribute(nameof(ClickAt), false);
                        this.SetDispalyAttribute(nameof(TargetControl), true);
                        break;
                    case Target.Point:
                        this.SetDispalyAttribute(nameof(ClickAt), true);
                        this.SetDispalyAttribute(nameof(TargetControl), false);
                        break;
                }
                return this.target;
            }
            set
            {
                this.target = value;
                OnPropertyChanged();
                ValidateComponent();
            }
        }

        /// <summary>
        /// Target control that needs to be clicked. Target control is automatically identfied if component is a child of <see cref="IControlEntity"/>.
        /// However, if control has been already looked up and stored in an argument, it can be passed as an argument instead as the target control which needs to be clicked.
        /// </summary>
        [DataMember]
        [Display(Name = "Target Control", GroupName = "Control Details", Order = 50, Description = "[Optional] Target control that needs to be clicked.")]  
        public Argument TargetControl { get; set; } = new InArgument<UIControl>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

        /// <summary>
        /// MouseClickActorComponent can be used to click at an arbitrary co-ordinates instead of a target control. Click At argument can be used to spcify an arbitrary co-ordinates
        /// where click is performed.
        /// </summary>
        [DataMember]
        [Display(Name = "Click At", GroupName = "Point", Order = 50, Description = "[Optional] Co-ordinates at which click needs to be performed if there is no target control.")]
        public Argument ClickAt { get; set; } = new InArgument<ScreenCoordinate>() { DefaultValue = new ScreenCoordinate()};

        /// <summary>
        /// Default constructor
        /// </summary>
        public MouseClickActorComponent() : base("Mouse Click", "MouseClick")
        {

        }

        /// <summary>
        /// Simulate a mouse click on a target control or an arbitrary co-ordinates.
        /// </summary>
        public override async Task ActAsync()
        {
            IArgumentProcessor argumentProcessor = this.ArgumentProcessor;

            ScreenCoordinate screenCoordinate = default;
            switch (this.Target)
            {
                case Target.Control:
                    screenCoordinate = await GetScreenCoordinateFromControl(this.TargetControl as InArgument<UIControl>);
                    break;
                case Target.Point:
                    screenCoordinate = await argumentProcessor.GetValueAsync<ScreenCoordinate>(this.ClickAt);
                    break;
            }

            var syntheticMouse = GetMouse();
            syntheticMouse.MoveMouseTo(screenCoordinate, this.SmootMode);

            switch (Button)
            {

                case MouseButton.LeftButton:
                    switch (this.ClickMode)
                    {
                        case ClickMode.SingleClick:
                            syntheticMouse.Click(MouseButton.LeftButton);
                            logger.Information($"Left click was performed at {screenCoordinate}.");
                            break;
                        case ClickMode.DoubleClick:
                            syntheticMouse.DoubleClick(MouseButton.LeftButton);
                            logger.Information($"Left double click was performed at {screenCoordinate}.");
                            break;
                    }
                    break;
                case MouseButton.MiddleButton:
                    switch (this.ClickMode)
                    {
                        case ClickMode.SingleClick:
                            syntheticMouse.Click(MouseButton.MiddleButton);
                            logger.Information($"Middle click was performed at {screenCoordinate}.");
                            break;
                        case ClickMode.DoubleClick:
                            syntheticMouse.DoubleClick(MouseButton.MiddleButton);
                            logger.Information($"Middle double click was performed at {screenCoordinate}.");
                            break;
                    }
                    break;
                case MouseButton.RightButton:
                    switch (this.ClickMode)
                    {
                        case ClickMode.SingleClick:
                            syntheticMouse.Click(MouseButton.RightButton);
                            logger.Information($"Right click was performed at {screenCoordinate}.");
                            break;
                        case ClickMode.DoubleClick:
                            syntheticMouse.DoubleClick(MouseButton.RightButton);
                            logger.Information($"Right double click was performed at {screenCoordinate}.");
                            break;
                    }
                    break;
            }
        }

        /// <summary>
        /// Check if component is correctly configured
        /// </summary>
        /// <returns></returns>
        public override bool ValidateComponent()
        {
            IsValid = true;
            switch (this.Target)
            {
                case Target.Point:
                    IsValid = this.ClickAt?.IsConfigured() ?? false;
                    break;
                case Target.Control:
                    IsValid = (this.TargetControl?.IsConfigured() ?? false) || this.ControlEntity != null;
                    break;
            }
            return IsValid && base.ValidateComponent();
        }

    }
}
