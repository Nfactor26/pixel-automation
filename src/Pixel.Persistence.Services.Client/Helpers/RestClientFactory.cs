using Dawn;
using Pixel.Automation.Core;
using Pixel.Persistence.Services.Client.Interfaces;
using RestSharp;

namespace Pixel.Persistence.Services.Client
{
    public class RestClientFactory : IRestClientFactory
    {
        private readonly ApplicationSettings applicationSettings;
        private RestClient restClient;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="applicationSettings"></param>
        public RestClientFactory(ApplicationSettings applicationSettings)
        {
            this.applicationSettings = Guard.Argument(applicationSettings).NotNull();           
        }

        ///<inheritdoc/>
        public RestClient GetOrCreateClient()
        {
            if(restClient == null)
            {
                restClient = new RestClient(applicationSettings.PersistenceServiceUri);
            }           
            return restClient;
        }
    }
}
