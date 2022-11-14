using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using Pixel.Persistence.Services.Client.Interfaces;
using RestSharp;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client;

public class FixturesRepositoryClient : IFixturesRepositoryClient
{
    private readonly ILogger logger = Log.ForContext<FixturesRepositoryClient>();
    private readonly IRestClientFactory clientFactory;
    private readonly ISerializer serializer;

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="applicationSettings"></param>
    public FixturesRepositoryClient(IRestClientFactory clientFactory, ISerializer serializer)
    {
        this.clientFactory = Guard.Argument(clientFactory).NotNull().Value;
        this.serializer = Guard.Argument(serializer).NotNull().Value;
    }

    /// <inheritdoc/>  
    public async Task<TestFixture> GetByIdAsync(string projectId, string projectVersion, string fixtureId)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
        Guard.Argument(fixtureId, nameof(fixtureId)).NotNull().NotEmpty();

        RestRequest restRequest = new RestRequest($"fixtures/{projectId}/{projectVersion}/id/{fixtureId}");
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.ExecuteGetAsync(restRequest);
        result.EnsureSuccess();
        using (var stream = new MemoryStream(result.RawBytes))
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return serializer.DeserializeContent<TestFixture>(reader.ReadToEnd());
            }
        }
    }

    /// <inheritdoc/>  
    public async Task<TestFixture> GetByNameAsync(string projectId, string projectVersion, string displayName)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
        Guard.Argument(displayName, nameof(displayName)).NotNull().NotEmpty();

        RestRequest restRequest = new RestRequest($"fixtures/{projectId}/{projectVersion}/name/{displayName}");
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.ExecuteGetAsync(restRequest);
        result.EnsureSuccess();
        using (var stream = new MemoryStream(result.RawBytes))
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return serializer.DeserializeContent<TestFixture>(reader.ReadToEnd());
            }
        }
    }

    /// <inheritdoc/>  
    public async Task<IEnumerable<TestFixture>> GetAllForProjectAsync(string projectId, string projectVersion)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
    
        RestRequest restRequest = new RestRequest($"fixtures/{projectId}/{projectVersion}");
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.ExecuteGetAsync(restRequest);
        result.EnsureSuccess();
        using (var stream = new MemoryStream(result.RawBytes))
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return serializer.DeserializeContent<List<TestFixture>>(reader.ReadToEnd());
            }
        }
    }

    /// <inheritdoc/>  
    public async Task<TestFixture> AddFixtureAsync(string projectId, string projectVersion, TestFixture testFixture)
    {
        Guard.Argument(testFixture).NotNull();          
        RestRequest restRequest = new RestRequest($"fixtures/{projectId}/{projectVersion}");
        restRequest.AddJsonBody(testFixture);
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.PostAsync<TestFixture>(restRequest);
        return result;
    }

    /// <inheritdoc/>  
    public async Task<TestFixture> UpdateFixtureAsync(string projectId, string projectVersion, TestFixture testFixture)
    {
        Guard.Argument(testFixture).NotNull();
        RestRequest restRequest = new RestRequest($"fixtures/{projectId}/{projectVersion}");
        restRequest.AddJsonBody(testFixture);
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.PutAsync<TestFixture>(restRequest);
        return result;
    }

    /// <inheritdoc/>  
    public async Task DeleteFixtureAsync(string projectId, string projectVersion, string fixtureId)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
        Guard.Argument(fixtureId, nameof(fixtureId)).NotNull().NotEmpty();
                 
        RestRequest restRequest = new RestRequest($"fixtures/{projectId}/{projectVersion}/{fixtureId}") { Method = Method.Delete };          
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.DeleteAsync(restRequest);
        result.EnsureSuccess();
    }

  
}
