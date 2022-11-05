using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Persistence.Services.Client.Interfaces;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace Pixel.Persistence.Services.Client;

public class PrefabsRepositoryClient : IPrefabsRepositoryClient
{

    private readonly ILogger logger = Log.ForContext<PrefabsRepositoryClient>();
    private readonly IRestClientFactory clientFactory;
    private readonly ISerializer serializer;
    protected readonly string baseUrl = "prefabs";

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="clientFactory"></param>    
    /// <param name="serializer"></param>      
    public PrefabsRepositoryClient(IRestClientFactory clientFactory, ISerializer serializer)
    {
        this.clientFactory = Guard.Argument(clientFactory).NotNull().Value;
        this.serializer = Guard.Argument(serializer).NotNull().Value;
    }

    /// <inheritdoc/>  
    public async Task<PrefabProject> GetByIdAsync(string prefabId)
    {
        Guard.Argument(prefabId, nameof(prefabId)).NotNull().NotEmpty();
        RestRequest restRequest = new RestRequest($"{baseUrl}/id/{prefabId}");
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.ExecuteGetAsync(restRequest);
        result.EnsureSuccess();
        using (var stream = new MemoryStream(result.RawBytes))
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return serializer.DeserializeContent<PrefabProject>(reader.ReadToEnd());
            }
        }
    }

    /// <inheritdoc/>  
    public async Task<PrefabProject> GetByNameAsync(string name)
    {
        Guard.Argument(name, nameof(name)).NotNull().NotEmpty();
        RestRequest restRequest = new RestRequest($"{baseUrl}/name/{name}");
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.ExecuteGetAsync(restRequest);
        result.EnsureSuccess();
        using (var stream = new MemoryStream(result.RawBytes))
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return serializer.DeserializeContent<PrefabProject>(reader.ReadToEnd());
            }
        }
    }

    /// <inheritdoc/>  
    public async Task<IEnumerable<PrefabProject>> GetAllAsync()
    {
        RestRequest restRequest = new RestRequest(baseUrl);
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.ExecuteGetAsync(restRequest);
        result.EnsureSuccess();
        using (var stream = new MemoryStream(result.RawBytes))
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return serializer.DeserializeContent<List<PrefabProject>>(reader.ReadToEnd());
            }
        }
    }

    /// <inheritdoc/>  
    public async Task<PrefabProject> AddPrefabAsync(PrefabProject prefabProject)
    {
        Guard.Argument(prefabProject, nameof(prefabProject)).NotNull();
        RestRequest restRequest = new RestRequest(baseUrl);
        restRequest.AddJsonBody(prefabProject);
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.PostAsync<PrefabProject>(restRequest);
        return result;
    }

    /// <inheritdoc/>  
    public async Task<PrefabVersion> AddPrefabVersionAsync(string projectId, PrefabVersion newVersion, PrefabVersion cloneFrom)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(newVersion, nameof(newVersion)).NotNull();
        Guard.Argument(cloneFrom, nameof(cloneFrom)).NotNull();

        if (newVersion.Equals(cloneFrom))
        {
            throw new InvalidOperationException($"New version : {newVersion} can't be same as version to be cloned : {cloneFrom}");
        }

        RestRequest restRequest = new RestRequest($"{baseUrl}/{projectId}/versions");
        restRequest.AddJsonBody(new Core.Request.AddPrefabVersionRequest(new Core.Models.PrefabVersion()
        {
            Version = newVersion.Version,
            IsActive = newVersion.IsActive,
            DataModelAssembly = newVersion.DataModelAssembly
        },
        new Core.Models.PrefabVersion()
        {
            Version = cloneFrom.Version,
            IsActive = cloneFrom.IsActive,
            DataModelAssembly = cloneFrom.DataModelAssembly
        }));
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.PostAsync<PrefabVersion>(restRequest);
        return result;
    }

    /// <inheritdoc/>  
    public async Task<PrefabVersion> UpdatePrefabVersionAsync(string prefabId, PrefabVersion prefabVersion)
    {
        Guard.Argument(prefabId, nameof(prefabId)).NotNull().NotEmpty();
        Guard.Argument(prefabVersion, nameof(prefabVersion)).NotNull();
        RestRequest restRequest = new RestRequest($"{baseUrl}/{prefabId}/versions");
        restRequest.AddJsonBody(prefabVersion);
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.PutAsync<PrefabVersion>(restRequest);
        return result;
    }
}
