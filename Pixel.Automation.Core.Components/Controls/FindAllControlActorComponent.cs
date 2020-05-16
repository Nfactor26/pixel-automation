using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components.Controls
{
    [DataContract]
    [Serializable]
    [Builder(typeof(FindAllControlsActorBuilder))]
    [ToolBoxItem("Find All Controls", "Control Lookup", iconSource: null, description: "Find all controls matching configured control criteria", tags: new string[] { })]
    public class FindAllControlsActorComponent : ActorComponent
    {
        [DataMember]
        [Display(Name = "Found Controls", Order = 10, GroupName = "Output")]
        [Description("Argument will hold all the found controls")]
        public Argument FoundControls { get; set; } = new OutArgument<IEnumerable<UIControl>>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

        [DataMember]
        [Display(Name = "Count", Order = 20, GroupName = "Output")]
        [Description("Argument will hold the count of found controls")]
        public Argument Count { get; set; } = new OutArgument<int>() { CanChangeType = false, Mode = ArgumentMode.DataBound };


        public FindAllControlsActorComponent() : base("Find All Controls", "FindAllControlsActorComponent")
        {

        }

        public override void Act()
        {
            var controlEntities = this.Parent.GetComponentsOfType<IControlEntity>(Core.Enums.SearchScope.Descendants);
            List<UIControl> locatedControls = new List<UIControl>();
            foreach(var controlEntity in controlEntities)
            {
                var foundControls = controlEntity.GetAllControls() ?? Array.Empty<UIControl>();
                locatedControls.AddRange(foundControls);
            }
            IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
            argumentProcessor.SetValue<IEnumerable<UIControl>>(this.FoundControls, locatedControls);      
            argumentProcessor.SetValue<int>(this.Count, locatedControls.Count());
        }
    }

    public class FindAllControlsActorBuilder : IComponentBuillder
    {
        public Core.Interfaces.IComponent CreateComponent()
        {
            GroupEntity groupEntity = new GroupEntity()
            {
                Name = "Find All Control",
                Tag = "FindAllControlsGroup",
                GroupActor = new FindAllControlsActorComponent()
            };         
            groupEntity.GroupPlaceHolder.AllowedComponentsType = typeof(IControlEntity);
            groupEntity.GroupPlaceHolder.Name = "Controls";
            return groupEntity;
        }
    }
}
