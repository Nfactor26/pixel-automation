using Dawn;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Pixel.Persistence.Core.Models;
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
    public class ControlRepositoryClient : IControlRepositoryClient
    {
        private readonly ILogger logger = Log.ForContext<ControlRepositoryClient>();      
        private readonly ISerializer serializer;
        private readonly IRestClientFactory clientFactory;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="clientFactory"></param>
        /// <param name="serializer"></param>
        public ControlRepositoryClient(IRestClientFactory clientFactory, ISerializer serializer)
        {
            this.clientFactory = Guard.Argument(clientFactory).NotNull().Value;
            this.serializer = Guard.Argument(serializer).NotNull().Value;           
        }

        ///<inheritdoc/>
        public async Task<IEnumerable<ControlDescription>> GetControls(string applicationId, DateTime laterThan)
        {
            Guard.Argument(applicationId).NotNull().NotEmpty();       
            RestRequest restRequest = new RestRequest($"control/{applicationId}") { Method = Method.Get, RequestFormat = DataFormat.Json };
            restRequest.AddHeader(nameof(laterThan), laterThan.ToString("O"));          
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteGetAsync(restRequest);
            result.EnsureSuccess();
            using (var stream = new MemoryStream(result.RawBytes))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return serializer.DeserializeContent<List<ControlDescription>>(reader.ReadToEnd());
                }
            }
        }

        ///<inheritdoc/>
        public async Task<IEnumerable<ControlImageDataFile>> GetControlImages(string applicationId, DateTime laterThan)
        {
            Guard.Argument(applicationId).NotNull().NotEmpty();
            RestRequest restRequest = new RestRequest($"control/image/{applicationId}") { Method = Method.Get, RequestFormat = DataFormat.Json };
            restRequest.AddHeader(nameof(laterThan), laterThan.ToString("O"));
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteGetAsync(restRequest);
            result.EnsureSuccess();
            using (var stream = new MemoryStream(result.RawBytes))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return serializer.DeserializeContent<List<ControlImageDataFile>>(reader.ReadToEnd());
                }
            }
        }

        ///<inheritdoc/>
        public async Task AddControlToScreen(ControlDescription controlDescription, string screenId)
        {
            Guard.Argument(screenId, nameof(screenId)).NotNull().NotEmpty().NotWhiteSpace();
            Guard.Argument(controlDescription, nameof(controlDescription)).NotNull();           
            var addControlRequest = new AddControlRequest(controlDescription.ApplicationId, controlDescription.ControlId, screenId, controlDescription);
            RestRequest restRequest = new RestRequest("control");
            restRequest.AddJsonBody(serializer.Serialize<AddControlRequest>(addControlRequest));
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteAsync(restRequest, Method.Post);
            result.EnsureSuccess();
        }       

        ///<inheritdoc/>
        public async Task UpdateControl(ControlDescription controlDescription)
        {
            Guard.Argument(controlDescription, nameof(controlDescription)).NotNull();
            RestRequest restRequest = new RestRequest("control");
            restRequest.AddJsonBody(serializer.Serialize<ControlDescription>(controlDescription));
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteAsync(restRequest, Method.Put);
            result.EnsureSuccess();
        }

        ///<inheritdoc/>
        public async Task DeleteControl(ControlDescription controlDescription)
        {
            Guard.Argument(controlDescription).NotNull();
            RestRequest restRequest = new RestRequest($"control/{controlDescription.ApplicationId}/{controlDescription.ControlId}");
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteAsync(restRequest, Method.Delete);
            result.EnsureSuccess();
        }

        ///<inheritdoc/>
        public async Task AddOrUpdateControlImage(ControlDescription controlDescription, string imageFile)
        {
            Guard.Argument(controlDescription).NotNull();
            Guard.Argument(imageFile).NotNull().NotEmpty();           
            logger.Debug("Add or update control image for control : {0} with Id : {1}", controlDescription.ControlName, controlDescription.ControlId);

            RestRequest restRequest = new RestRequest("control/image");
            var controlImageMetaData = new ControlImageMetaData()
            {
                ApplicationId = controlDescription.ApplicationId,
                ControlId = controlDescription.ControlId,
                Version = controlDescription.Version.ToString(),
                FileName = Path.GetFileName(imageFile)
            };
            restRequest.AddParameter(nameof(ControlImageMetaData), serializer.Serialize<ControlImageMetaData>(controlImageMetaData), ParameterType.RequestBody);
            restRequest.AddFile("file", imageFile);
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteAsync(restRequest, Method.Post);
            result.EnsureSuccess();
        }

        public async Task DeleteControlImageAsync(ControlDescription controlDescription, string imageFile)
        {
            Guard.Argument(controlDescription).NotNull();
            Guard.Argument(imageFile).NotNull().NotEmpty();
            string fileName = Path.GetFileName(imageFile);
            logger.Debug("Delete control image {0} for control : {1} with Id : {2}", fileName, controlDescription.ControlName, controlDescription.ControlId);

            RestRequest restRequest = new RestRequest("control/image/delete");
            var controlImageMetaData = new ControlImageMetaData()
            {
                ApplicationId = controlDescription.ApplicationId,
                ControlId = controlDescription.ControlId,
                Version = controlDescription.Version.ToString(),
                FileName = fileName
            };
            restRequest.AddJsonBody(serializer.Serialize<ControlImageMetaData>(controlImageMetaData));    
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteAsync(restRequest, Method.Delete);
            result.EnsureSuccess();
        }
    }
}
