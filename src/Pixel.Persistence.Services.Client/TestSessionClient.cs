using Dawn;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Services.Client.Interfaces;
using RestSharp;
using Serilog;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public class TestSessionClient : ITestSessionClient
    {
        private readonly ILogger logger = Log.ForContext<TestSessionClient>();
        private readonly IRestClientFactory clientFactory;
        
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="clientFactory"></param>
        public TestSessionClient(IRestClientFactory clientFactory)
        {
            this.clientFactory = Guard.Argument(clientFactory).NotNull().Value;           
        }

        ///<inheritdoc/>
        public async Task<string> AddSessionAsync(TestSession testSession)
        {
            Guard.Argument(testSession).NotNull();
            logger.Debug("Add test session {@TestSession}", testSession);
            
            RestRequest restRequest = new RestRequest("testsession");           
            restRequest.AddJsonBody(testSession);
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.PostAsync<TestSession>(restRequest);
            return result.SessionId;
        }

        ///<inheritdoc/>
        public async Task UpdateSessionAsync(string sessionId, TestSession testSession)
        {
            Guard.Argument(sessionId).NotNull().NotEmpty();
            Guard.Argument(testSession).NotNull();
            logger.Debug("Updated test session {@TestSession}", testSession);

            RestRequest restRequest = new RestRequest($"testsession/{sessionId}") { Method = Method.Post };
            restRequest.AddJsonBody(testSession);
            var client = this.clientFactory.GetOrCreateClient();
            var result =  await client.ExecuteAsync(restRequest);
            result.EnsureSuccess();
        }

        ///<inheritdoc/>
        public async Task AddResultAsync(TestResult testResult)
        {
            Guard.Argument(testResult).NotNull();
            logger.Debug("Add test result {@TestResult}", testResult);

            RestRequest restRequest = new RestRequest("testresult") { Method = Method.Post };
            restRequest.AddJsonBody(testResult);
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteAsync(restRequest);
            result.EnsureSuccess();
        }
    }
}
