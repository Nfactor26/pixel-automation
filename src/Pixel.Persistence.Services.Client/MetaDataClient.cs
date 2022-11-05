using Dawn;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Services.Client.Interfaces;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public class MetaDataClient : IMetaDataClient
    {
        private readonly IRestClientFactory clientFactory;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="clientFactory"></param>
        public MetaDataClient(IRestClientFactory clientFactory)
        {
           this.clientFactory = Guard.Argument(clientFactory).NotNull().Value;           
        }

        ///<inheritdoc/>
        public async Task<IEnumerable<ApplicationMetaData>> GetApplicationMetaData()
        {
            RestRequest restRequest = new RestRequest("metadata/application");
            var client = this.clientFactory.GetOrCreateClient();
            var response = await client.GetAsync<IEnumerable<ApplicationMetaData>>(restRequest);          
            return response;
        }      
    }
}
