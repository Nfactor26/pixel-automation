using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
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
            ApplicationDetails.TargetApplication.Close();
        }

    }
}
