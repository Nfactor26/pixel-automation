using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.Core.Interfaces;
using Polly;
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
[Builder(typeof(FindFirstControlActorBuider))]
[ToolBoxItem("Find First Control", "Control Lookup", iconSource: null, description: "Find first control available", tags: new string[] { })]
public class FindFirstControlActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<FindFirstControlActorComponent>();

    [DataMember(Order = 200)]
    [Display(Name = "Found Control", Order = 10, GroupName = "Output")]
    [Description("Argument will hold first control that was located")]
    public Argument FoundControl { get; set; } = new OutArgument<UIControl>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    [DataMember(Order = 210)]
    [Display(Name = "Found", Order = 20, GroupName = "Output")]
    [Description("[Optional] Argument will hold a boolean value indicating whether any of the control was located")]
    public Argument Exists { get; set; } = new OutArgument<bool>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    [DataMember(Order = 220)]
    [Display(Name = "Retry Attempts", Order = 30, GroupName = "Search Strategy")]
    [Description("Number of times to retry lookup for each control until one is located. Controls are looked up in round robin fashion.")]
    public Argument RetryAttempts { get; set; } = new InArgument<int>() { CanChangeType = false, Mode = ArgumentMode.Default, DefaultValue = 5 };

    [DataMember(Order = 230)]
    [Display(Name="Throw If Not Found", Order = 40, GroupName = "Error Handling")]
    [Description("Indicates whether an exception will be thrown if none of the controls are found")]
    public bool ThrowIfNotFound { get; set; }

    public FindFirstControlActorComponent() : base("Find First Control", "FindFirstontrolActorComponent")
    {

    }

    public override async Task ActAsync()
    {
        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
        int retryAttempts = await argumentProcessor.GetValueAsync<int>(this.RetryAttempts);
        var retrySequence = new List<TimeSpan>();
        foreach (var i in Enumerable.Range(1, retryAttempts))
        {
            retrySequence.Add(TimeSpan.FromSeconds(2));
        }

        var retryPolicy = Policy.Handle<Exception>()
        .WaitAndRetry(retrySequence, (exception, timeSpan, retryCount, context) =>
        {
            logger.Debug($"None of the controls could be located. Retry count is : {retryCount}");
        });           

        var controlEntities = this.Parent.GetComponentsOfType<IControlEntity>(Core.Enums.SearchScope.Descendants);
    
        // we don't want to wait too long when looking up multiple controls in round robin and expecting only one to be found.
        foreach (var controlEntity in controlEntities)
        {
            controlEntity.ControlDetails.RetryAttempts = 1;
            controlEntity.ControlDetails.RetryInterval = 1;
        }

        UIControl foundControl = default;
        try
        {
            foundControl = await retryPolicy.Execute(async () =>
            {
                foreach (var controlEntity in controlEntities)
                {
                    try
                    {
                        var foundControl = await controlEntity.GetControl();
                        await argumentProcessor.SetValueAsync<UIControl>(this.FoundControl, foundControl);
                        if(this.Exists.IsConfigured())
                        {
                            await argumentProcessor.SetValueAsync<bool>(this.Exists, foundControl != null);
                        }
                        logger.Information("Control : '{0}' was located", controlEntity.ControlDetails);
                        return foundControl;
                    }
                    catch
                    {

                    }
                }
                throw new Exception("None of the control could be located");
            });

        }
        catch
        {

        }

        if(foundControl != null)
        {
            return;
        }

        await argumentProcessor.SetValueAsync<bool>(this.Exists, false);
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
        var groupEntity = new GroupEntity()
        {
            Name = "Find First Control",
            Tag = "FindFirstControlsGroup",
            GroupActor = new FindFirstControlActorComponent()
        };          
        groupEntity.GroupPlaceHolder.AllowedComponentsType = typeof(IControlEntity).Name;
        groupEntity.GroupPlaceHolder.Name = "Control";
        return groupEntity;
    }
}
