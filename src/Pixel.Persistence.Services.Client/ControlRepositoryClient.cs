using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Services.Client.Interfaces;
using RestSharp;
using Serilog;
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
        public async Task<byte[]> GetControls(GetControlDataForApplicationRequest controlDataRequest)
        {
            Guard.Argument(controlDataRequest).NotNull();
            logger.Debug("Get controls for applicationId : {0}", controlDataRequest.ApplicationId);

            //Note : RestSharp doesn't support content body in get request. Hence, we are adding as query string
            RestRequest restRequest = new RestRequest("control") { Method = Method.GET, RequestFormat = DataFormat.Json };
            restRequest.AddParameter(nameof(GetControlDataForApplicationRequest.ApplicationId), controlDataRequest.ApplicationId, ParameterType.QueryString);
            foreach(var controlId in controlDataRequest.ControlIdCollection)
            {
                restRequest.AddParameter(nameof(GetControlDataForApplicationRequest.ControlIdCollection), controlId, ParameterType.QueryString);
            }

            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteGetAsync(restRequest);
            result.EnsureSuccess();
            return result.RawBytes;
        }

        ///<inheritdoc/>
        public async Task AddOrUpdateControl(ControlDescription controlDescription)
        {
            Guard.Argument(controlDescription).NotNull();
            logger.Debug("Add or update {@ControlDescription}", controlDescription);

            RestRequest restRequest = new RestRequest("control") { Method = Method.POST };
            restRequest.AddJsonBody(serializer.Serialize<ControlDescription>(controlDescription));
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteAsync(restRequest, Method.POST);
            result.EnsureSuccess();
        }

        ///<inheritdoc/>
        public async Task AddOrUpdateControlImage(ControlDescription controlDescription, string imageFile, string resolution)
        {
            Guard.Argument(controlDescription).NotNull();
            Guard.Argument(imageFile).NotNull().NotEmpty();
            Guard.Argument(resolution).NotNull().NotEmpty();
            logger.Debug("Add or update control image for control : {0} with Id : {1}", controlDescription.ControlName, controlDescription.ControlId);

            RestRequest restRequest = new RestRequest("control/image") { Method = Method.POST };
            var controlImageMetaData = new ControlImageMetaData()
            {
                ApplicationId = controlDescription.ApplicationId,
                ControlId = controlDescription.ControlId,
                Resolution = resolution ?? "Default"

            };
            restRequest.AddParameter(nameof(ControlImageMetaData), serializer.Serialize<ControlImageMetaData>(controlImageMetaData), ParameterType.RequestBody);
            restRequest.AddFile("file", imageFile);
            var client = this.clientFactory.GetOrCreateClient();
            var result = await client.ExecuteAsync(restRequest);
            result.EnsureSuccess();
        }
    }
}
