using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components;

[DataContract]
[Serializable]
[ToolBoxItem("Reset Component", "Core Components", iconSource: null, description: "Reset a component and all its children", tags: new string[] { "Wait", "Core" })]
public class ResetActorComponent : ActorComponent
{
    [DataMember(Order = 200)]
    [System.ComponentModel.Description("Id of the component that needs to be reset")]
    [Display(Name = "Component Id", Order = 10, GroupName = "Input")]
    public string ComponentId { get; set; }

    public ResetActorComponent() : base("Reset Component", "ResetActorComponent")
    {

    }

    public override async Task ActAsync()
    {        
        IComponent targetComponent = EntityManager.RootEntity.GetComponentById(ComponentId,SearchScope.Descendants);

        if (!targetComponent.IsEnabled)
        {
            return;
        }

        if (targetComponent is Entity)
        {
            (targetComponent as Entity).ResetHierarchy();
        }
        else
        {
            targetComponent.ResetComponent();
        }

        await Task.CompletedTask;
    }
}
