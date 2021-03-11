using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.Java.Access.Bridge.Components.ActorComponents
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Close", "Java", "Application", iconSource: null, description: "Close target application", tags: new string[] { "Close", "Java" })]

    public class CloseApplicationActorComponent : ActorComponent
    {
        private readonly ILogger logger = Log.ForContext<CloseApplicationActorComponent>();


        [RequiredComponent]
        [Browsable(false)]
        public JavaApplication ApplicationDetails
        {
            get
            {
                return this.EntityManager.GetOwnerApplication<JavaApplication>(this);
            }
        }

        public CloseApplicationActorComponent() : base("Close Application", "CloseApplication")
        {

        }


        public override void Act()
        {
            logger.Information($"Trying to close application : {ApplicationDetails}");
            ApplicationDetails.TargetApplication.Close();
            logger.Information("Application is closed now");
        }

    }
}
