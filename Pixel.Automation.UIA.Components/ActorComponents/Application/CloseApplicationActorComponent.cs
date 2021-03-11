using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.UIA.Components.ActorComponents
{

    [DataContract]
    [Serializable]
    [ToolBoxItem("Close", "UIA", "Application", iconSource: null, description: "Close target application", tags: new string[] { "Close", "UIA" })]

    public class CloseApplicationActorComponent : ActorComponent
    {
        private readonly ILogger logger = Log.ForContext<CloseApplicationActorComponent>();

        [RequiredComponent]
        [Browsable(false)]
        public WinApplication ApplicationDetails
        {
            get
            {
                return this.EntityManager.GetOwnerApplication<WinApplication>(this);
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
