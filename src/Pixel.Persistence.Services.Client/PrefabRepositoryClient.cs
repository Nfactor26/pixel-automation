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
    public class PrefabRepositoryClient : IPrefabRepositoryClient
    {
        private readonly ILogger logger = Log.ForContext<PrefabRepositoryClient>();
        private readonly IRestClientFactory clientFactory;
        private readonly ISerializer serializer;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="clientFactory"></param>
        /// <param name="serializer"></param>
        public PrefabRepositoryClient(IRestClientFactory clientFactory, ISerializer serializer)
        {
            this.clientFactory = Guard.Argument(clientFactory).NotNull().Value;
            this.serializer = Guard.Argument(serializer).NotNull().Value;        
        }

        ///<inheritdoc/>
        public async Task<PrefabProject> GetPrefabFileAsync(string prefabId)
        {
            Guard.Argument(prefabId).NotNull().NotEmpty();
            logger.Debug("Get Prefab file for prefabId : {0}", prefabId);

            RestRequest restRequest = new RestRequest($"prefab/{prefabId}");
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteGetAsync(restRequest);
            result.EnsureSuccess();
            using (var stream = new MemoryStream(result.RawBytes))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return serializer.DeserializeContent<PrefabProject>(reader.ReadToEnd());
                }
            }
        }

        ///<inheritdoc/>
        public async Task<byte[]> GetPrefabDataFilesAsync(string prefabId, string version)
        {
            Guard.Argument(prefabId).NotNull().NotEmpty();
            Guard.Argument(version).NotNull().NotEmpty();
            logger.Debug("Get Prefab data files for prefabId : {0} and version : {1}", prefabId, version);

            RestRequest restRequest = new RestRequest($"prefab/{prefabId}/{version}");
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteGetAsync(restRequest);
            result.EnsureSuccess();
            return result.RawBytes;
        }

        ///<inheritdoc/>
        public async Task AddOrUpdatePrefabAsync(PrefabProject prefabProject, string prefabFile)
        {
            Guard.Argument(prefabProject).NotNull();
            Guard.Argument(prefabFile).NotNull().NotEmpty();
            logger.Debug("Add or update {@PrefabDescription}", prefabProject);

            RestRequest restRequest = new RestRequest("prefab") { Method = Method.Post };
            var projectMetaData = new PrefabMetaData()
            {
                PrefabId = prefabProject.PrefabId,
                ApplicationId = prefabProject.ApplicationId,
                Type = "PrefabFile"
            };
            restRequest.AddParameter(nameof(PrefabMetaData), serializer.Serialize<PrefabMetaData>(projectMetaData), ParameterType.RequestBody);
            restRequest.AddFile("file", prefabFile);
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteAsync(restRequest);
            result.EnsureSuccess();
        }

        ///<inheritdoc/>
        public async Task AddOrUpdatePrefabDataFilesAsync(PrefabProject prefabProject, VersionInfo version, string zippedDataFile)
        {
            Guard.Argument(prefabProject).NotNull();
            Guard.Argument(version).NotNull();
            Guard.Argument(zippedDataFile).NotNull().NotEmpty();
            logger.Debug("Add or update Prefab data files for Prefab : {0}", prefabProject.PrefabName);

            RestRequest restRequest = new RestRequest("prefab") { Method = Method.Post };
            var prefabMetaData = new PrefabMetaData()
            {
                PrefabId = prefabProject.PrefabId,
                ApplicationId = prefabProject.ApplicationId,
                Type = "PrefabDataFiles",
                Version = version.ToString(),
                IsActive = version.IsActive
            };
            restRequest.AddParameter(nameof(PrefabMetaData), serializer.Serialize<PrefabMetaData>(prefabMetaData), ParameterType.RequestBody);
            restRequest.AddFile("file", zippedDataFile);
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteAsync(restRequest);
            result.EnsureSuccess();
        }
    }
}
