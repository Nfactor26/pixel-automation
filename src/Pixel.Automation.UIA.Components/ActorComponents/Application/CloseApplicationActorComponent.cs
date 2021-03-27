using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.UIA.Components.ActorComponents
{

    /// <summary>
    /// Use <see cref="CloseApplicationActorComponent"/> to close application which was previously launched or attached to.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Close", "Application", "UIA", iconSource: null, description: "Close target application", tags: new string[] { "Close", "UIA" })]
    public class CloseApplicationActorComponent : ActorComponent
    {
        private readonly ILogger logger = Log.ForContext<CloseApplicationActorComponent>();

        /// <summary>
        /// Owner application details
        /// </summary>
        [RequiredComponent]
        [Browsable(false)]
        public WinApplication ApplicationDetails
        {
            get
            {
                return this.EntityManager.GetOwnerApplication<WinApplication>(this);
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public CloseApplicationActorComponent() : base("Close Application", "CloseApplication")
        {

        }

        /// <summary>
        /// Close the owner application
        /// </summary>
        public override void Act()
        {
            ApplicationDetails.TargetApplication.Close();
            logger.Information($"Application : {ApplicationDetails}  is closed now.");
        }

        public override string ToString()
        {
            return "Close Application Actor";
        }
    }
}
