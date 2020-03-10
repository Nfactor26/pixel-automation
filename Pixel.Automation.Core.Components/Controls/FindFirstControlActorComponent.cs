using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Polly;
using Serilog;
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
    [Builder(typeof(FindFirstControlActorBuider))]
    [ToolBoxItem("Find First Control", "Control Lookup", iconSource: null, description: "Find first control available", tags: new string[] { })]
    public class FindFirstControlActorComponent : ActorComponent
    {
        private readonly ILogger logger = Log.ForContext<FindFirstControlActorComponent>();

        [DataMember]
        [Display(Name = "Found Control", Order = 10, GroupName = "Output")]
        [Description("Argument will hold first control that was located")]
        public Argument FoundControl { get; set; } = new OutArgument<UIControl>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

        [DataMember]
        [Display(Name = "Found", Order = 20, GroupName = "Output")]
        [Description("Argument will hold a boolean value indicating whether any of the control was located")]
        public Argument Exists { get; set; } = new OutArgument<bool>() { CanChangeType = false, Mode = ArgumentMode.DataBound };


        [DataMember]
        [Display(Name = "Retry Attempts", Order = 30, GroupName = "Search Strategy")]
        [Description("Number of times to retry lookup for each control until one is located. Controls are looked up in round robin fashion.")]
        public Argument RetryAttempts { get; set; } = new InArgument<int>() { CanChangeType = false, Mode = ArgumentMode.Default, DefaultValue = 5 };

        [DataMember]
        [Display(Name="Throw If Not Found", Order = 40, GroupName = "Error Handling")]
        [Description("Indicates whether an exception will be thrown if none of the controls are found")]
        public bool ThrowIfNotFound { get; set; }

        public FindFirstControlActorComponent() : base("Find First Control", "FindFirstontrolActorComponent")
        {

        }

        public override void Act()
        {
            IArgumentProcessor argumentProcessor = this.EntityManager.GetServiceOfType<IArgumentProcessor>();
            int retryAttempts = argumentProcessor.GetValue<int>(this.RetryAttempts);
            var retrySequence = new List<TimeSpan>();
            foreach (var i in Enumerable.Range(1, retryAttempts))
            {
                retrySequence.Add(TimeSpan.FromSeconds(1));
            }

            var retryPolicy =  Policy.Handle<Exception>()         
            .WaitAndRetry(retrySequence, (exception, timeSpan, retryCount, context) =>
            {
                logger.Information($"None of the controls could be located. Retry count is : {retryCount}");
            });

            var controlEntities = this.Parent.GetComponentsOfType<ControlEntity>(Core.Enums.SearchScope.Descendants);

            var foundCountol = retryPolicy.Execute(() =>
            {
                foreach (var controlEntity in controlEntities)
                {
                    try
                    {
                        var foundControl = controlEntity.GetControl();
                        argumentProcessor.SetValue<UIControl>(this.FoundControl, foundControl);
                        argumentProcessor.SetValue<bool>(this.Exists, foundControl != null);
                        return foundControl;
                    }
                    catch
                    {

                    }
                }
                throw new Exception("None of the control could be located");
            });

            if(foundCountol != null)
            {
                return;
            }

            argumentProcessor.SetValue<bool>(this.Exists, false);
            if (ThrowIfNotFound)
            {
                throw new ElementNotFoundException("No control could be located");
            }
        }
    }

    public class FindFirstControlActorBuider : IComponentBuillder
    {
        public Core.Interfaces.IComponent CreateComponent()
        {
            GroupEntity groupEntity = new GroupEntity()
            {
                Name = "Find First Control",
                Tag = "FindFirstControlsGroup",
                GroupActor = new FindFirstControlActorComponent()
            };
            groupEntity.GroupPlaceHolder.MaxComponentsCount = 50;
            groupEntity.GroupPlaceHolder.AllowedComponentsType = typeof(ControlEntity);
            groupEntity.GroupPlaceHolder.Name = "Control";
            return groupEntity;
        }
    }
}
