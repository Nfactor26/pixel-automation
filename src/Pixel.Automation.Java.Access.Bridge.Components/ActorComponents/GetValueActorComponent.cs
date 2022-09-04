using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using WindowsAccessBridgeInterop;

namespace Pixel.Automation.Java.Access.Bridge.Components
{
    /// <summary>
    /// Use <see cref="GetValueActorComponent"/> to retrieve the value of a control.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Get Value", "Java", iconSource: null, description: "Retrieve the value of a control", tags: new string[] { "Value", "UIA" })]

    public class GetValueActorComponent : JABActorComponent
    {
        private readonly ILogger logger = Log.ForContext<GetValueActorComponent>();

        /// <summary>
        /// Argument where the retrieved value will be stored
        /// </summary>
        [DataMember]
        [Display(Name = "Result", GroupName = "Output", Order = 10, Description = "Argument where the retreived value will be stored")]
        public Argument Result { get; set; } = new OutArgument<string>();

        /// <summary>
        /// Default constructor
        /// </summary>
        public GetValueActorComponent() : base("Get Value", "GetValue")
        {

        }

        /// <summary>
        /// Get the value of a control
        /// </summary>
        public override async Task ActAsync()
        {
            AccessibleContextNode targetControl = await this.GetTargetControl();
            var info = targetControl.GetInfo();
            if ((info.accessibleInterfaces & AccessibleInterfaces.cAccessibleValueInterface) != 0)
            {
                var sb = new StringBuilder(1024);
                targetControl.AccessBridge.Functions.GetCurrentAccessibleValueFromContext(targetControl.JvmId, targetControl.AccessibleContextHandle, sb, (short)sb.Capacity);
                string controlValue = sb.ToString();
                await this.ArgumentProcessor.SetValueAsync<string>(this.Result, controlValue);
                logger.Information("Value was retrieved from control.");
                return;
            }
            throw new InvalidOperationException($"Control: {this.TargetControl} doesn't support cAccessibleValueInterface.");
        }

    }
}
