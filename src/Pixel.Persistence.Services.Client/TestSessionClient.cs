using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Persistence.Services.Client.Models;
using RestSharp;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public class TestSessionClient : ITestSessionClient
    {
        private readonly ILogger logger = Log.ForContext<TestSessionClient>();
        private readonly ISerializer serializer;
        private readonly IRestClientFactory clientFactory;
        
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="clientFactory"></param>
        public TestSessionClient(IRestClientFactory clientFactory, ISerializer serializer)
        {
            this.clientFactory = Guard.Argument(clientFactory).NotNull().Value;
            this.serializer = Guard.Argument(serializer).NotNull().Value;
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
            return result.Id;
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
        public async Task<TestResult> AddResultAsync(TestResult testResult)
        {
            Guard.Argument(testResult).NotNull();
            logger.Debug("Add test result {@TestResult}", testResult);

            RestRequest restRequest = new RestRequest("testresult") { Method = Method.Post };
            restRequest.AddJsonBody(testResult);
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.PostAsync<TestResult>(restRequest);
            return result;
        }

        public async Task AddTraceImagesAsync(TestResult testResult, IEnumerable<string> imageFiles)
        {
            Guard.Argument(testResult, nameof(testResult)).NotNull();
            Guard.Argument(imageFiles, nameof(imageFiles)).NotNull();         
            RestRequest restRequest = new RestRequest($"testresult/trace/images");
            var traceImageMetaData = new TraceImageMetaData(testResult.ProjectId, testResult.Id);          
            restRequest.AddParameter(nameof(TraceImageMetaData), serializer.Serialize<TraceImageMetaData>(traceImageMetaData), ParameterType.RequestBody);
            foreach(var imageFile in imageFiles)
            {
                restRequest.AddFile("files", imageFile);
            }
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteAsync(restRequest, Method.Post);
            result.EnsureSuccess();
        }
    }
}
