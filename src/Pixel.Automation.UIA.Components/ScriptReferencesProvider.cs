using Pixel.Automation.Core.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Pixel.Automation.UIA.Components;

/// <inheritdoc/>
internal class ScriptReferencesProvider : IScriptReferencesProvider
{
    /// <inheritdoc/>
    public IEnumerable<string> GetAssemblyReferences()
    {
        return
        [
            Assembly.GetExecutingAssembly().Location,
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                Path.GetFileName(typeof(Pixel.Windows.Automation.AutomationElement).Assembly.Location) 
             )
        ];
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetImports()
    {
        return
        [
            "Pixel.Automation.UIA.Components",
            "Pixel.Automation.UIA.Components.Enums",
            "Pixel.Windows.Automation"
        ];
    }
}
