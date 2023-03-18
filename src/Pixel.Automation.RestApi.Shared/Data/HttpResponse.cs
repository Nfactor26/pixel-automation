using System.Net;

namespace Pixel.Automation.RestApi.Shared;

/// <summary>
/// Response received on executing an <see cref="HttpRequest"/>
/// </summary>
public class HttpResponse
{
    HttpStatusCode statusCode;
    /// <summary>
    /// HTTP response status code
    /// </summary>
    public HttpStatusCode StatusCode
    {
        get => statusCode;
        set => statusCode = value;
    }

    Uri responseUri;
    /// <summary>
    /// The URL that actually responded to the content (different from request if redirected)
    /// </summary>
    public Uri ResponseUri
    {
        get => responseUri;
        set => responseUri = value;
    }

    string contentType;
    /// <summary>
    /// MIME content type of response
    /// </summary>
    public string ContentType
    {
        get => contentType;
        set => contentType = value;
    }

    long? contentLength;
    /// <summary>
    /// Length in bytes of the response content
    /// </summary>
    public long? ContentLength
    {
        get => contentLength;
        set => contentLength = value;
    }

    ICollection<string> contentEncoding;
    /// <summary>
    /// Encoding of the response content
    /// </summary>
    public ICollection<string> ContentEncoding
    {
        get => contentEncoding;
        set => contentEncoding = value;
    }

    string content;
    /// <summary>
    /// String representation of response content
    /// </summary>
    public string Content
    {
        get => content;
        set => content = value;
    }

    byte[] rawBytes;
    /// <summary>
    /// Response content
    /// </summary>
    public byte[] RawBytes
    {
        get => rawBytes;
        set => rawBytes = value;
    }

    List<Cookie> cookies = new List<Cookie>();
    /// <summary>
    /// Cookies returned by server with the response
    /// </summary>
    public List<Cookie> Cookies
    {
        get => cookies;
        set => cookies = value;
    }

    List<ResponseHeader> headers = new List<ResponseHeader>();
    /// <summary>
    /// Headers returned by server with the response
    /// </summary>
    public List<ResponseHeader> Headers
    {
        get => headers;
        set => headers = value;
    }

    /// <summary>
    /// constructor
    /// </summary>
    public HttpResponse()
    {
        
    }
}

public class ResponseHeader
{
    string headerKey;
    /// <summary>
    /// Name of the header
    /// </summary>
    public string HeaderKey
    {
        get => headerKey;
        set => headerKey = value;
    }

    string headerValue;
    /// <summary>
    /// Value of the header
    /// </summary>
    public string HeaderValue
    {
        get => headerValue;
        set => headerValue = value;
    }
}
