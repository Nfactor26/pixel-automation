﻿using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

namespace Pixel.Automation.Core.Components.Controls
{
    [DataContract]
    [Serializable]
    [Builder(typeof(HighlightControlActorBuilder))]
    [ToolBoxItem("Highlight Control", "Control Lookup", iconSource: null, description: "Highlight the target control", tags: new string[] { "Highlight", "Utility" })]
    public class HighlightControlActorComponent : ActorComponent
    {
        [DataMember]
        [Display(Name = "Target Control", Order = 10, GroupName = "Input")]
        [Description("Specify a control to be highlighted")]       
        public Argument TargetControl { get; set; } = new InArgument<UIControl>() { CanChangeType = false, Mode= ArgumentMode.DataBound };
      
        [DataMember]
        [Display(Name = "Highlight Duration", Order = 20, GroupName = "Input")]      
        [Description("Highlight duration in seconds e.g. 0.5, 1, 10, etc.")]
        public Argument HighlightDuration { get; set; } = new InArgument<double>() { DefaultValue = 1 };


        public HighlightControlActorComponent() : base("Highlight Control", "HighlightControl")
        {

        }

        public override void Act()
        {
            IArgumentProcessor argumentProcessor = this.ArgumentProcessor;

            UIControl targetControl = default;         
            if (this.Parent.GetComponentsOfType<IControlEntity>(SearchScope.Descendants).Any())
            {
                var controlEntity = this.Parent.GetFirstComponentOfType<IControlEntity>(SearchScope.Descendants);
                if(controlEntity.ControlDetails.LookupType.Equals(LookupType.Relative))
                {
                    throw new InvalidOperationException("Highlight Control Actor doesn't support Relative controls");
                }
                targetControl = controlEntity.GetControl();
            }
            else
            {
                targetControl = argumentProcessor.GetValue<UIControl>(this.TargetControl);
            }

            if (targetControl != null)
            {
                Rectangle boundingBox = targetControl.GetBoundingBox();

                var highlightRectangle = this.EntityManager.GetServiceOfType<IHighlightRectangle>();

                highlightRectangle.Visible = true;

                highlightRectangle.Location = boundingBox;

                double highlightDuration = argumentProcessor.GetValue<double>(this.HighlightDuration);
                Thread.Sleep((int)(highlightDuration * 1000));

                highlightRectangle.Visible = false;
        
                return;
            }

        }
    }

    public class HighlightControlActorBuilder : IComponentBuillder
    {
        public Core.Interfaces.IComponent CreateComponent()
        {
            GroupEntity groupEntity = new GroupEntity()
            {
                Name = "Highlight Control",
                Tag = "HighlightControlGroup",
                GroupActor = new HighlightControlActorComponent()
            };
            groupEntity.GroupPlaceHolder.MaxComponentsCount = 1;
            groupEntity.GroupPlaceHolder.AllowedComponentsType = typeof(IControlEntity);
            groupEntity.GroupPlaceHolder.Name = "Control";
            return groupEntity;
        }
    }
}
