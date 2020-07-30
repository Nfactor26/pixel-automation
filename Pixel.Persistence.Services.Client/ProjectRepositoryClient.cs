using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Persistence.Core.Models;
using RestSharp;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public class ProjectRepositoryClient : IProjectRepositoryClient
    {
        private readonly string baseUrl = "http://localhost:49574/api/project";
        private readonly IRestClient client;
        private readonly ISerializer serializer;

        public ProjectRepositoryClient(ISerializer serializer)
        {
            this.serializer = serializer;
            this.client = new RestClient(baseUrl);
        }

        public async Task<AutomationProject> GetProjectFile(string projectId)
        {
            RestRequest restRequest = new RestRequest($"{projectId}");
            var response = await client.ExecuteGetAsync(restRequest);
            if (response.StatusCode.Equals(HttpStatusCode.OK))
            {
                using (var stream = new MemoryStream(response.RawBytes))
                {
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        return serializer.DeserializeContent<AutomationProject>(reader.ReadToEnd());
                    }
                }
            }
            throw new Exception($"{response.StatusCode}, {response.ErrorMessage ?? string.Empty}");
        }

        public async Task<byte[]> GetProjectDataFiles(string projectId, string version)
        {
            RestRequest restRequest = new RestRequest($"{projectId}/{version}");
            var response = await client.ExecuteGetAsync(restRequest);
            if (response.StatusCode.Equals(HttpStatusCode.OK))
            {
                return response.RawBytes;
            }
            throw new Exception($"{response.StatusCode}, {response.ErrorMessage ?? string.Empty}");
        }

        public async Task<string> AddOrUpdateProject(AutomationProject automationProject, string projectFile)
        {
            RestRequest restRequest = new RestRequest() { Method = Method.POST };
            var projectMetaData = new ProjectMetaData()
            {
                ProjectId = automationProject.ProjectId,
                Type = "ProjectFile"
            };
            restRequest.AddParameter(nameof(ProjectMetaData), serializer.Serialize<ProjectMetaData>(projectMetaData), ParameterType.RequestBody);
            restRequest.AddFile("file", projectFile);
            var result = await client.PostAsync<string>(restRequest);
            return result;
        }


        public async Task<string> AddOrUpdateProjectDataFiles(AutomationProject automationProject, VersionInfo version, string projectFile)
        {
            RestRequest restRequest = new RestRequest() { Method = Method.POST };
            var projectMetaData = new ProjectMetaData()
            {
                ProjectId = automationProject.ProjectId,
                Type = "ProjectDataFiles",
                Version = version.ToString(),
                IsActive = version.IsActive,
                IsDeployed = version.IsDeployed
            };
            restRequest.AddParameter(nameof(ProjectMetaData), serializer.Serialize<ProjectMetaData>(projectMetaData), ParameterType.RequestBody);
            restRequest.AddFile("file", projectFile);
            var result = await client.PostAsync<string>(restRequest);
            return result;
        }
    }
}
