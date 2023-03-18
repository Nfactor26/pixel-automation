using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.RestApi.Shared;
using RestHttpRequest = Pixel.Automation.RestApi.Shared.HttpRequest;

namespace Pixel.Automation.HttpRequest.Editor;

/// <summary>
/// Configures http request for form data in body
/// </summary>
public class FormDataBodyConfigurationViewModel : Screen, IConfigurationScreen
{
    private readonly RestHttpRequest httpRequest;

    /// <summary>
    /// Actor component for executing the rest request
    /// </summary>
    public Component OwnerComponent { get; private set; }
   
    /// <summary>
    /// FormFields to be added in the request body
    /// </summary>
    public BindableCollection<FormField> FormFields { get; set; } = new();

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="ownerComponent"></param>
    /// <param name="httpRequest"></param>
    public FormDataBodyConfigurationViewModel(Component ownerComponent, RestHttpRequest httpRequest)
    {
        this.DisplayName = "Form Data";
        this.OwnerComponent = Guard.Argument(ownerComponent, nameof(ownerComponent)).NotNull();
        this.httpRequest = Guard.Argument(httpRequest, nameof(httpRequest)).NotNull();        
    }

    /// <summary>
    /// Add a new FormField row
    /// </summary>
    public void AddFormField()
    {
        this.FormFields.Add(new FormField());
    }

    /// <summary>
    /// Remove an existing form field row
    /// </summary>
    /// <param name="formField"></param>
    public void DeleteFormField(FormField formField)
    {
        this.FormFields.Remove(formField);
    }

    /// </inheritdoc>
    public void ApplyChanges(RestHttpRequest request)
    {
        if(request.RequestBody is FormDataBodyContent fbc)
        {
            fbc.FormFields.Clear();
            if(this.FormFields.Any())
            {
                fbc.FormFields.AddRange(this.FormFields);
            }
        }       
    }

    /// </inheritdoc>
    protected override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        if (this.httpRequest.RequestBody == null || this.httpRequest.RequestBody.GetType() != typeof(FormDataBodyContent))
        {
            this.httpRequest.RequestBody = new FormDataBodyContent();
        }
        if(this.httpRequest.RequestBody is FormDataBodyContent formDataBodyContent)
        {
            if (formDataBodyContent.FormFields.Any())
            {
                this.FormFields.AddRange(formDataBodyContent.FormFields);
            }
            else
            {
                this.AddFormField();
            }
        }     
        return base.OnActivateAsync(cancellationToken);
    }
}
