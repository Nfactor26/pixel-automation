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

public class TestDataRepositoryClient : ITestDataRepositoryClient
{

    private readonly ILogger logger = Log.ForContext<TestDataRepositoryClient>();
    private readonly IRestClientFactory clientFactory;
    private readonly ISerializer serializer;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="applicationSettings"></param>
    public TestDataRepositoryClient(IRestClientFactory clientFactory, ISerializer serializer)
    {
        this.clientFactory = Guard.Argument(clientFactory).NotNull().Value;
        this.serializer = Guard.Argument(serializer).NotNull().Value;
    }

    /// <inheritdoc/>
    public async Task<TestDataSource> GetByIdAsync(string projectId, string projectVersion, string fixtureId)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
        Guard.Argument(fixtureId, nameof(fixtureId)).NotNull().NotEmpty();

        RestRequest restRequest = new RestRequest($"testdata/{projectId}/{projectVersion}/id/{fixtureId}");
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.ExecuteGetAsync(restRequest);
        result.EnsureSuccess();
        using (var stream = new MemoryStream(result.RawBytes))
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return serializer.DeserializeContent<TestDataSource>(reader.ReadToEnd());
            }
        }
    }

    /// <inheritdoc/>
    public async Task<TestDataSource> GetByNameAsync(string projectId, string projectVersion, string name)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
        Guard.Argument(name, nameof(name)).NotNull().NotEmpty();

        RestRequest restRequest = new RestRequest($"testdata/{projectId}/{projectVersion}/name/{name}");
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.ExecuteGetAsync(restRequest);
        result.EnsureSuccess();
        using (var stream = new MemoryStream(result.RawBytes))
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return serializer.DeserializeContent<TestDataSource>(reader.ReadToEnd());
            }
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TestDataSource>> GetAllForProjectAsync(string projectId, string projectVersion)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();

        RestRequest restRequest = new RestRequest($"testdata/{projectId}/{projectVersion}");
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.ExecuteGetAsync(restRequest);
        result.EnsureSuccess();
        using (var stream = new MemoryStream(result.RawBytes))
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return serializer.DeserializeContent<List<TestDataSource>>(reader.ReadToEnd());
            }
        }
    }

    /// <inheritdoc/>
    public async Task<TestDataSource> AddDataSourceAsync(string projectId, string projectVersion, TestDataSource testData)
    {
        Guard.Argument(testData).NotNull();
        RestRequest restRequest = new RestRequest($"testdata/{projectId}/{projectVersion}");
        restRequest.AddJsonBody(testData);
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.PostAsync<TestDataSource>(restRequest);
        return result;
    }

    /// <inheritdoc/>
    public async Task DeleteDataSourceAsync(string projectId, string projectVersion, string dataSourceId)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
        Guard.Argument(dataSourceId, nameof(dataSourceId)).NotNull().NotEmpty();

        RestRequest restRequest = new RestRequest($"testdata/{projectId}/{projectVersion}/{dataSourceId}") { Method = Method.Delete };
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.DeleteAsync(restRequest);
        result.EnsureSuccess();
    }

    /// <inheritdoc/>
    public async Task<TestDataSource> UpdateDataSourceAsync(string projectId, string projectVersion, TestDataSource dataSource)
    {
        Guard.Argument(dataSource).NotNull();
        RestRequest restRequest = new RestRequest($"testdata/{projectId}/{projectVersion}");
        restRequest.AddJsonBody(dataSource);
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.PutAsync<TestDataSource>(restRequest);
        return result;
    }

}
