using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Persistence.Core.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public class ApplicationRepositoryClient : IApplicationRepositoryClient
    {
        private readonly string baseUrl = "http://localhost:49574/api/application";
        private readonly IRestClient client;
        private readonly ISerializer serializer;

        public ApplicationRepositoryClient(ISerializer serializer)
        {
            this.serializer = serializer;
            this.client = new RestClient(baseUrl);           
        }


        public async Task<ApplicationDescription> GetApplication(string applicationId)
        {
            RestRequest restRequest = new RestRequest($"{applicationId}");
            var response = await client.ExecuteGetAsync(restRequest);
            if (response.StatusCode.Equals(HttpStatusCode.OK))
            {
                using (var stream = new MemoryStream(response.RawBytes))
                {
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        return serializer.DeserializeContent<ApplicationDescription>(reader.ReadToEnd());
                    }
                }
            }
            throw new Exception($"{response.StatusCode}, {response.ErrorMessage ?? "Failed to download application with id :" + applicationId}");
        }
               
        public async Task<IEnumerable<ApplicationDescription>> GetApplications(IEnumerable<ApplicationMetaData> applicationsToDownload)
        {
            List<ApplicationDescription> applicationDescriptions = new List<ApplicationDescription>();         
            foreach (var application in applicationsToDownload)
            {
                var applicationDescription = await GetApplication(application.ApplicationId);
                applicationDescriptions.Add(applicationDescription);
            }
            return applicationDescriptions;
        }
        

        public async Task<ApplicationDescription> AddOrUpdateApplication(ApplicationDescription applicationDescription, string applicationFile)
        {
            RestRequest restRequest = new RestRequest() { Method = Method.POST };
            var applicationMetaData = new ApplicationMetaData() { ApplicationId = applicationDescription.ApplicationId, ApplicationName = applicationDescription.ApplicationName, ApplicationType = applicationDescription.ApplicationType };
            restRequest.AddParameter(nameof(ApplicationMetaData), serializer.Serialize<ApplicationMetaData>(applicationMetaData), ParameterType.RequestBody);      
            restRequest.AddFile("file", applicationFile);
            var result = await client.PostAsync<ApplicationDescription>(restRequest);
            return result;
        }      
       

        public async Task<ApplicationDescription> UpdateApplication(ApplicationDescription applicationDescription)
        {
            RestRequest restRequest = new RestRequest();
            restRequest.AddJsonBody(applicationDescription);
            var result = await client.PutAsync<ApplicationDescription>(restRequest);
            return result;
        }
    }
}
