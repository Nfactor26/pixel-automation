using Pixel.Automation.Core.Interfaces;
using System.Collections.Generic;
using System.Reflection;

namespace Pixel.Automation.Java.Access.Bridge.Components;

/// <inheritdoc/>
internal class ScriptReferencesProvider : IScriptReferencesProvider
{
    /// <inheritdoc/>
    public IEnumerable<string> GetAssemblyReferences()
    {
        return
        [
            Assembly.GetExecutingAssembly().Location
        ];
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetImports()
    {
        return
        [
            "Pixel.Automation.Java.Access.Bridge.Components",
            "WindowsAccessBridgeInterop"
        ];
    }
}
