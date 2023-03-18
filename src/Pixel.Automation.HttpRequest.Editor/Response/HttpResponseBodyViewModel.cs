using Caliburn.Micro;
using ICSharpCode.AvalonEdit.Document;
using Pixel.Automation.RestApi.Shared;
using System.Text.Json;

namespace Pixel.Automation.HttpRequest.Editor;

/// <summary>
/// View model for displaying the http response content
/// </summary>
public class HttpResponseBodyViewModel : Screen, IResultsScreen
{
    private readonly ResponseContentHandling responseContentSettings;

    /// <summary>
    /// Received response after executing an http request
    /// </summary>
    public HttpResponse Response { get; private set; } = new();
   
    /// <summary>
    /// TextDocument for the AvalonEdit control
    /// </summary>
    public TextDocument Document { get; private set; } = new();
  
    private string responseType;
    /// <summary>
    /// Response Type configured
    /// </summary>
    public string ResponseType
    {
       get => this.responseType;
        set
        {
            this.responseType = value;
            NotifyOfPropertyChange();
        }
    }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="responseContentSettings"></param>
    public HttpResponseBodyViewModel(ResponseContentHandling responseContentSettings)
    {
        this.DisplayName = "Body";
        this.responseContentSettings = responseContentSettings;
    }

    /// </inheritdoc>
    public void ProcessResult(HttpResponse response)
    {
        this.Response = response;
        switch (responseContentSettings?.ExpectedResponseType)
        {
            case ExpectedResponse.Text:
            case ExpectedResponse.Xml:
            case ExpectedResponse.Json:
                if (this.Response.ContentType == "application/json")
                {
                    this.ResponseType = "Json";
                    this.Document.Text = JsonPrettify(response?.Content ?? string.Empty);
                }
                else if (this.Response.ContentType == "application/xml")
                {
                    this.ResponseType = "XML";
                    this.Document.Text = response?.Content ?? string.Empty;
                }
                else
                {
                    this.Document.Text = response?.Content ?? string.Empty;
                }
                break;
          default:
                this.Document.Text = response?.Content ?? string.Empty;
                break;
        }
                  
        NotifyOfPropertyChange(() => Response);
        NotifyOfPropertyChange(() => ResponseType);           
    }

    static string JsonPrettify(string json)
    {
        if(string.IsNullOrEmpty(json))
        {
            return json;
        }
        using var jDoc = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(jDoc, new JsonSerializerOptions { WriteIndented = true });
    }
}
