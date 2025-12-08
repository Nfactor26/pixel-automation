using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.RestApi.Shared;
using RestSharp;
using System.Reflection;

namespace Pixel.Automation.RestApi.Components;

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
                Path.GetFileName(typeof(RestClient).Assembly.Location)
            ),
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                Path.GetFileName(typeof(IHttpRequestExecutor).Assembly.Location)
            )
        ];
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetImports()
    {
        return
        [
            "Pixel.Automation.RestApi.Components",
            "Pixel.Automation.RestApi.Shared",
            "RestSharp",
            "RestSharp.Authenticators.OAuth2",
            "RestSharp.Authenticators"
        ];
    }
}
