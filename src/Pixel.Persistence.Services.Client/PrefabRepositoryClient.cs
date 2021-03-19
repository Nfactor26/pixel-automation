using Dawn;
using Pixel.Automation.Core;
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
    public class PrefabRepositoryClient : IPrefabRepositoryClient
    {
        private readonly string baseUrl;
        private readonly ISerializer serializer;

        public PrefabRepositoryClient(ISerializer serializer, ApplicationSettings applicationSettings)
        {
            Guard.Argument(serializer, nameof(serializer)).NotNull();
            Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
            this.serializer = serializer;
            this.baseUrl = $"{applicationSettings.PersistenceServiceUri}/Prefab";
        }

        public async Task<PrefabDescription> GetPrefabFileAsync(string prefabId)
        {
            RestRequest restRequest = new RestRequest($"{prefabId}");
            var client = new RestClient(baseUrl);
            var response = await client.ExecuteGetAsync(restRequest);
            if (response.StatusCode.Equals(HttpStatusCode.OK))
            {
                using (var stream = new MemoryStream(response.RawBytes))
                {
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        return serializer.DeserializeContent<PrefabDescription>(reader.ReadToEnd());
                    }
                }
            }
            throw new Exception($"{response.StatusCode}, {response.ErrorMessage ?? string.Empty}");
        }

        public async Task<byte[]> GetPrefabDataFilesAsync(string prefabId, string version)
        {
            RestRequest restRequest = new RestRequest($"{prefabId}/{version}");
            var client = new RestClient(baseUrl);
            var response = await client.ExecuteGetAsync(restRequest);
            if (response.StatusCode.Equals(HttpStatusCode.OK))
            {
                return response.RawBytes;
            }
            throw new Exception($"{response.StatusCode}, {response.ErrorMessage ?? string.Empty}");
        }

        public async Task<string> AddOrUpdatePrefabAsync(PrefabDescription prefabDescription, string prefabDescriptionFile)
        {
            RestRequest restRequest = new RestRequest() { Method = Method.POST };
            var projectMetaData = new PrefabMetaData()
            {
                PrefabId = prefabDescription.PrefabId,
                ApplicationId = prefabDescription.ApplicationId,
                Type = "PrefabFile"
            };
            restRequest.AddParameter(nameof(PrefabMetaData), serializer.Serialize<PrefabMetaData>(projectMetaData), ParameterType.RequestBody);
            restRequest.AddFile("file", prefabDescriptionFile);
            var client = new RestClient(baseUrl);
            var result = await client.PostAsync<string>(restRequest);
            return result;
        }


        public async Task<string> AddOrUpdatePrefabDataFilesAsync(PrefabDescription prefabDescription, VersionInfo version, string zippedDataFile)
        {
            RestRequest restRequest = new RestRequest() { Method = Method.POST };
            var prefabMetaData = new PrefabMetaData()
            {
                PrefabId = prefabDescription.PrefabId,
                ApplicationId = prefabDescription.ApplicationId,
                Type = "PrefabDataFiles",
                Version = version.ToString(),
                IsActive = version.IsActive,
                IsDeployed = version.IsDeployed
            };
            restRequest.AddParameter(nameof(PrefabMetaData), serializer.Serialize<PrefabMetaData>(prefabMetaData), ParameterType.RequestBody);
            restRequest.AddFile("file", zippedDataFile);
            var client = new RestClient(baseUrl);
            var result = await client.PostAsync<string>(restRequest);
            return result;
        }
    }
}
