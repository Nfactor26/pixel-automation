using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components.Processors;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Controls
{
    [DataContract]
    [Serializable]   
    [ToolBoxItem("Repeat Until", "Control Lookup", iconSource: null, description: "Repeat until configured control state criteria is satisfied", tags: new string[] { "repeat", "until", "control" })]
    public class RepeatUntilEntityProcessor : EntityProcessor
    {
        private readonly ILogger logger = Log.ForContext<RepeatUntilEntityProcessor>();

        [DataMember]
        [Display(Name = "Index", Order = 10, GroupName = "Input", Description = "Index for the current iteration")]
        public Argument Index { get; set; } = new InArgument<int>() { CanChangeType = false, AllowedModes = ArgumentMode.DataBound, Mode = ArgumentMode.DataBound };

        [DataMember]
        [Display(Name = "Repeat Until", Order = 20, GroupName = "Input", Description = "Repeat until specified condition is satisfied for control")]
        public UntilControl RepeatUntil { get; set; } = UntilControl.Exists;

        [DataMember]
        [Display(Name = "Found Control", Order = 10, GroupName = "Output", Description = "[Optional] Argument will store the the found control")]
        public Argument FoundControl { get; set; } = new OutArgument<UIControl>() { CanChangeType = false, Mode = ArgumentMode.DataBound };
     
        public RepeatUntilEntityProcessor() : base("Repeat Until", "RepeatUntilEntityProcessor")
        {

        }

        public override async Task BeginProcessAsync()
        {
            var argumentProcessor = this.ArgumentProcessor;
            int index = await argumentProcessor.GetValueAsync<int>(this.Index);
            bool exitCriteriaSatisfied = false;
            var controlPlaceHolderEntity = this.GetComponentsByTag("Control").Single() as Entity;
            var statementsPlaceHolderEntity = this.GetComponentsByTag("Statements").Single() as Entity;
            while (true)
            {
                logger.Information("Current Index is : {0}", index);
                var controlEntity = controlPlaceHolderEntity.GetFirstComponentOfType<IControlEntity>(SearchScope.Descendants);
                UIControl control = null;
                try
                {
                    control = await controlEntity.GetControl();
                }
                catch (Exception)
                {
                    //when we are repeating until control exists, eventually control should stop existing.
                    //We don't want to consider this as an execption because this is expected to happen.
                    if (this.RepeatUntil.Equals(UntilControl.Exists))
                    {
                        logger.Information("Exit criteria : {0} for control is satisfied now.", RepeatUntil.ToString());
                        break;
                    }
                    //If repeat until criteria is control should not exist, we will get exception everytime control doesn't exist and should ignore it.
                    //For any other expected state e.g. enabled or disabled or visible or hidden, if control could not be found, treat as an exception.
                    if (!this.RepeatUntil.Equals(UntilControl.NotExists))
                    {
                        throw;
                    }
                }
                switch (this.RepeatUntil)
                {                  
                    case UntilControl.IsEnabled:
                        exitCriteriaSatisfied = !(await control.IsEnabledAsync());
                        break;
                    case UntilControl.IsDisabled:
                        exitCriteriaSatisfied = !(await control.IsDisabledAsync());
                        break;
                    case UntilControl.IsVisibile:
                        exitCriteriaSatisfied = !(await control.IsVisibleAsync());
                        break;
                    case UntilControl.IsHidden:
                        exitCriteriaSatisfied = !(await control.IsHiddenAsync());
                        break;
                    default:
                        break;
                }

                if (exitCriteriaSatisfied)
                {
                    logger.Information("Exit criteria : {0} for control is satisfied now.", RepeatUntil.ToString());
                    break;
                }

                if (this.FoundControl.IsConfigured())
                {
                    await argumentProcessor.SetValueAsync<UIControl>(this.FoundControl, control);
                }               
                await ProcessEntity(controlPlaceHolderEntity);              
                await ProcessEntity(statementsPlaceHolderEntity);

                await argumentProcessor.SetValueAsync<int>(this.Index, ++index);

                //Reset any inner loop for  next iteration
                foreach (var loop in this.GetInnerLoops())
                {
                    (loop as Entity).ResetHierarchy();
                }
            }
        }

        public override void ResolveDependencies()
        {
            if (this.Components.Count() > 0)
            {
                return;
            }
           
            base.AddComponent(new PlaceHolderEntity("Control", "Control"));
            base.AddComponent(new PlaceHolderEntity("Statements", "Statements"));
        }

        public override Entity AddComponent(Interfaces.IComponent component)
        {
            return this;
        }

        public override bool ValidateComponent()
        {
            if (this.Components.Count() > 0)
            {
                var controlPlaceHolderEntity = this.GetComponentsByTag("Control").Single() as Entity;
                return controlPlaceHolderEntity.GetComponentsOfType<IControlEntity>(SearchScope.Children).Any() && base.ValidateComponent();
            }
            return base.ValidateComponent();
        }
    }
}
