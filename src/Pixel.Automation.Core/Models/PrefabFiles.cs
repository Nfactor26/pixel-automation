using Dawn;
using System.IO;

namespace Pixel.Automation.Core.Models;

/// <summary>
/// Each prefab has essentially these files. There can be additional scripts associated with components belonging to process.
/// </summary>
public class PrefabFiles
{
    public string PrefabDirectory { get; private set; }

    public string PrefabFile { get; private set; }

    public string ProcessFile { get; private set; }

    public string ScriptFile { get; private set; }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="prefab">Prefab</param>
    /// <param name="workingDirectory">Complete path for the Prefab version</param>
    public PrefabFiles(Prefab prefab, string workingDirectory)
    {
        Guard.Argument(prefab).NotNull();
        Guard.Argument(workingDirectory).NotNull().NotEmpty().NotWhiteSpace();
        PrefabDirectory = workingDirectory;
        if (!Directory.Exists(PrefabDirectory))
        {
            Directory.CreateDirectory(PrefabDirectory);
        }
        PrefabFile = Path.Combine(PrefabDirectory, $"{prefab.PrefabId}.prefab");
        ProcessFile = Path.Combine(PrefabDirectory, $"{prefab.PrefabId}.proc");
        ScriptFile = Path.Combine(PrefabDirectory, $"{prefab.PrefabId}.csx");
    }
}
