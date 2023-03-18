namespace Pixel.Automation.RestApi.Shared;

/// <summary>
/// Contract for executing a http request
/// </summary>
public interface IHttpRequestExecutor
{
    /// <summary>
    /// Indicates if http request can be executed
    /// </summary>
    bool CanExecuteRequest { get; }

    /// <summary>
    /// Execute the http request asynchronously and return the <see cref="HttpResponse"/>
    /// </summary>
    /// <param name="httpRequest"></param>
    /// <param name="responseContentSettings"></param>
    /// <returns></returns>
    Task<HttpResponse> ExecuteRequestAsync(HttpRequest httpRequest, ResponseContentHandling responseContentSettings);
}
