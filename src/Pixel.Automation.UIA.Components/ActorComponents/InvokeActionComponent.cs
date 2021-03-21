extern alias uiaComWrapper;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.Runtime.Serialization;
using uiaComWrapper::System.Windows.Automation;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
    /// <summary>
    /// Use <see cref="InvokeActorComponent"/> to perform defult stateless action on a control e.g. click on a button.
    /// Control must supported <see cref="InvokePattern"/>.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Invoke", "UIA", iconSource: null, description: "Trigger Invoke pattern on AutomationElement", tags: new string[] { "Invoke","UIA" })]
    public class InvokeActorComponent : UIAActorComponent
    {
        private readonly ILogger logger = Log.ForContext<InvokeActorComponent>();

        /// <summary>
        /// Default constructor
        /// </summary>
        public InvokeActorComponent():base("Invoke","Invoke")
        {

        }

        /// <summary>
        /// Perform Invoke action on the control.
        /// </summary>
        public override void Act()
        {
            AutomationElement control = GetTargetControl();
            control.Invoke();
            logger.Information("Invoke performed on control.");
        }

        public override string ToString()
        {
            return "Invoke Actor";
        }
    }
}
