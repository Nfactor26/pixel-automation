using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Controls;

[DataContract]
[Serializable]
[Builder(typeof(FindAllControlsActorBuilder))]
[ToolBoxItem("Find All Controls", "Control Lookup", iconSource: null, description: "Find all controls matching configured control criteria", tags: new string[] { })]
public class FindAllControlsActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<FindAllControlsActorComponent>();

    [DataMember(Order = 200)]
    [Display(Name = "Found Controls", Order = 10, GroupName = "Output")]
    [Description("Argument will hold all the found controls")]
    public Argument FoundControls { get; set; } = new OutArgument<IEnumerable<UIControl>>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    [DataMember(Order = 210)]
    [Display(Name = "Count", Order = 20, GroupName = "Output")]
    [Description("[Optional] Argument will hold the count of found controls")]
    public Argument Count { get; set; } = new OutArgument<int>() { CanChangeType = false, Mode = ArgumentMode.DataBound };


    public FindAllControlsActorComponent() : base("Find All Controls", "FindAllControlsActorComponent")
    {

    }

    public override async Task ActAsync()
    {
        var controlEntities = this.Parent.GetComponentsOfType<IControlEntity>(Core.Enums.SearchScope.Descendants);
        List<UIControl> locatedControls = new List<UIControl>();
        foreach(var controlEntity in controlEntities)
        {
            var foundControls = await controlEntity.GetAllControls() ?? Array.Empty<UIControl>();
            locatedControls.AddRange(foundControls);
            logger.Information("Found : '{0}' controls for : '{1}'", foundControls.Count(), controlEntity.ControlDetails);
        }
        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
        await argumentProcessor.SetValueAsync<IEnumerable<UIControl>>(this.FoundControls, locatedControls);
        if(this.Count.IsConfigured())
        {
            await argumentProcessor.SetValueAsync<int>(this.Count, locatedControls.Count());
        }        
    }
}

public class FindAllControlsActorBuilder : IComponentBuillder
{
    public Core.Interfaces.IComponent CreateComponent()
    {
        var groupEntity = new GroupEntity()
        {
            Name = "Find All Control",
            Tag = "FindAllControlsGroup",
            GroupActor = new FindAllControlsActorComponent()
        };         
        groupEntity.GroupPlaceHolder.AllowedComponentsType = typeof(IControlEntity).Name;
        groupEntity.GroupPlaceHolder.Name = "Controls";
        return groupEntity;
    }
}
