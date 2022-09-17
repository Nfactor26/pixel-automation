using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="SelectOptionsActorComponent"/> to select an option.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Select Options", "Playwright", iconSource: null, description: "Selects one or multiple options in the <select> element.", tags: new string[] { "select", "Web" })]

public class SelectOptionsActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<SelectOptionsActorComponent>();

    /// <summary>
    /// Optional input argument for <see cref="LocatorSelectOptionOptions"/> that can be used to customize the select operation
    /// </summary>
    [DataMember]
    [Display(Name = "Select Options", GroupName = "Configuration", Order = 10, Description = "[Optional] Input argument for LocatorSelectOptionOptions")]
    public Argument SelectOptions { get; set; } = new InArgument<LocatorSelectOptionOptions>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };


    /// <summary>
    /// Input argument to provide one or more values to be selected.
    /// </summary>
    [DataMember]
    [Display(Name = "Value/s", GroupName = "Configuration", Order = 20, Description = "one or more values to select")]
    [AllowedTypes(typeof(string), typeof(IEnumerable<string>), typeof(SelectOptionValue), typeof(IEnumerable<SelectOptionValue>), typeof(IElementHandle), typeof(IEnumerable<IElementHandle>))]
    public Argument Values { get; set; } = new InArgument<string>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound, CanChangeType = true };


    /// <summary>
    /// Constructor
    /// </summary>
    public SelectOptionsActorComponent() : base("Select Options", "SelectOptions")
    {

    }

    /// <summary>
    /// Select option using SelectOptionAsync() method.
    /// </summary>
    public override async Task ActAsync()
    {
            
        var control = await GetTargetControl();
        var selectOptions = this.SelectOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorSelectOptionOptions>(this.SelectOptions) : null;
        switch(this.Values)
        {
            case InArgument<string>:
                await control.SelectOptionAsync(await this.ArgumentProcessor.GetValueAsync<string>(this.Values), selectOptions);
                break;
            case InArgument<IEnumerable<string>>:
                await control.SelectOptionAsync(await this.ArgumentProcessor.GetValueAsync<IEnumerable<string>>(this.Values), selectOptions);
                break;
            case InArgument<SelectOptionValue>:
                await control.SelectOptionAsync(await this.ArgumentProcessor.GetValueAsync<SelectOptionValue>(this.Values), selectOptions);
                break;
            case InArgument<IEnumerable<SelectOptionValue>>:
                await control.SelectOptionAsync(await this.ArgumentProcessor.GetValueAsync<IEnumerable<SelectOptionValue>>(this.Values), selectOptions);
                break;
            case InArgument<IElementHandle>:
                await control.SelectOptionAsync(await this.ArgumentProcessor.GetValueAsync<IElementHandle>(this.Values), selectOptions);
                break;
            case InArgument<IEnumerable<IElementHandle>>:
                await control.SelectOptionAsync(await this.ArgumentProcessor.GetValueAsync<IEnumerable<IElementHandle>>(this.Values), selectOptions);
                break;
            default:
                throw new ArgumentException("Invalid type for Value. Supported types are string, SelectOptionValue, IElementHandler, IEnumerable<string>, IEnumerable<SelectOptionValue>, IEnumerable<IElementHandle>");

        }
       
        logger.Information("Select options performed on element.");
        
    }

}
