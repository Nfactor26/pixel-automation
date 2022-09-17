using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="GetAllTextContentActorComponent"/> to retrieve textContent values for all matching nodes.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Get All Text Content", "Playwright", iconSource: null, description: "Retrieve textContent values for all matching nodes.", tags: new string[] { "text content", "all", "text", "content", "Web" })]

public class GetAllTextContentActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<GetInnerTextActorComponent>();

    /// <summary>
    /// Output argument to store the  retrieved text contents
    /// </summary>
    [DataMember]
    [Display(Name = "Result", GroupName = "Output", Order = 10, Description = "Output argument to store the retrieved text contents")]
    public Argument Result { get; set; } = new OutArgument<IReadOnlyList<string>>() { Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Constructor
    /// </summary>
    public GetAllTextContentActorComponent() : base("Get All Text Content", "GetAllTextContent")
    {

    }

    /// <summary>
    /// Retrieve Retrieve textContent values for all matching nodes using AllTextContentsAsync()
    /// </summary>
    public override async Task ActAsync()
    {       
        var control = await GetTargetControl();
        var result = await control.AllTextContentsAsync();
        await this.ArgumentProcessor.SetValueAsync<IReadOnlyList<string>>(this.Result, result);
        logger.Information($"Retrieved textContent values for all matching nodes.");
    }
}
