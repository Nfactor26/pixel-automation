using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using Pixel.Persistence.Services.Client.Interfaces;
using RestSharp;
using Serilog;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client;

public class ProjectFilesRepositoryClient : FilesRepositoryClient, IProjectFilesRepositoryClient
{
    public ProjectFilesRepositoryClient(IRestClientFactory clientFactory, ISerializer serializer) 
        : base(clientFactory, serializer)
    {
    }

    protected override string GetBaseUrl()
    {
        return "projectfiles";
    }
}

public class PrefabFilesRepositoryClient : FilesRepositoryClient, IPrefabFilesRepositoryClient
{
    public PrefabFilesRepositoryClient(IRestClientFactory clientFactory, ISerializer serializer)
        : base(clientFactory, serializer)
    {
    }

    protected override string GetBaseUrl()
    {
        return "prefabfiles";
    }
}

public abstract class FilesRepositoryClient : IFilesRepositoryClient
{
    protected readonly ILogger logger = Log.ForContext<PrefabsRepositoryClient>();
    protected readonly IRestClientFactory clientFactory;
    protected readonly ISerializer serializer;
    protected readonly string baseUrl;


    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="clientFactory"></param>
    /// <param name="serializer"></param>
    public FilesRepositoryClient(IRestClientFactory clientFactory, ISerializer serializer)
    {
        this.clientFactory = Guard.Argument(clientFactory).NotNull().Value;
        this.serializer = Guard.Argument(serializer).NotNull().Value;
        this.baseUrl = GetBaseUrl();
    }

    /// <summary>
    /// Get the base url for the api endpoint
    /// </summary>
    /// <returns></returns>
    protected abstract string GetBaseUrl();

    /// <inheritdoc/>  
    public async Task<byte[]> DownloadProjectDataFilesWithTags(string projectId, string projectVersion, string[] tags)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
        Guard.Argument(tags, nameof(tags)).NotNull().NotEmpty();

        RestRequest restRequest = new RestRequest($"{baseUrl}/{projectId}/{projectVersion}/tags");
        foreach (var tag in tags)
        {
            restRequest.AddQueryParameter("tag", tag);
        }
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.ExecuteGetAsync(restRequest);
        result.EnsureSuccess();
        return result.RawBytes;
    }

    /// <inheritdoc/>  
    public async Task<byte[]> DownProjectDataFile(string projectId, string projectVersion, string fileName)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
        Guard.Argument(fileName, nameof(fileName)).NotNull().NotEmpty();

        RestRequest restRequest = new RestRequest($"{baseUrl}/{projectId}/{projectVersion}/name/{fileName}");
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.ExecuteGetAsync(restRequest);
        result.EnsureSuccess();
        return result.RawBytes;
    }

    /// <inheritdoc/>  
    public async Task AddProjectDataFile(ProjectDataFile file, string filePath)
    {
        Guard.Argument(file, nameof(file)).NotNull();
        Guard.Argument(filePath).NotNull().NotEmpty();

        var projectFileRequest = new AddProjectFileRequest()
        {
            ProjectId = file.ProjectId,
            ProjectVersion = file.ProjectVersion,
            Tag = file.Tag,
            FileName = file.FileName,
            FilePath = file.FilePath
        };
        RestRequest restRequest = new RestRequest(baseUrl) { Method = Method.Post };      
        restRequest.AddParameter(nameof(AddProjectFileRequest), serializer.Serialize<AddProjectFileRequest>(projectFileRequest), ParameterType.RequestBody);
        restRequest.AddFile("file", filePath);
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.ExecuteAsync(restRequest);
        result.EnsureSuccess();
    }
    
    /// <inheritdoc/>  
    public async Task DeleteProjectDataFile(string projectId, string projectVersion, string fileName)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
        Guard.Argument(fileName, nameof(fileName)).NotNull().NotEmpty();

        RestRequest restRequest = new RestRequest($"{baseUrl}/{projectId}/{projectVersion}/{fileName}");
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.PostAsync(restRequest);
        result.EnsureSuccess();       
    }
}
