using Dawn;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Helpers
{
    /// <summary>
    /// Creates, modifies, or deletes an environment variable stored in the current process.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Set Env Variable", "Utility", iconSource: null, description: "Set environment variable", tags: new string[] { "set", "environment", "variable"})]
    public class SetEnvironmentVariableActorComponent : ActorComponent
    {
        private readonly ILogger logger = Log.ForContext<SetEnvironmentVariableActorComponent>();

        /// <summary>
        /// Name of an environment variable
        /// </summary>
        [DataMember]     
        [Display(Name = "Env Variable", GroupName = "Input", Order = 10, Description = "Name of the environment variable")]
        public Argument Variable { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, CanChangeType = false };

        /// <summary>
        /// Value to assign to environment variable
        /// </summary>
        [DataMember]      
        [Display(Name = "Value", GroupName = "Input", Order = 20, Description = "Value to set for the environment variable")]
        public Argument Value { get; set; } = new InArgument<string?>() { Mode = ArgumentMode.Default, CanChangeType = false };

        /// <summary>
        /// constructor
        /// </summary>
        public SetEnvironmentVariableActorComponent() : base("Set Environment Variable", "SetEnvironmentVariable")
        {

        }

        public override async Task ActAsync()
        {
            var variableName = await this.ArgumentProcessor.GetValueAsync<string>(this.Variable);
            Guard.Argument(variableName).NotNull().NotEmpty().NotWhiteSpace();
            var variableValue = await this.ArgumentProcessor.GetValueAsync<string?>(this.Value);
            Environment.SetEnvironmentVariable(variableName, variableValue);
            logger.Information("Value of environment value {0} was set", variableName);
        }
    }
}
