using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.RestApi.Shared;

namespace Pixel.Automation.HttpRequest.Editor;

/// <summary>
/// View model for configuring the <see cref="QueryParameter"/>
/// </summary>
public class QueryStringsConfigurationViewModel : Screen, IConfigurationScreen
{
    /// <summary>
    /// Actor component for exeucting the rest request
    /// </summary>
    public Component OwnerComponent { get; set; }

    /// <summary>
    /// Collection of QueryParameters visible on screen
    /// </summary>
    public BindableCollection<QueryParameter> Parameters { get; set; } = new();
   
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="ownerComponent"></param>
    /// <param name="parameters"></param>
    public QueryStringsConfigurationViewModel(Component ownerComponent, List<QueryParameter> parameters)
    {
        this.DisplayName = "Query Strings";
        this.OwnerComponent = Guard.Argument(ownerComponent, nameof(ownerComponent));
        Guard.Argument(parameters, nameof(parameters)).NotNull();
        if (parameters.Any())
        {
            this.Parameters.AddRange(parameters);
        }
        else
        {
            this.AddQueryString();
        }
    }

    /// <summary>
    /// Add a new QueryParameter row
    /// </summary>
    public void AddQueryString()
    {
        this.Parameters.Add(new QueryParameter());
    }

    /// <summary>
    /// Delete an existing QueryParameter row
    /// </summary>
    /// <param name="requestParameter"></param>
    public void DeleteQueryString(QueryParameter requestParameter)
    {
        this.Parameters.Remove(requestParameter);
    }

    /// </inheritdoc>
    public void ApplyChanges(RestApi.Shared.HttpRequest request)
    {
        request.RequestParameters.Clear();
        if (this.Parameters.Any())
        {
            request.RequestParameters.AddRange(this.Parameters);
        }
    }
}
