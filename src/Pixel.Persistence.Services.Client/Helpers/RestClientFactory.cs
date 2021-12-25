using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Persistence.Services.Client.Interfaces;
using RestSharp;
using System;
using System.Net.Http;

namespace Pixel.Persistence.Services.Client
{
    public class RestClientFactory : IRestClientFactory
    {
        private readonly ISignInManager signInManager;
        private readonly ApplicationSettings applicationSettings;
       
        private RestClient restClient;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="signInManager"></param>
        public RestClientFactory(ISignInManager signInManager, ApplicationSettings applicationSettings)
        {
            this.signInManager = Guard.Argument(signInManager).NotNull().Value;
            this.applicationSettings = Guard.Argument(applicationSettings).NotNull();
        }

        ///<inheritdoc/>
        public RestClient GetOrCreateClient()
        {
            if(restClient == null)
            {
                var authenticationHandler = signInManager.GetAuthenticationHandler() ??
                    throw new InvalidOperationException("Authentication handler is not available.");              
                var httpClient = new HttpClient(authenticationHandler)
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
