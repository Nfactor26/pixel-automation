using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.RestApi.Shared;
using RestHttpRequest = Pixel.Automation.RestApi.Shared.HttpRequest;

namespace Pixel.Automation.HttpRequest.Editor;

/// <summary>
/// Configures http request with empty body
/// </summary>
public class EmptyDataBodyConfigurationViewModel : Screen, IConfigurationScreen
{
    private readonly RestHttpRequest httpRequest;

    /// <summary>
    /// Actor component for executing the rest request
    /// </summary>
    public Component OwnerComponent { get; private set; }
  
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="ownerComponent"></param>
    /// <param name="httpRequest"></param>
    public EmptyDataBodyConfigurationViewModel(Component ownerComponent, RestHttpRequest httpRequest)
    {
        this.DisplayName = "None";
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
        if (this.httpRequest.RequestBody == null || this.httpRequest.RequestBody.GetType() != typeof(EmptyBodyContent))
        {
            this.httpRequest.RequestBody = new EmptyBodyContent();
        }       
        return base.OnActivateAsync(cancellationToken);
    }
}
