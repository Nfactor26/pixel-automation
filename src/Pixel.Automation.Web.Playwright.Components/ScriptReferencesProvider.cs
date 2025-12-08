using Microsoft.Playwright;
using Pixel.Automation.Core.Interfaces;
using System.Reflection;

namespace Pixel.Automation.Web.Playwright.Components;

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
                Path.GetFileName(typeof(ILocator).Assembly.Location)
             )
        ];
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetImports()
    {
        return
        [
            "Pixel.Automation.Web.Playwright.Components",
            "Microsoft.Playwright"
        ];
    }
}
