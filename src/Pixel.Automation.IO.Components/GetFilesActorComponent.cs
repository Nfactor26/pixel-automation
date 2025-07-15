using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.IO.Components;

[DataContract]
[Serializable]
[ToolBoxItem("Get Files", "System.IO", "File", iconSource: null, description: "Returns the names of files that meet specified criteria.", tags: new string[] { "Get Files" })]
public class GetFilesActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<GetDirectoriesActorComponent>();

    [DataMember]
    [DisplayName("Path")]
    [Category("Input")]
    [Description("The relative or absolute path to the directory to search. This string is not case-sensitive.")]
    public Argument Path { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = String.Empty };

    [DataMember]
    [DisplayName("Search Pattern")]
    [Category("Input")]
    [Description("The search string to match against the names of subdirectories in path")]
    public Argument SearchPattern { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = String.Empty };

    [DataMember]
    [DisplayName("Enumeration Options")]
    [Category("Input")]
    [Description("An object that describes the search and enumeration configuration to use.")]
    public Argument EnumerationOptions { get; set; } = new InArgument<EnumerationOptions>() { Mode = ArgumentMode.DataBound };

    [DataMember]
    [DisplayName("Files Result")]
    [Category("Output")]
    [Description("Collection of the full names (including paths) for the files in the specified directory, or an empty array if no files are found.")]
    public Argument FilesResult { get; set; } = new InArgument<IEnumerable<string>>() { Mode = ArgumentMode.DataBound };

    public GetFilesActorComponent() : base("Get Files", "GetFiles")
    {

    }

    public override async Task ActAsync()
    {
        List<string> directories = new();

        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;
        string path = await argumentProcessor.GetValueAsync<string>(this.Path);
      
        if (this.SearchPattern.IsConfigured())
        {
            string searchPattern = await argumentProcessor.GetValueAsync<string>(this.SearchPattern);
           
            if(this.EnumerationOptions.IsConfigured())
            {
                EnumerationOptions options = await argumentProcessor.GetValueAsync<EnumerationOptions>(this.EnumerationOptions);
                directories.AddRange(Directory.GetFiles(path, searchPattern, options));
                await SetResult(directories);
                return;
            }
           
            directories.AddRange(Directory.GetFiles(path, searchPattern));
            await SetResult(directories);
            return;
        }
        directories.AddRange(Directory.GetFiles(path));
        await SetResult(directories);
       
        async Task SetResult(IEnumerable<string> result)
        {
            await argumentProcessor.SetValueAsync(this.FilesResult, result);
            logger.Information("{0} Files found at Path : {1}", result.Count(), path);
        }
    }
}
