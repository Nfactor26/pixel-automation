using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Persistence.Services.Client.Interfaces;
using RestSharp;
using Serilog;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client;

public class ReferencesRepositoryClient : IReferencesRepositoryClient
{
    private readonly ILogger logger = Log.ForContext<ReferencesRepositoryClient>();
    private readonly IRestClientFactory clientFactory;
    private readonly ISerializer serializer;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="applicationSettings"></param>
    public ReferencesRepositoryClient(IRestClientFactory clientFactory, ISerializer serializer)
    {
        this.clientFactory = Guard.Argument(clientFactory).NotNull().Value;
        this.serializer = Guard.Argument(serializer).NotNull().Value;
    }

    /// <inheritdoc/>  
    public async Task<ProjectReferences> GetProjectReferencesAsync(string projectId, string projectVersion)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();

        RestRequest restRequest = new RestRequest($"references/{projectId}/{projectVersion}");
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.ExecuteGetAsync(restRequest);
        result.EnsureSuccess();
        using (var stream = new MemoryStream(result.RawBytes))
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return serializer.DeserializeContent<ProjectReferences>(reader.ReadToEnd());
            }
        }
    }

    /// <inheritdoc/>
    public async Task<ProjectReferences> AddProjectReferencesAsync(string projectId, string projectVersion, ProjectReferences projectReferences)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
        Guard.Argument(projectReferences, nameof(projectReferences)).NotNull();

        RestRequest restRequest = new RestRequest($"references/{projectId}/{projectVersion}");
        restRequest.AddJsonBody(projectReferences);
        var client = this.clientFactory.GetOrCreateClient();
        return await client.PostAsync<ProjectReferences>(restRequest);
    }

    /// <inheritdoc/>
    public async Task AddOrUpdateControlReferences(string projectId, string projectVersion, ControlReference controlReference)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
        Guard.Argument(controlReference, nameof(controlReference)).NotNull();

        RestRequest restRequest = new RestRequest($"references/controls/{projectId}/{projectVersion}");
        restRequest.AddJsonBody(controlReference);
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.ExecuteAsync<ControlReference>(restRequest, Method.Post);
        result.EnsureSuccess();
    }

    /// <inheritdoc/>
    public async Task AddOrUpdatePrefabReferences(string projectId, string projectVersion, PrefabReference prefabReference)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
        Guard.Argument(prefabReference, nameof(prefabReference)).NotNull();

        RestRequest restRequest = new RestRequest($"references/prefabs/{projectId}/{projectVersion}");
        restRequest.AddJsonBody(prefabReference);
        var client = this.clientFactory.GetOrCreateClient();
        await client.PostAsync<PrefabReference>(restRequest);
    }

    /// <inheritdoc/>
    public async Task SetEditorReferencesAsync(string projectId, string projectVersion, EditorReferences editorReferences)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
        Guard.Argument(editorReferences, nameof(editorReferences)).NotNull();

        RestRequest restRequest = new RestRequest($"references/editors/{projectId}/{projectVersion}");
        restRequest.AddJsonBody(editorReferences);
        var client = this.clientFactory.GetOrCreateClient();
        await client.PostAsync<EditorReferences>(restRequest);
    }

    /// <inheritdoc/>  
    public async Task AddTestDataSourceGroupAsync(string projectId, string projectVersion, string groupKey)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
        Guard.Argument(groupKey, nameof(groupKey)).NotNull();
        
        RestRequest restRequest = new RestRequest($"references/datasources/{projectId}/{projectVersion}/group/new/{groupKey}");     
        var client = this.clientFactory.GetOrCreateClient();    
        var result = await client.ExecuteAsync(restRequest, Method.Post);
        result.EnsureSuccess();
    }

    /// <inheritdoc/>  
    public async Task RenameTestDataSourceGroupAsync(string projectId, string projectVersion, string currentKey, string newKey)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
        Guard.Argument(currentKey, nameof(currentKey)).NotNull().NotWhiteSpace();
        Guard.Argument(newKey, nameof(newKey)).NotNull().NotWhiteSpace();

        RestRequest restRequest = new RestRequest($"references/datasources/{projectId}/{projectVersion}/group/rename/{currentKey}/to/{newKey}");
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.ExecuteAsync(restRequest, Method.Post);
        result.EnsureSuccess();
    }

    /// <inheritdoc/>  
    public async Task MoveTestDataSourceToGroupAsync(string projectId, string projectVersion, string testDataSourceId, string currentGroup, string newGroup)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
        Guard.Argument(testDataSourceId, nameof(testDataSourceId)).NotNull().NotEmpty();
        Guard.Argument(currentGroup, nameof(currentGroup)).NotNull().NotWhiteSpace();
        Guard.Argument(newGroup, nameof(newGroup)).NotNull().NotWhiteSpace();

        RestRequest restRequest = new RestRequest($"references/datasources/{projectId}/{projectVersion}/{testDataSourceId}/move/{currentGroup}/to/{newGroup}");
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.ExecuteAsync(restRequest, Method.Post);
        result.EnsureSuccess();
    }
}
