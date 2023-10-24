using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Controls;

[DataContract]
[Serializable]
[Builder(typeof(FindControlActorBuilder))]
[ToolBoxItem("Find Control", "Control Lookup", iconSource: null, description: "Find the control matching configured control criteria", tags: new string[] { })]
public class FindControlActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<FindControlActorComponent>();

    [DataMember(Order = 200)]
    [Display(Name = "Found Control", Order = 10, GroupName = "Output")]
    [Description("Argument will hold the found control")]
    public Argument FoundControl { get; set; } = new OutArgument<UIControl>() { CanChangeType = false, Mode = ArgumentMode.DataBound };


    [DataMember(Order = 210)]
    [Display(Name = "Found", Order = 20, GroupName = "Output")]
    [Description("Argument will hold a boolean value indicating whether the control was located")]
    public Argument Exists { get; set; } = new OutArgument<bool>() { CanChangeType = false, Mode = ArgumentMode.DataBound };
 

    public FindControlActorComponent() : base("Find Control", "FindControlActorComponent")
    {

    }

    public override async Task ActAsync()
    {          
        var controlEntity = this.Parent.GetFirstComponentOfType<IControlEntity>(Core.Enums.SearchScope.Descendants);
        var foundControl = await controlEntity.GetControl();
        logger.Information("Control : '{0}' was located", controlEntity.ControlDetails);
        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
        await argumentProcessor.SetValueAsync<UIControl>(this.FoundControl, foundControl);
        await argumentProcessor.SetValueAsync<bool>(this.Exists, foundControl != null);
    }
}

public class FindControlActorBuilder : IComponentBuillder
{
    public Core.Interfaces.IComponent CreateComponent()
    {
        var groupEntity = new GroupEntity()
        {
            Name = "Find Control",
            Tag = "FindControlGroup",
            GroupActor = new FindControlActorComponent()
        };
        groupEntity.GroupPlaceHolder.MaxComponentsCount = 1;
        groupEntity.GroupPlaceHolder.AllowedComponentsType = typeof(IControlEntity).Name;
        groupEntity.GroupPlaceHolder.Name = "Control";
        return groupEntity;
    }
}
