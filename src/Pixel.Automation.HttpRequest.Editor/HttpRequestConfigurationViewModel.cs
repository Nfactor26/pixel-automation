using Caliburn.Micro;
using Dawn;
using Notifications.Wpf.Core;
using Pixel.Automation.Core;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.RestApi.Shared;
using Serilog;
using RestHttpRequest = Pixel.Automation.RestApi.Shared.HttpRequest;

namespace Pixel.Automation.HttpRequest.Editor;

/// <summary>
/// View model for configuring an http request
/// </summary>
public class HttpRequestConfigurationViewModel : SmartScreen, IHttpRequestEditor
{
    private readonly ILogger logger = Log.ForContext<HttpRequestConfigurationViewModel>();
    private readonly INotificationManager notificationManager;

    RestHttpRequest request;
    public RestHttpRequest Request
    {
        get => request;
    }

    ResponseContentHandling responseContentSettings;
    public ResponseContentHandling ResponseContentSettings
    {
        get => this.responseContentSettings;
    }

    HttpResponse response;

    /// <summary>
    /// Indicates if there response has some value
    /// </summary>
    public bool HasResponse
    {
        get => this.response != null;
    }
   
    Component ownerComponent;
    public Component OwnerComponent
    {
        get => ownerComponent;
    }

    /// <summary>
    /// Available screens under request tab
    /// </summary>
    public BindableCollection<Screen> RequestScreens { get; private set; } = new();

    /// <summary>
    /// Available screens under response tab
    /// </summary>
    public BindableCollection<Screen> ResponseScreens { get; private set; } = new();

    private int selectedIndex = 0;
    /// <summary>
    /// Index of selected Tab
    /// </summary>
    public int SelectedIndex
    {
        get => this.selectedIndex;
        set
        {
            this.selectedIndex = value;
            NotifyOfPropertyChange();
        }
    }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="notificationManager"></param>
    public HttpRequestConfigurationViewModel(INotificationManager notificationManager)
    {
        this.DisplayName = "Request Configuration";
        this.notificationManager = Guard.Argument(notificationManager, nameof(notificationManager)).NotNull().Value;
    }

    /// <summary>
    /// Initialize the view model
    /// </summary>
    /// <param name="httpRequest"></param>
    /// <param name="responseContentSettings"></param>
    /// <param name="ownerComponent"></param>
    public void Initialize(object httpRequest, object responseContentSettings, object ownerComponent)
    {
        this.ownerComponent = ownerComponent as Component;

        this.request = httpRequest as RestHttpRequest;
        this.responseContentSettings = responseContentSettings as ResponseContentHandling;
     
        this.RequestScreens.Add(new HeadersConfigurationViewModel(this.ownerComponent, request.RequestHeaders));
        this.RequestScreens.Add(new QueryStringsConfigurationViewModel(this.ownerComponent, request.RequestParameters));
        this.RequestScreens.Add(new PathSegmentsConfigurationViewModel(this.ownerComponent, request.PathSegments));
        this.RequestScreens.Add(new RequestBodyConfigurationViewModel(this.ownerComponent, request));
        this.RequestScreens.Add(new ResponseHandlingViewModel(this.ownerComponent, this.responseContentSettings));

        this.ResponseScreens.Add(new HttpResponseBodyViewModel(this.responseContentSettings));
        this.ResponseScreens.Add(new HttpResponseHeadersViewModel());          
    }

    public bool CanSendRequest
    {
        get
        {
            if (this.ownerComponent is IHttpRequestExecutor requestExecutor)
            {
              return requestExecutor.CanExecuteRequest;
            }
            return false;
        }
    }

    /// <summary>
    /// Execute the request
    /// </summary>
    /// <returns></returns>
    public async Task SendRequest()
    {
        try
        {
            if (this.ownerComponent is IHttpRequestExecutor requestExecutor)
            {
                this.response = await requestExecutor.ExecuteRequestAsync(this.request, this.responseContentSettings);
                foreach(var screen in this.ResponseScreens)
                {
                    if(screen is IResultsScreen resultsScreen)
                    {
                        resultsScreen.ProcessResult(this.response);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            this.response = new HttpResponse() { Content = ex.Message };
            logger.Error(ex, "There was an error while trying to execute http request");
            await this.notificationManager.ShowErrorNotificationAsync(ex);
        }
        finally
        {
            NotifyOfPropertyChange(() => HasResponse);
            this.SelectedIndex = 1;
        }
    }

    /// <summary>
    /// Apply configured changes
    /// </summary>
    /// <returns></returns>
    public async Task ApplyChanges()
    {
        try
        {
            foreach (var screen in this.RequestScreens)
            {
                if (screen is IConfigurationScreen configurationScreen)
                {
                    configurationScreen.ApplyChanges(this.Request);
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex.Message, "An error occured while trying to apply changes");
            await this.notificationManager.ShowErrorNotificationAsync(ex);
        }
    }

  
    /// </inheritdoc>       
    public override Task TryCloseAsync(bool? dialogResult = null)
    {
        return base.TryCloseAsync(dialogResult);
    }        
}
