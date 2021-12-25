using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Services.Client.Interfaces;
using RestSharp;
using Serilog;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public class ProjectRepositoryClient : IProjectRepositoryClient
    {
        private readonly ILogger logger = Log.ForContext<ProjectRepositoryClient>();
        private readonly IRestClientFactory clientFactory;
        private readonly ISerializer serializer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="applicationSettings"></param>
        public ProjectRepositoryClient(IRestClientFactory clientFactory, ISerializer serializer)
        {
            this.clientFactory = Guard.Argument(clientFactory).NotNull().Value;
            this.serializer = Guard.Argument(serializer).NotNull().Value;
        }

        ///<inheritdoc/>
        public async Task<AutomationProject> GetProjectFile(string projectId)
        {
            Guard.Argument(projectId).NotNull().NotEmpty();
            logger.Debug("Get Project file for projectId : {0}", projectId);

            RestRequest restRequest = new RestRequest($"project/{projectId}");
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

        ///<inheritdoc/>
        public async Task<byte[]> GetProjectDataFiles(string projectId, string version)
        {
            Guard.Argument(projectId).NotNull().NotEmpty();
            Guard.Argument(version).NotNull().NotEmpty();
            logger.Debug("Get project data files for projectId : {0} and version : {1}", projectId, version);

            RestRequest restRequest = new RestRequest($"project/{projectId}/{version}");
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteGetAsync(restRequest);
            result.EnsureSuccess();
            return result.RawBytes;
        }

        ///<inheritdoc/>
        public async Task AddOrUpdateProject(AutomationProject automationProject, string projectFile)
        {
            Guard.Argument(automationProject).NotNull();
            Guard.Argument(projectFile).NotNull().NotEmpty();
            logger.Debug("Add or update project {@AutomationProject}", automationProject);

            RestRequest restRequest = new RestRequest("project") { Method = Method.Post };
            var projectMetaData = new ProjectMetaData()
            {
                ProjectId = automationProject.ProjectId,
                Type = "ProjectFile"
            };
            restRequest.AddParameter(nameof(ProjectMetaData), serializer.Serialize<ProjectMetaData>(projectMetaData), ParameterType.RequestBody);
            restRequest.AddFile("file", projectFile);
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteAsync(restRequest);
            result.EnsureSuccess();
        }

        ///<inheritdoc/>
        public async Task AddOrUpdateProjectDataFiles(AutomationProject automationProject, VersionInfo version, string zippedDataFile)
        {
            Guard.Argument(automationProject).NotNull();
            Guard.Argument(version).NotNull();
            Guard.Argument(zippedDataFile).NotNull().NotEmpty();
            logger.Debug("Add or update project data files for project : {0}", automationProject.Name);

            RestRequest restRequest = new RestRequest("project") { Method = Method.Post };
            var projectMetaData = new ProjectMetaData()
            {
                ProjectId = automationProject.ProjectId,
                Type = "ProjectDataFiles",
                Version = version.ToString(),
                IsActive = version.IsActive,
                IsDeployed = version.IsDeployed
            };
            restRequest.AddParameter(nameof(ProjectMetaData), serializer.Serialize<ProjectMetaData>(projectMetaData), ParameterType.RequestBody);
            restRequest.AddFile("file", zippedDataFile);
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteAsync(restRequest);
            result.EnsureSuccess();
        }
    }
}
