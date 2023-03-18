using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.RestApi.Shared;
using RestHttpRequest = Pixel.Automation.RestApi.Shared.HttpRequest;

namespace Pixel.Automation.HttpRequest.Editor;

/// <summary>
/// Configures http request for binary data in body
/// </summary>
public class BinaryDataBodyConfigurationViewModel : Screen, IConfigurationScreen
{
    private readonly RestHttpRequest httpRequest;

    /// <summary>
    /// Actor component for executing the rest request
    /// </summary>
    public Component OwnerComponent { get; private set; }
    
    /// <summary>
    /// Files to be added in body of the request
    /// </summary>
    public BinaryBodyContent RequestBody { get; private set; }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="ownerComponent"></param>
    /// <param name="httpRequest"></param>
    public BinaryDataBodyConfigurationViewModel(Component ownerComponent, RestHttpRequest httpRequest)
    {
        this.DisplayName = "Binary Data";
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
        if (this.httpRequest.RequestBody == null || this.httpRequest.RequestBody.GetType() != typeof(BinaryBodyContent))
        {
            this.httpRequest.RequestBody = new BinaryBodyContent();
        }
        if (this.httpRequest.RequestBody is BinaryBodyContent binaryBodyContent)
        {
            this.RequestBody = binaryBodyContent;
        }
        return base.OnActivateAsync(cancellationToken);
    }
}
