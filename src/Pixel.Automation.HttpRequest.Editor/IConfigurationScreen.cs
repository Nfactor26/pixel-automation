namespace Pixel.Automation.HttpRequest.Editor;

/// <summary>
/// Configuration Screens are used to configure the http reuquest
/// </summary>
public interface IConfigurationScreen
{
    /// <summary>
    /// Apply changes back to the HttpRequest
    /// </summary>
    /// <param name="request"></param>
    void ApplyChanges(RestApi.Shared.HttpRequest request);
}
