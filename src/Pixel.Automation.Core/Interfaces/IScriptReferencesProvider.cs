using System.Collections.Generic;

namespace Pixel.Automation.Core.Interfaces;

/// <summary>
/// Provides required scripting references for script editors and script engines.
/// </summary>
public interface IScriptReferencesProvider
{
    /// <summary>
    /// Provides assembly references needed for the plugin for script editor and script engines.
    /// </summary>
    /// <returns></returns>
    IEnumerable<string> GetAssemblyReferences();

    /// <summary>
    /// Provides import namespaces needed for the plugin for script editor and script engines.
    /// </summary>
    /// <returns></returns>
    IEnumerable<string> GetImports();
}

