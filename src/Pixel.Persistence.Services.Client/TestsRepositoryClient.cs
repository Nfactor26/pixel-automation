using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using Pixel.Persistence.Services.Client.Interfaces;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client;

public class TestsRepositoryClient : ITestsRepositoryClient
{
    private readonly ILogger logger = Log.ForContext<TestsRepositoryClient>();
    private readonly IRestClientFactory clientFactory;
    private readonly ISerializer serializer;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="applicationSettings"></param>
    public TestsRepositoryClient(IRestClientFactory clientFactory, ISerializer serializer)
    {
        this.clientFactory = Guard.Argument(clientFactory).NotNull().Value;
        this.serializer = Guard.Argument(serializer).NotNull().Value;
    }

    /// <inheritdoc/>
    public async Task<TestCase> GetByIdAsync(string projectId, string projectVersion, string testId)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
        Guard.Argument(testId, nameof(testId)).NotNull().NotEmpty();

        RestRequest restRequest = new RestRequest($"tests/{projectId}/{projectVersion}/id/{testId}");
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.ExecuteGetAsync(restRequest);
        result.EnsureSuccess();
        using (var stream = new MemoryStream(result.RawBytes))
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return serializer.DeserializeContent<TestCase>(reader.ReadToEnd());
            }
        }
    }

    /// <inheritdoc/>
    public async Task<TestCase> GetByNameAsync(string projectId, string projectVersion, string displayName)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
        Guard.Argument(displayName, nameof(displayName)).NotNull().NotEmpty();

        RestRequest restRequest = new RestRequest($"tests/{projectId}/{projectVersion}/name/{displayName}");
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.ExecuteGetAsync(restRequest);
        result.EnsureSuccess();
        using (var stream = new MemoryStream(result.RawBytes))
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return serializer.DeserializeContent<TestCase>(reader.ReadToEnd());
            }
        }
    }
  
    /// <inheritdoc/>
    public async Task<IEnumerable<TestCase>> GetAllForProjectAsync(string projectId, string projectVersion, DateTime laterThan)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
   
        RestRequest restRequest = new RestRequest($"tests/{projectId}/{projectVersion}");
        restRequest.AddHeader(nameof(laterThan), laterThan.ToString("O"));
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.ExecuteGetAsync(restRequest);
        result.EnsureSuccess();
        using (var stream = new MemoryStream(result.RawBytes))
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return serializer.DeserializeContent<List<TestCase>>(reader.ReadToEnd());
            }
        }
    }

    /// <inheritdoc/>
    public async Task<TestCase> AddTestCaseAsync(string projectId, string projectVersion, TestCase testCase)
    {
        Guard.Argument(testCase, nameof(testCase)).NotNull();
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
      
        RestRequest restRequest = new RestRequest($"tests/{projectId}/{projectVersion}");
        restRequest.AddJsonBody(testCase);
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.PostAsync<TestCase>(restRequest);
        return result;
    }

    /// <inheritdoc/>
    public async Task<TestCase> UpdateTestCaseAsync(string projectId, string projectVersion, TestCase testCase)
    {
        Guard.Argument(testCase).NotNull();
        RestRequest restRequest = new RestRequest($"tests/{projectId}/{projectVersion}");
        restRequest.AddJsonBody(testCase);
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.PutAsync<TestCase>(restRequest);
        return result;
    }

    /// <inheritdoc/>
    public async Task DeleteTestCaseAsync(string projectId, string projectVersion, string testId)
    {
        Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
        Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
        Guard.Argument(testId, nameof(testId)).NotNull().NotEmpty();

        RestRequest restRequest = new RestRequest($"tests/{projectId}/{projectVersion}/{testId}") { Method = Method.Delete };
        var client = this.clientFactory.GetOrCreateClient();
        var result = await client.DeleteAsync(restRequest);
        result.EnsureSuccess();
    } 
   
}
