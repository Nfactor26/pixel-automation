using Pixel.Persistence.Core.Models;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public class TestSessionClient : ITestSessionClient
    {
        private readonly string baseUrl = "http://localhost:49574/api";
        private readonly IRestClient client;

        public TestSessionClient()
        {            
            this.client = new RestClient(baseUrl);
        }

        public async Task AddSession(TestSession testSession)
        {         
            RestRequest restRequest = new RestRequest("TestSession");           
            restRequest.AddJsonBody(testSession);
            _ = await client.PostAsync<TestSession>(restRequest);
            await Task.CompletedTask;
        }
    }
}
