using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components.Controls
{
    [DataContract]
    [Serializable]
    [Builder(typeof(FindControlActorBuilder))]
    [ToolBoxItem("Find Control", "Control Lookup", iconSource: null, description: "Find the control matching configured control criteria", tags: new string[] { })]
    public class FindControlActorComponent : ActorComponent
    {
        [DataMember]
        [Display(Name = "Found Control", Order = 10, GroupName = "Output")]
        [Description("Argument will hold the found control")]
        public Argument FoundControl { get; set; } = new OutArgument<UIControl>() { CanChangeType = false, Mode = ArgumentMode.DataBound };


        [DataMember]
        [Display(Name = "Found", Order = 20, GroupName = "Output")]
        [Description("Argument will hold a boolean value indicating whether the control was located")]
        public Argument Exists { get; set; } = new OutArgument<bool>() { CanChangeType = false, Mode = ArgumentMode.DataBound };
     

        public FindControlActorComponent() : base("Find Control", "FindControlActorComponent")
        {

        }

        public override void Act()
        {          
            ControlEntity controlEntity = this.Parent.GetFirstComponentOfType<ControlEntity>(Core.Enums.SearchScope.Descendants);
            var foundControl = controlEntity.GetControl();
            IArgumentProcessor argumentProcessor = this.EntityManager.GetServiceOfType<IArgumentProcessor>();
            argumentProcessor.SetValue<UIControl>(this.FoundControl, foundControl);
            argumentProcessor.SetValue<bool>(this.Exists, foundControl != null);
        }
    }

    public class FindControlActorBuilder : IComponentBuillder
    {
        public Core.Interfaces.IComponent CreateComponent()
        {
            GroupEntity groupEntity = new GroupEntity()
            {
                Name = "Find Control",
                Tag = "FindControlGroup",
                GroupActor = new FindControlActorComponent()
            };
                 groupEntity.GroupPlaceHolder.MaxComponentsCount = 1;
            groupEntity.GroupPlaceHolder.AllowedComponentsType = typeof(ControlEntity);
            groupEntity.GroupPlaceHolder.Name = "Control";
            return groupEntity;
        }
    }
}
