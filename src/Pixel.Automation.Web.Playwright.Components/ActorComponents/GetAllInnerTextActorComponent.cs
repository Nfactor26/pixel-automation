using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="GetAllInnerTextActorComponent"/> to retreive values for all matching nodes
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Get All Inner Text", "Playwright", iconSource: null, description: "Retrieve innerText values for all matching nodes", tags: new string[] { "inner text", "all", "inner", "text", "Web" })]

public class GetAllInnerTextActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<GetInnerTextActorComponent>();

    /// <summary>
    /// Output argument to store the retrieved inner texts
    /// </summary>
    [DataMember]
    [Display(Name = "Result", GroupName = "Output", Order = 10, Description = "Output argument to store the retrieved inner texts")]
    public Argument Result { get; set; } = new OutArgument<IReadOnlyList<string>>() { Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public GetAllInnerTextActorComponent() : base("Get All Inner Text", "GetAllInnerText")
    {

    }

    /// <summary>
    /// Retrieve innerText values for all matching nodes using AllInnerTextsAsync()
    /// </summary>
    public override async Task ActAsync()
    {       
        var (name, control) = await GetTargetControl();
        var result = await control.AllInnerTextsAsync();
        await this.ArgumentProcessor.SetValueAsync<IReadOnlyList<string>>(this.Result, result);
        logger.Information("Retrieved innerText values for all matching nodes for control : '{1}'", name);
    }
}
