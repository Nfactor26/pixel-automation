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
    /// Retrieves the value of an environment variable from the current process.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Get Env Variable", "Utility", iconSource: null, description: "Retrieve value of an environment variable from the current process", tags: new string[] { "get", "environment", "variable" })]
    public class GetEnvironmentVariableActorComponent : ActorComponent
    {
        private readonly ILogger logger = Log.ForContext<GetEnvironmentVariableActorComponent>();

        /// <summary>
        /// Name of an environment variable
        /// </summary>
        [DataMember(Order = 200)]
        [Display(Name = "Env Variable", GroupName = "Input", Order = 10, Description = "Name of the environment variable")]
        public Argument Variable { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, CanChangeType = false };

        /// <summary>
        /// Retrieved value of environment variable
        /// </summary>
        [DataMember(Order = 210)]
        [Display(Name = "Value", GroupName = "Output", Order = 10, Description = "Retrieved value of environment variable")]
        public Argument Value { get; set; } = new OutArgument<string?>() { Mode = ArgumentMode.Default, CanChangeType = false };

        /// <summary>
        /// constructor
        /// </summary>
        public GetEnvironmentVariableActorComponent() : base("Get Environment Variable", "GetEnvironmentVariable")
        {

        }

        public override async Task ActAsync()
        {
            var variableName = await this.ArgumentProcessor.GetValueAsync<string>(this.Variable);
            Guard.Argument(variableName).NotNull().NotEmpty().NotWhiteSpace();
            var variableValue = Environment.GetEnvironmentVariable(variableName);
            await this.ArgumentProcessor.SetValueAsync(Value, variableValue);
            logger.Information("Value of environment value {0} was retrieved", variableName);
        }
    }
}
