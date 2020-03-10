using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Application Pool", "Core Entities", iconSource: null, description: "Represents an application pool", tags: new string[] { "Application Pool" })]
    public class ApplicationPoolEntity : Entity
    {
        public ApplicationPoolEntity() : base("Application Pool", "ApplicationPoolEntity")
        {

        }
    }
}
