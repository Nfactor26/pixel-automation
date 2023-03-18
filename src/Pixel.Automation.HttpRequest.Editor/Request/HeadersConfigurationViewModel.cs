using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.RestApi.Shared;

namespace Pixel.Automation.HttpRequest.Editor;

/// <summary>
/// View model for configuring the <see cref="RequestHeader"/>
/// </summary>
public class HeadersConfigurationViewModel : Screen, IConfigurationScreen
{
    /// <summary>
    /// Actor component for executing the rest request
    /// </summary>
    public Component OwnerComponent { get; set; }

    /// <summary>
    /// Collection of RequestHeaders visible on screen
    /// </summary>
    public BindableCollection<RequestHeader> Headers { get; set; } = new();
    
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="ownerComponent"></param>
    /// <param name="headers"></param>
    public HeadersConfigurationViewModel(Component ownerComponent, List<RequestHeader> headers)
    {
        this.DisplayName = "Headers";
        this.OwnerComponent = Guard.Argument(ownerComponent, nameof(ownerComponent));
        Guard.Argument(headers, nameof(headers)).NotNull();
        if (headers.Any())
        {
            this.Headers.AddRange(headers);
        }
        else
        {
            this.AddRequestHeader();
        }
    }

    /// <summary>
    /// Add a new RequestHeader row
    /// </summary>
    public void AddRequestHeader()
    {
        this.Headers.Add(new RequestHeader());       
    }

    /// <summary>
    /// Delete an existing RequestHeader row
    /// </summary>
    /// <param name="requestHeader"></param>
    public void DeleteRequestHeader(RequestHeader requestHeader)
    {
        this.Headers.Remove(requestHeader);       
    }

    /// </inheritdoc>
    public void ApplyChanges(RestApi.Shared.HttpRequest request)
    {
        request.RequestHeaders.Clear();
        if (this.Headers.Any())
        {
            request.RequestHeaders.AddRange(this.Headers);
        }
    }
}
