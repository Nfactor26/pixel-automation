using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.UIA.Components.ActorComponents
{

    [DataContract]
    [Serializable]
    [ToolBoxItem("Close", "UIA", "Application", iconSource: null, description: "Close target application", tags: new string[] { "Close", "UIA" })]

    public class WinApplicationCloserComponent : ActorComponent
    {
        [RequiredComponent]
        [Browsable(false)]
        public WinApplication ApplicationDetails
        {
            get
            {
                return this.EntityManager.GetOwnerApplication<WinApplication>(this);
            }
        }

        public WinApplicationCloserComponent() : base("Close Window", "ShutDownComponent")
        {
           
        }


        public override void Act()
        {
            ApplicationDetails.TargetApplication.Close();
        }

    }
}
