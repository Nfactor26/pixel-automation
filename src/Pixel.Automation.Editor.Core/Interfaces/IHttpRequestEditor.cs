namespace Pixel.Automation.Editor.Core.Interfaces;

/// <summary>
/// Editor for configuring Http Request
/// </summary>
public interface IHttpRequestEditor
{
    /// <summary>
    /// Initialize the editor for configuring http request
    /// </summary>
    /// <param name="ownerComponent">Actor component that will execute the Http Request</param>
    /// <param name="httpRequest">HttpRequest configuration</param>
    /// <param name="responseContentSettings">HttpResponse content configuration</param>
    void Initialize(object ownerComponent, object httpRequest, object responseContentSettings);
}
