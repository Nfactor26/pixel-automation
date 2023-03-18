using Pixel.Automation.RestApi.Shared;

namespace Pixel.Automation.HttpRequest.Editor;

/// <summary>
/// Result screen will process HttpResponse and display details about it
/// </summary>
public interface IResultsScreen
{
    /// <summary>
    /// Process the HttpResponse received to display it
    /// </summary>
    /// <param name="response"></param>
    public void ProcessResult(HttpResponse response);
}
