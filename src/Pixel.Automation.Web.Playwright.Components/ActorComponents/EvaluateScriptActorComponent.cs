using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="EvaluateScriptActorComponent"/> to evaluate javascript
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Evaluate Javascript", "Playwright", iconSource: null, description: "Evaluate javascript in browser and return a value if any", tags: new string[] { "avasscript", "Web" })]
public class EvaluateScriptActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<EvaluateScriptActorComponent>();

    /// <summary>
    /// Input argument to provide javascript to be evaulated
    /// </summary>
    [DataMember]
    [Display(Name = "JavaScript", GroupName = "Configuration", Order = 10, Description = "Input argument to provide javascript to be evaluated.")]     
    public Argument Script { get; set; } = new InArgument<string>() { CanChangeType = false, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Input argument to provide additional arguments to be passed to javascript.   
    /// </summary>
    [DataMember]
    [Display(Name = "Arguments", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for additional arguments to be passed to javascript")]      
    public Argument Arguments { get; set; } = new InArgument<object>() { Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Output argument to store the result of the evaluate javascript operation
    /// </summary>
    [DataMember]
    [Display(Name = "Result", GroupName = "Output", Order = 10 , Description = "Output argument to store the result of evaluate javascript operation")]       
    public Argument Result { get; set; } = new OutArgument<string>();

    /// <summary>
    /// Constructor
    /// </summary>
    public EvaluateScriptActorComponent() : base("Evaluate Javascript", "EvaluateJavaScript")
    {

    }

    /// <summary>
    /// Evaluate the javascript using EvaluateAsync() method and store the result
    /// </summary>
    public override async Task ActAsync()
    {           
        string jsCode = await ArgumentProcessor.GetValueAsync<string>(this.Script);      
        var result = await this.ApplicationDetails.ActivePage.EvaluateAsync<string>(jsCode, this.Arguments.IsConfigured() ? await ArgumentProcessor.GetValueAsync<object>(this.Arguments) : null );
        await ArgumentProcessor.SetValueAsync<string>(this.Result, result);
        logger.Information("javascript executed successfully.");
    }

    ///</inheritdoc>
    public override string ToString()
    {
        return "Evaluate Javscript Actor";
    }

}
