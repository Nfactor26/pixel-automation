using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
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
    [ToolBoxItem("Close", "Application", iconSource: null, description: "Close target application", tags: new string[] { "Close" })]
    public class CloseApplicationActorComponent : ActorComponent
    {
        /// <summary>
        /// Owner application entity
        /// </summary>
        [RequiredComponent]
        [Browsable(false)]
        public IApplicationEntity ApplicationEntity
        {
            get
            {
                return this.EntityManager.GetApplicationEntity(this);
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
            var applicationEntity = this.ApplicationEntity;
            applicationEntity.Close();
        }

        public override string ToString()
        {
            return "Close Application Actor";
        }
    }
}
