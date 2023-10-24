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

namespace Pixel.Automation.Core.Components.Controls;

[DataContract]
[Serializable]   
[ToolBoxItem("Repeat Until", "Control Lookup", iconSource: null, description: "Repeat until configured control state criteria is satisfied", tags: new string[] { "repeat", "until", "control" })]
public class RepeatUntilEntityProcessor : EntityProcessor
{
    private readonly ILogger logger = Log.ForContext<RepeatUntilEntityProcessor>();

    [DataMember(Order = 200)]
    [Display(Name = "Index", Order = 10, GroupName = "Input", Description = "Index for the current iteration")]
    public Argument Index { get; set; } = new InArgument<int>() { CanChangeType = false, AllowedModes = ArgumentMode.DataBound, Mode = ArgumentMode.DataBound };

    [DataMember(Order = 210)]
    [Display(Name = "Repeat Until", Order = 20, GroupName = "Input", Description = "Repeat until specified condition is satisfied for control")]
    public UntilControl RepeatUntil { get; set; } = UntilControl.Exists;

    [DataMember(Order = 220)]
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
        logger.Information(": Begin Repeat Until");
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
                    logger.Information("Control : '{0}' is {1}", control.ControlName, exitCriteriaSatisfied ? "disabled" : "enabled");
                    break;
                case UntilControl.IsDisabled:
                    exitCriteriaSatisfied = !(await control.IsDisabledAsync());
                    logger.Information("Control : '{0}' is {1}", control.ControlName, exitCriteriaSatisfied ? "enabled" : "disabled");
                    break;
                case UntilControl.IsVisibile:
                    exitCriteriaSatisfied = !(await control.IsVisibleAsync());
                    logger.Information("Control : '{0}' is {1}", control.ControlName, exitCriteriaSatisfied ? "hidden" : "visible");
                    break;
                case UntilControl.IsHidden:
                    exitCriteriaSatisfied = !(await control.IsHiddenAsync());
                    logger.Information("Control : '{0}' is {1}", control.ControlName, exitCriteriaSatisfied ? "visible" : "hidden");
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
        logger.Information(": End Repeat Until");
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

    public override Entity AddComponent(IComponent component)
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
