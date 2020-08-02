using Dawn;
using Pixel.Automation.Core;
using Pixel.Persistence.Core.Models;
using RestSharp;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public class TestSessionClient : ITestSessionClient
    {
        private readonly string baseUrl;    

        public TestSessionClient(ApplicationSettings applicationSettings)
        {
            Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
            this.baseUrl = applicationSettings.PersistenceServiceUri;
        }

        public async Task AddSession(TestSession testSession)
        {         
            RestRequest restRequest = new RestRequest("TestSession");           
            restRequest.AddJsonBody(testSession);
            var client = new RestClient(baseUrl);
            _ = await client.PostAsync<TestSession>(restRequest);
            await Task.CompletedTask;
        }
    }
}
