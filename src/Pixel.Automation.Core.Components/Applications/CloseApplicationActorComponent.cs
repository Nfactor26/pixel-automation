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
/// Use <see cref="CloseApplicationActorComponent"/> to close application which was previously launched or attached to.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Close", "Application", iconSource: null, description: "Close target application", tags: new string[] { "Close" })]
public class CloseApplicationActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<CloseApplicationActorComponent>();

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
    public CloseApplicationActorComponent() : base("Close Application", "CloseApplication")
    {

    }

    /// <summary>
    /// Close the owner application
    /// </summary>
    public override async Task ActAsync()
    {
        var applicationEntity = this.ApplicationEntity;
        await applicationEntity.CloseAsync();
        logger.Information("Application : '{0}' was closed", applicationEntity.GetTargetApplicationDetails().ApplicationName);
    }

}
