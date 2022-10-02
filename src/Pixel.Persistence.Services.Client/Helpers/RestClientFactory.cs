using Dawn;
using Pixel.Automation.Core;
using Pixel.Persistence.Services.Client.Interfaces;
using RestSharp;
using System;
using System.Net.Http;

namespace Pixel.Persistence.Services.Client
{
    public class RestClientFactory : IRestClientFactory
    {       
        private readonly ApplicationSettings applicationSettings;
       
        private RestClient restClient;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="signInManager"></param>
        public RestClientFactory(ApplicationSettings applicationSettings)
        {          
            this.applicationSettings = Guard.Argument(applicationSettings).NotNull();
        }

        ///<inheritdoc/>
        public RestClient GetOrCreateClient()
        {
            if(restClient == null)
            {                              
                var httpClient = new HttpClient()
                {
                    BaseAddress = new Uri(applicationSettings.PersistenceServiceUri)
                };                          
                restClient = new RestClient(httpClient, new RestClientOptions()
                {
                     BaseUrl = new Uri(applicationSettings.PersistenceServiceUri)
                });
            }           
            return restClient;
        }
    }
}
