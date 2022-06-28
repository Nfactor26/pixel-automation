using Microsoft.Playwright;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pixel.Automation.Web.Playwright.Components;

/// <summary>
/// Use <see cref="SetInputFileActorComponent"/> to select input files for upload.
/// </summary>
[DataContract]
[Serializable]
[ToolBoxItem("Input File", "Playwright", iconSource: null, description: "Select input files for upload", tags: new string[] { "input" , "file", "Web" })]

public class SetInputFileActorComponent : PlaywrightActorComponent
{
    private readonly ILogger logger = Log.ForContext<DoubleClickActorComponent>();

    /// <summary>
    /// Input argument to specify input files for upload
    /// </summary>
    [DataMember]
    [Display(Name = "Input Files", GroupName = "Configuration", Order = 10, Description = "Input argument for input files for upload")]
    public Argument InputFiles { get; set; } = new InArgument<string>() { CanChangeType = true, Mode = ArgumentMode.DataBound };


    /// <summary>
    /// Optional input argument for <see cref="LocatorSetInputFilesOptions"/> that can be used to customize the set input file operation
    /// </summary>
    [DataMember]
    [Display(Name = "Input Files Options", GroupName = "Configuration", Order = 20, Description = "[Optional] Input argument for LocatorSetInputFilesOptions")]
    public Argument SetInputFilesOptions { get; set; } = new InArgument<LocatorSetInputFilesOptions>() { CanChangeType = false, Mode = ArgumentMode.DataBound };


    /// <summary>
    /// Constructor
    /// </summary>
    public SetInputFileActorComponent() : base("InputFile", "InputFile")
    {

    }

    /// <summary>
    /// Set input files for upload using SetInputFileAsync() method
    /// </summary>
    public override async Task ActAsync()
    {
        var setInputFilesOptions = this.SetInputFilesOptions.IsConfigured() ? await this.ArgumentProcessor.GetValueAsync<LocatorSetInputFilesOptions>(this.SetInputFilesOptions) : null;
        var control = await GetTargetControl();
        switch (this.InputFiles)
        {
            case InArgument<string>:
                await control.SetInputFilesAsync(await this.ArgumentProcessor.GetValueAsync<IEnumerable<string>>(this.InputFiles) , setInputFilesOptions);
                break;
            case InArgument<IEnumerable<string>>:
                await control.SetInputFilesAsync(await this.ArgumentProcessor.GetValueAsync<IEnumerable<string>>(this.InputFiles), setInputFilesOptions);
                break;
            case InArgument<FilePayload>:
                await control.SetInputFilesAsync(await this.ArgumentProcessor.GetValueAsync<FilePayload>(this.InputFiles), setInputFilesOptions);
                break;
            case InArgument<IEnumerable<FilePayload>>:
                await control.SetInputFilesAsync(await this.ArgumentProcessor.GetValueAsync<IEnumerable<FilePayload>>(this.InputFiles), setInputFilesOptions);
                break;            
            default:
                throw new ArgumentException("Invalid type for Value. Supported types are string, SelectOptionValue, IElementHandler, IEnumerable<string>, IEnumerable<SelectOptionValue>, IEnumerable<IElementHandle>");

        }       
       
        logger.Information("Input files were set.");
    }

    ///</inheritdoc>
    public override string ToString()
    {
        return "Set Input File Actor";
    }
}
