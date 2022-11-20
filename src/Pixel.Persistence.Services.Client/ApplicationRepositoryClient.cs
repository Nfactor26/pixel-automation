using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
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
            restRequest.AddHeader(nameof(laterThan), laterThan.ToString("O"));
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
        }

        ///<inheritdoc/>
        public async Task AddOrUpdateApplication(ApplicationDescription applicationDescription)
        {
            Guard.Argument(applicationDescription).NotNull();
            logger.Debug("Add Or Update {@ApplicationDescription}", applicationDescription);

            RestRequest restRequest = new RestRequest("/application") { Method = Method.Post };
            restRequest.AddJsonBody(serializer.Serialize<ApplicationDescription>(applicationDescription));
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteAsync(restRequest, Method.Post);
            result.EnsureSuccess();
        }

    }
}
