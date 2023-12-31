using Dawn;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Persistence.Services.Client.Interfaces;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public class ApplicationRepositoryClient : IApplicationRepositoryClient
    {
        private readonly ILogger logger = Log.ForContext<ApplicationRepositoryClient>();
        private readonly IRestClientFactory clientFactory;
        private readonly ISerializer serializer;
        
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="clientFactory"></param>
        /// <param name="serializer"></param>
        /// <param name="applicationSettings"></param>
        public ApplicationRepositoryClient(IRestClientFactory clientFactory, ISerializer serializer)
        {
            this.clientFactory = Guard.Argument(clientFactory).NotNull().Value;
            this.serializer = Guard.Argument(serializer).NotNull().Value;          
        }

        ///<inheritdoc/>
        public async Task<ApplicationDescription> GetApplication(string applicationId)
        {
            Guard.Argument(applicationId).NotNull().NotEmpty();
            logger.Debug("Get ApplicationDescription for applicationId : {0}", applicationId);

            RestRequest restRequest = new RestRequest($"application/id/{applicationId}");
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteGetAsync(restRequest);
            result.EnsureSuccess();
            using (var stream = new MemoryStream(result.RawBytes))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return serializer.DeserializeContent<ApplicationDescription>(reader.ReadToEnd());
                }
            }
        }

        ///<inheritdoc/>
        public async Task<IEnumerable<ApplicationDescription>> GetApplications(DateTime laterThan)
        {
            RestRequest restRequest = new RestRequest("application");
            restRequest.AddHeader("platform", GetCurrentOperatingSystem());
            restRequest.AddHeader("laterThan", laterThan.ToString("O"));
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteGetAsync(restRequest);
            result.EnsureSuccess();
            using (var stream = new MemoryStream(result.RawBytes))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return serializer.DeserializeContent<List<ApplicationDescription>>(reader.ReadToEnd());
                }
            }

            string GetCurrentOperatingSystem()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return OSPlatform.Windows.ToString();
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return OSPlatform.Linux.ToString();
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return OSPlatform.OSX.ToString();
                else
                    throw new NotSupportedException("OS Platform is not supported");
            }
        }

        ///<inheritdoc/>
        public async Task AddApplication(ApplicationDescription applicationDescription)
        {
            Guard.Argument(applicationDescription, nameof(applicationDescription)).NotNull();
            logger.Debug("Add {@ApplicationDescription}", applicationDescription);

            RestRequest restRequest = new RestRequest("/application") { Method = Method.Post };
            restRequest.AddJsonBody(serializer.Serialize<ApplicationDescription>(applicationDescription));
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteAsync(restRequest, Method.Post);
            result.EnsureSuccess();
        }

        ///<inheritdoc/>
        public async Task UpdateApplication(ApplicationDescription applicationDescription)
        {
            Guard.Argument(applicationDescription, nameof(applicationDescription)).NotNull();
            logger.Debug("Update {@ApplicationDescription}", applicationDescription);

            RestRequest restRequest = new RestRequest("/application") { Method = Method.Put };
            restRequest.AddJsonBody(serializer.Serialize<ApplicationDescription>(applicationDescription));
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteAsync(restRequest, Method.Put);
            result.EnsureSuccess();
        }

        ///<inheritdoc/>
        public async Task DeleteApplicationAsync(ApplicationDescription applicationDescription)
        {
            Guard.Argument(applicationDescription, nameof(applicationDescription)).NotNull();
            RestRequest restRequest = new RestRequest($"/application/{applicationDescription.ApplicationId}");           
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteAsync(restRequest, Method.Delete);
            result.EnsureSuccess();
        }

        ///<inheritdoc/>
        public async Task AddApplicationScreen(string applicationId, ApplicationScreen applicationScreen)
        {
            Guard.Argument(applicationId, nameof(applicationId)).NotNull().NotEmpty().NotWhiteSpace();
            Guard.Argument(applicationScreen, nameof(applicationScreen)).NotNull();
            RestRequest restRequest = new RestRequest($"/application/{applicationId}/screens");
            restRequest.AddJsonBody(serializer.Serialize<ApplicationScreen>(applicationScreen));
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteAsync(restRequest, Method.Post);
            result.EnsureSuccess();
        }

        ///<inheritdoc/>
        public async Task RenameApplicationScreen(string applicationId, string screenId, string newName)
        {
            Guard.Argument(applicationId, nameof(applicationId)).NotNull().NotEmpty().NotWhiteSpace();
            Guard.Argument(screenId, nameof(screenId)).NotNull().NotEmpty().NotWhiteSpace();
            Guard.Argument(newName, nameof(newName)).NotNull().NotEmpty().NotWhiteSpace();
            RestRequest restRequest = new RestRequest($"/application/{applicationId}/screens/rename/{screenId}/to/{newName}");       
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteAsync(restRequest, Method.Put);
            result.EnsureSuccess();
        }
      
        ///<inheritdoc/>
        public async Task MoveControlToScreen(ControlDescription controlDescription, string targetScreenId)
        {
            Guard.Argument(targetScreenId, nameof(targetScreenId)).NotNull().NotEmpty().NotWhiteSpace();
            Guard.Argument(controlDescription, nameof(controlDescription)).NotNull();
            RestRequest restRequest = new RestRequest($"/application/{controlDescription.ApplicationId}/screens/control/{controlDescription.ControlId}/move/to/{targetScreenId}");
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteAsync(restRequest, Method.Post);
            result.EnsureSuccess();
        }

        ///<inheritdoc/>
        public async Task MovePrefabToScreen(PrefabProject prefabProject, string targetScreenId)
        {
            Guard.Argument(targetScreenId, nameof(targetScreenId)).NotNull().NotEmpty().NotWhiteSpace();
            Guard.Argument(prefabProject, nameof(prefabProject)).NotNull();
            RestRequest restRequest = new RestRequest($"/application/{prefabProject.ApplicationId}/screens/prefab/{prefabProject.ProjectId}/move/to/{targetScreenId}");
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteAsync(restRequest, Method.Post);
            result.EnsureSuccess();
        }
    }
}
