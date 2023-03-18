using Caliburn.Micro;
using Pixel.Automation.RestApi.Shared;

namespace Pixel.Automation.HttpRequest.Editor;

/// <summary>
/// View model for displaying the response headers
/// </summary>
public  class HttpResponseHeadersViewModel : Screen, IResultsScreen
{
    /// <summary>
    /// Received response after executing an http request
    /// </summary>
    public HttpResponse Response { get; private set; } = new();
  
    /// <summary>
    /// constructor
    /// </summary>
    public HttpResponseHeadersViewModel()
    {
        this.DisplayName = "Headers";
    }

    /// </inheritdoc>
    public void ProcessResult(HttpResponse response)
    {
        this.Response = response;
        NotifyOfPropertyChange(() => Response);
    }
}
