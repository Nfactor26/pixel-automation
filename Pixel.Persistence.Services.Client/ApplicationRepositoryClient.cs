using Dawn;
using Pixel.Automation.Core;
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
        private readonly string baseUrl;       
        private readonly ISerializer serializer;

        public ApplicationRepositoryClient(ISerializer serializer, ApplicationSettings applicationSettings)
        {
            Guard.Argument(serializer, nameof(serializer)).NotNull();
            Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
            this.serializer = serializer;        
            this.baseUrl = applicationSettings.PersistenceServiceUri;
        }


        public async Task<ApplicationDescription> GetApplication(string applicationId)
        {
            RestRequest restRequest = new RestRequest($"{applicationId}");
            var client = new RestClient(baseUrl);
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
            var client = new RestClient(baseUrl);
            var result = await client.PostAsync<ApplicationDescription>(restRequest);
            return result;
        }      
       

        public async Task<ApplicationDescription> UpdateApplication(ApplicationDescription applicationDescription)
        {
            RestRequest restRequest = new RestRequest();
            restRequest.AddJsonBody(applicationDescription);
            var client = new RestClient(baseUrl);
            var result = await client.PutAsync<ApplicationDescription>(restRequest);
            return result;
        }
    }
}
