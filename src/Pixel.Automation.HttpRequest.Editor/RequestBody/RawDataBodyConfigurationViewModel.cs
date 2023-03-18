using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.RestApi.Shared;
using RestHttpRequest = Pixel.Automation.RestApi.Shared.HttpRequest;

namespace Pixel.Automation.HttpRequest.Editor;

/// <summary>
/// Configures http request for raw data in body
/// </summary>
public class RawDataBodyConfigurationViewModel : Screen, IConfigurationScreen
{
    private readonly RestHttpRequest httpRequest;

    /// <summary>
    /// Actor component for executing the rest request
    /// </summary>
    public Component OwnerComponent { get; private set; }       
   
    /// <summary>
    /// Raw body content
    /// </summary>
    public RawBodyContent RequestBody { get; private set; }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="ownerComponent"></param>
    /// <param name="httpRequest"></param>
    public RawDataBodyConfigurationViewModel(Component ownerComponent, RestHttpRequest httpRequest)
    {
        this.DisplayName = "Raw";
        this.OwnerComponent = Guard.Argument(ownerComponent, nameof(ownerComponent)).NotNull();
        this.httpRequest = Guard.Argument(httpRequest, nameof(httpRequest)).NotNull();
    }

    /// </inheritdoc>
    public void ApplyChanges(RestHttpRequest request)
    {
       
    }

    /// </inheritdoc>
    protected override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        if (this.httpRequest.RequestBody == null || this.httpRequest.RequestBody.GetType() != typeof(RawBodyContent))
        {
            this.httpRequest.RequestBody = new RawBodyContent();
        }
        if (this.httpRequest.RequestBody is RawBodyContent rawBodyContent)
        {
            this.RequestBody = rawBodyContent;
        }
        return base.OnActivateAsync(cancellationToken);
    }
}

