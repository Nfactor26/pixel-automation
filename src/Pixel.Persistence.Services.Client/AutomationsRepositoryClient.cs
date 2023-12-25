using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Persistence.Core.Request;
using Pixel.Persistence.Services.Client.Interfaces;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public class AutomationsRepositoryClient : IAutomationsRepositoryClient
    {
        private readonly ILogger logger = Log.ForContext<AutomationsRepositoryClient>();
        private readonly IRestClientFactory clientFactory;
        private readonly ISerializer serializer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clientFactory"></param>    
        /// <param name="serializer"></param>      
        public AutomationsRepositoryClient(IRestClientFactory clientFactory, ISerializer serializer)
        {
            this.clientFactory = Guard.Argument(clientFactory).NotNull().Value;
            this.serializer = Guard.Argument(serializer).NotNull().Value;
        }

        /// <inheritdoc/>  
        public async Task<AutomationProject> GetByIdAsync(string projectId)
        {
            Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();           
            RestRequest restRequest = new RestRequest($"projects/id/{projectId}");
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteGetAsync(restRequest);
            result.EnsureSuccess();
            using (var stream = new MemoryStream(result.RawBytes))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return serializer.DeserializeContent<AutomationProject>(reader.ReadToEnd());
                }
            }
        }

        /// <inheritdoc/>  
        public async Task<AutomationProject> GetByNameAsync(string name)
        {
            Guard.Argument(name, nameof(name)).NotNull().NotEmpty();
            RestRequest restRequest = new RestRequest($"projects/name/{name}");
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteGetAsync(restRequest);
            result.EnsureSuccess();
            using (var stream = new MemoryStream(result.RawBytes))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return serializer.DeserializeContent<AutomationProject>(reader.ReadToEnd());
                }
            }
        }

        /// <inheritdoc/>  
        public async Task<IEnumerable<AutomationProject>> GetAllAsync()
        {          
            RestRequest restRequest = new RestRequest("projects");
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteGetAsync(restRequest);
            result.EnsureSuccess();
            using (var stream = new MemoryStream(result.RawBytes))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return serializer.DeserializeContent<List<AutomationProject>>(reader.ReadToEnd());
                }
            }
        }

        /// <inheritdoc/>  
        public async Task<AutomationProject> AddProjectAsync(AutomationProject automationProject)
        {
            Guard.Argument(automationProject, nameof(automationProject)).NotNull();
            RestRequest restRequest = new RestRequest("projects");
            restRequest.AddJsonBody(automationProject);
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.PostAsync<AutomationProject>(restRequest);
            return result;
        }

        /// <inheritdoc/>  
        public async Task<VersionInfo> AddProjectVersionAsync(string projectId, VersionInfo newVersion, VersionInfo cloneFrom)
        {
            Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
            Guard.Argument(newVersion, nameof(newVersion)).NotNull();
            Guard.Argument(cloneFrom, nameof(cloneFrom)).NotNull();
            
            if(newVersion.Equals(cloneFrom))
            {
                throw new InvalidOperationException($"New version : {newVersion} can't be same as version to be cloned : {cloneFrom}");
            }

            RestRequest restRequest = new RestRequest($"projects/{projectId}/versions");
            restRequest.AddJsonBody(new AddProjectVersionRequest(new Pixel.Persistence.Core.Models.ProjectVersion()
            {
                Version = newVersion.Version,
                IsActive = newVersion.IsActive,
                DataModelAssembly = newVersion.DataModelAssembly
            },
            new Pixel.Persistence.Core.Models.ProjectVersion()
            {
                Version = cloneFrom.Version,
                IsActive = cloneFrom.IsActive,
                DataModelAssembly = cloneFrom.DataModelAssembly
            })) ;            
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.PostAsync<VersionInfo>(restRequest);
            return result;
        }

        public async Task<VersionInfo> UpdateProjectVersionAsync(string projectId, VersionInfo projectVersion)
        {
            Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
            Guard.Argument(projectVersion, nameof(projectVersion)).NotNull();
            RestRequest restRequest = new RestRequest($"projects/{projectId}/versions");
            restRequest.AddJsonBody(projectVersion);          
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.PutAsync<VersionInfo>(restRequest);
            return result;
        }
    }
}
