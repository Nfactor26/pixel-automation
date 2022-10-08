using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Polly;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Controls
{
    [DataContract]
    [Serializable]
    [Builder(typeof(WaitUntilActorBuilder))]
    [ToolBoxItem("Wait Until", "Control Lookup", iconSource: null, description: "Wait until configured control state criteria is satisfied", tags: new string[] { "wait", "until", "control" })]
    public class WaitUntilActorComponent : ActorComponent
    {
        private readonly ILogger logger = Log.ForContext<ActorComponent>();

        [DataMember]
        [Display(Name = "Retry Attempts", Order = 10, GroupName = "Input", Description = "Number of retry attempts")]
        public Argument RetryAttempts { get; set; } = new InArgument<int>() { CanChangeType = false, DefaultValue = 5 };

        [DataMember]
        [Display(Name = "Retry Interval", Order = 20, GroupName = "Input", Description = "Delay between retry attempts in seconds")]
        public Argument RetryInterval { get; set; } = new InArgument<int>() { CanChangeType = false, DefaultValue = 1 };

        [DataMember]
        [Display(Name = "Wait Until", Order = 30, GroupName = "Input", Description = "Wait until specified condition is satisfied for control")]
        public UntilControl WaitUntil { get; set; } = UntilControl.Exists;

        [DataMember]
        [Display(Name = "Found Control", Order = 10, GroupName = "Output", Description = "[Optional] Argument will store the the found control")]
        public Argument FoundControl { get; set; } = new OutArgument<UIControl>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

        public WaitUntilActorComponent() : base("Wait Until", "WaitUntil")
        {

        }

        public override async Task ActAsync()
        {
            IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
            int retryAttempts = await argumentProcessor.GetValueAsync<int>(this.RetryAttempts);
            int retryInterval = await argumentProcessor.GetValueAsync<int>(this.RetryInterval);
            var retrySequence = new List<TimeSpan>();
            foreach (var i in Enumerable.Range(1, retryAttempts))
            {
                retrySequence.Add(TimeSpan.FromSeconds(retryInterval));
            }
            var policy = Policy.Handle<Exception>().WaitAndRetryAsync(retrySequence, (exception, timeSpan, retryCount, context) =>
            {              
               if (retryCount < retrySequence.Count)
               {
                   logger.Information("Wait until : {0} criteria not satisfied for control. Attempt {1} out of {2}", retryCount, retryAttempts);
               }
            });

            var foundControl = await policy.ExecuteAsync(async () =>
            {
                var controlEntity = this.Parent.GetFirstComponentOfType<IControlEntity>(Enums.SearchScope.Descendants);
                controlEntity.ControlDetails.RetryAttempts = 1;
                controlEntity.ControlDetails.RetryInterval = 1;
                UIControl control;
                try
                {
                    control = await controlEntity.GetControl();
                }
                catch (Exception)
                {
                    //When we are waiting until control doesn't exist, we will get an exception looking for control eventually when the criteria is sastisfied.
                    //We need to ignore this exception as this is expected.
                    if(this.WaitUntil.Equals(UntilControl.NotExists))
                    {
                        return null;
                    }
                    throw;
                }
                bool criteriaSatisfied = false;
                switch(this.WaitUntil)
                {
                    case UntilControl.Exists:
                        criteriaSatisfied = true;
                        break;
                    case UntilControl.NotExists:
                        criteriaSatisfied = false;
                        break;
                    case UntilControl.IsEnabled:
                        criteriaSatisfied = await control.IsEnabledAsync();
                        break;
                    case UntilControl.IsDisabled:
                        criteriaSatisfied = await control.IsDisabledAsync();
                        break;
                    case UntilControl.IsVisibile:
                        criteriaSatisfied = await control.IsVisibleAsync();
                        break;
                    case UntilControl.IsHidden:
                        criteriaSatisfied = await control.IsHiddenAsync();
                        break;                   
                }
                if(!criteriaSatisfied)
                {
                    throw new Exception();
                }
                return control;
            });

            if (this.FoundControl.IsConfigured() && !this.WaitUntil.Equals(UntilControl.NotExists))
            {
                await argumentProcessor.SetValueAsync<UIControl>(this.FoundControl, foundControl);
            }
        }
    }

    public class WaitUntilActorBuilder : IComponentBuillder
    {
        public IComponent CreateComponent()
        {
            var groupEntity = new GroupEntity()
            {
                Name = "Wait Until",
                Tag = "WaitUntilGroupEntity",
                GroupActor = new WaitUntilActorComponent()
            };
            groupEntity.GroupPlaceHolder.MaxComponentsCount = 1;
            groupEntity.GroupPlaceHolder.AllowedComponentsType = typeof(IControlEntity).Name;
            groupEntity.GroupPlaceHolder.Name = "Control";
            return groupEntity;
        }
    }
}
