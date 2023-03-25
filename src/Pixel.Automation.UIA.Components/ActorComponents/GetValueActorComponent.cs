using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Pixel.Windows.Automation;

namespace Pixel.Automation.UIA.Components
{
    /// <summary>
    /// Use <see cref="GetValueActorComponent"/> to get the value of a control.
    /// Control must support <see cref="ValuePattern"/>.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Get Value", "UIA", iconSource: null, description: "Trigger Value pattern on AutomationElement to GetValue", tags: new string[] { "GetValue", "UIA" })]
    public class GetValueActorComponent : UIAActorComponent
    {
        private readonly ILogger logger = Log.ForContext<GetValueActorComponent>();

        /// <summary>
        /// Argument where the value of the attribute will be stored
        /// </summary>
        [DataMember]
        [Display(Name = "Result", GroupName = "Output", Order = 10, Description = "Argument where the value will be stored")]
        public Argument Result { get; set; } = new OutArgument<string>();

        /// <summary>
        /// Default constructor
        /// </summary>
        public GetValueActorComponent() : base("Get Value", "GetValue")
        {

        }

        /// <summary>
        /// Get the value of control that supports ValuePattern.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws InvalidOperationException if ValuePattern is not supported</exception>
        public override async Task ActAsync()
        {
            AutomationElement control = await GetTargetControl();
            string result = control.GetValue();
            await ArgumentProcessor.SetValueAsync<string>(Result, result);
            logger.Information("Value of control was retrieved.");
        }

    }
}
