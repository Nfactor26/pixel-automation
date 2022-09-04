﻿using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
    /// <summary>
    /// Use <see cref="LaunchApplicationActorComponent"/> to launch an application.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Launch", "Application", iconSource: null, description: "Launch target application", tags: new string[] { "Launch" })]
    public class LaunchApplicationActorComponent : ActorComponent
    {
        /// <summary>
        /// Owner application entity
        /// </summary>      
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
        public LaunchApplicationActorComponent() : base("Launch Application", "LuanchApplication")
        {

        }

        /// <summary>
        /// Launch the executable for owner application
        /// </summary>
        public override async Task ActAsync()
        {
            await this.ApplicationEntity.LaunchAsync();
        }

    }
}
