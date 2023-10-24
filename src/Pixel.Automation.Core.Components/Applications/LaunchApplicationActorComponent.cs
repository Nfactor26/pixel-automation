using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.UIA.Components;

/// <summary>
/// Use <see cref="LaunchApplicationActorComponent"/> to launch an application.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Launch", "Application", iconSource: null, description: "Launch target application", tags: new string[] { "Launch" })]
public class LaunchApplicationActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<LaunchApplicationActorComponent>();

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
        logger.Information("Application : '{0}' was launched", this.ApplicationEntity.GetTargetApplicationDetails().ApplicationName);
    }

}
