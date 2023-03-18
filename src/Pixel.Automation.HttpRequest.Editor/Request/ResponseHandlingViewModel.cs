using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.RestApi.Shared;

namespace Pixel.Automation.HttpRequest.Editor;

public class ResponseHandlingViewModel : Screen, IConfigurationScreen
{
    /// <summary>
    /// Actor component for executing the rest request
    /// </summary>
    public Component OwnerComponent { get; private set; }    
  
    /// <summary>
    /// Configuration of response content handling
    /// </summary>
    public ResponseContentHandling ResponseContentSettings { get; private set; }
   
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="ownerComponent"></param>
    /// <param name="responseContentSettings"></param>
    public ResponseHandlingViewModel(Component ownerComponent, ResponseContentHandling responseContentSettings)
    {
        this.DisplayName = "Response Handling";
        this.OwnerComponent = Guard.Argument(ownerComponent, nameof(ownerComponent));
        this.ResponseContentSettings = Guard.Argument(responseContentSettings, nameof(responseContentSettings));
    }

    /// </inheritdoc>
    public void ApplyChanges(RestApi.Shared.HttpRequest request)
    {
       
    }
}
