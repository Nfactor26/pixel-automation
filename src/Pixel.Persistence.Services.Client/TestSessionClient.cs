using Dawn;
using Pixel.Automation.Core;
using Pixel.Persistence.Core.Models;
using RestSharp;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public class TestSessionClient : ITestSessionClient
    {
        private readonly string sessionUrl;
        private readonly string resultsUrl;

        public TestSessionClient(ApplicationSettings applicationSettings)
        {
            Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
            this.sessionUrl = $"{applicationSettings.PersistenceServiceUri}/TestSession";
            this.resultsUrl = $"{applicationSettings.PersistenceServiceUri}/TestResult";
        }

        public async Task<string> AddSessionAsync(TestSession testSession)
        {         
            RestRequest restRequest = new RestRequest();           
            restRequest.AddJsonBody(testSession);
            var client = new RestClient(sessionUrl);
            var result = await client.PostAsync<TestSession>(restRequest);
            return result.SessionId;
        }

        public async Task UpdateSessionAsync(string sessionId, TestSession testSession)
        {
            RestRequest restRequest = new RestRequest();
            restRequest.AddJsonBody(testSession);
            var client = new RestClient($"{sessionUrl}/{sessionId}");
            _ = await client.PostAsync<TestSession>(restRequest);
            await Task.CompletedTask;
        }

        public async Task AddResultAsync(TestResult testResult)
        {
            RestRequest restRequest = new RestRequest();
            restRequest.AddJsonBody(testResult);
            var client = new RestClient(resultsUrl);
            _ = await client.PostAsync<TestSession>(restRequest);
            await Task.CompletedTask;
        }
    }
}
