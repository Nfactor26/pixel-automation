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
[ToolBoxItem("Copy", "System.IO", iconSource: null, description: "Copies a file or directory to a target location.", tags: new string[] { "Copy", "File", "Directory" })]
public class CopyActorComponent : IOActorComponent
{
    private readonly ILogger logger = Log.ForContext<CopyActorComponent>();

    [DataMember]
    [DisplayName("Source Path")]
    [Category("Input")]
    public Argument SourcePath { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = string.Empty };

    [DataMember]
    [DisplayName("Target Path")]
    [Category("Input")]
    public Argument TargetPath { get; set; } = new InArgument<string>() { Mode = ArgumentMode.Default, DefaultValue = string.Empty };

    [DataMember]
    [DisplayName("Overwrite")]
    [Category("Input")]
    [Description("If true, existing file will be overwritten")]
    public Argument Overwrite { get; set; } = new InArgument<bool>() { Mode = ArgumentMode.Default, DefaultValue = false };

    [DataMember]
    [DisplayName("Recursive Copy")]
    [Category("Input")]
    [Description("If true, any subdirectories and files will be recursively copied")]
    public Argument Recursive { get; set; } = new InArgument<bool>() { Mode = ArgumentMode.Default, DefaultValue = false };


    public CopyActorComponent() : base("Copy", "Copy")
    {
        
    }

    public override async Task ActAsync()
    {
        IArgumentProcessor argumentProcessor = this.ArgumentProcessor;

        // Retrieve input arguments
        string sourcePath = await argumentProcessor.GetValueAsync<string>(this.SourcePath);
        string targetPath = await argumentProcessor.GetValueAsync<string>(this.TargetPath);
        bool overwrite = await argumentProcessor.GetValueAsync<bool>(this.Overwrite);
        bool recursive = await argumentProcessor.GetValueAsync<bool>(this.Recursive);

        ThrowIfPathNotExists(sourcePath);

        if (File.Exists(sourcePath))
        {
            File.Copy(sourcePath, targetPath, overwrite);
            logger.Information("File copied from {0} to {1}", sourcePath, targetPath);
        }
        else if (Directory.Exists(sourcePath))
        {
            // Copy directory
            CopyDirectory(sourcePath, targetPath, recursive, overwrite);
            logger.Information("Directory copied from {0} to {1} (Recursive: {2})", sourcePath, targetPath, recursive);
        }
    }

    void CopyDirectory(string sourceDir, string targetDir, bool recursive, bool overwrite)
    {
        // Create the target directory if it doesn't exist
        Directory.CreateDirectory(targetDir);

        // Copy files in the current directory
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            string targetFilePath = Path.Combine(targetDir, Path.GetFileName(file));
            File.Copy(file, targetFilePath, overwrite);
        }

        // If recursive, copy subdirectories
        if (recursive)
        {
            foreach (var subDir in Directory.GetDirectories(sourceDir))
            {
                string targetSubDir = Path.Combine(targetDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, targetSubDir, recursive, overwrite);
            }
        }
    }
}
