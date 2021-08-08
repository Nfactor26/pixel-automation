﻿using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Persistence.Core.Models;
using RestSharp;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public class ControlRepositoryClient : IControlRepositoryClient
    {
        private readonly string baseUrl;     
        private readonly ISerializer serializer;

        public ControlRepositoryClient(ISerializer serializer, ApplicationSettings applicationSettings)
        {
            Guard.Argument(serializer, nameof(serializer)).NotNull();
            Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
            this.serializer = serializer;
            this.baseUrl = $"{applicationSettings.PersistenceServiceUri}/Control";
        }

        public async Task<byte[]> GetControls(GetControlDataForApplicationRequest controlDataRequest)
        {
            RestRequest restRequest = new RestRequest() { Method = Method.GET, RequestFormat = DataFormat.Json };
            restRequest.AddParameter(nameof(GetControlDataForApplicationRequest.ApplicationId), controlDataRequest.ApplicationId, ParameterType.QueryString);
            foreach(var controlId in controlDataRequest.ControlIdCollection)
            {
                restRequest.AddParameter(nameof(GetControlDataForApplicationRequest.ControlIdCollection), controlId, ParameterType.QueryString);
            }
            var client = new RestClient(baseUrl);
            var response = await client.ExecuteGetAsync(restRequest);
            if (response.StatusCode.Equals(HttpStatusCode.OK))
            {
                return response.RawBytes;
            }
            throw new Exception($"{response.StatusCode}, {response.ErrorMessage ?? "Failed to download controls for application with id :" + controlDataRequest.ApplicationId}");
        }

        public async Task AddOrUpdateControl(ControlDescription controlDescription)
        {
            Guard.Argument(controlDescription, nameof(controlDescription)).NotNull();

            RestRequest restRequest = new RestRequest() { Method = Method.POST };
            restRequest.AddJsonBody(serializer.Serialize<ControlDescription>(controlDescription));
            var client = new RestClient(baseUrl);
            var result = await client.ExecuteAsync(restRequest, Method.POST);
            if (!result.IsSuccessful)
            {
                throw new Exception(result.ErrorMessage, result.ErrorException);
            }
        }

        public async Task AddOrUpdateControlImage(ControlDescription controlDescription, string imageFile, string resolution)
        {
            RestRequest restRequest = new RestRequest("image") { Method = Method.POST };
            var controlImageMetaData = new ControlImageMetaData()
            {
                ApplicationId = controlDescription.ApplicationId,
                ControlId = controlDescription.ControlId,
                Resolution = resolution ?? "Default"

            };
            restRequest.AddParameter(nameof(ControlImageMetaData), serializer.Serialize<ControlImageMetaData>(controlImageMetaData), ParameterType.RequestBody);
            restRequest.AddFile("file", imageFile);
            var client = new RestClient(baseUrl);
            await client.PostAsync<string>(restRequest);           
        }
    }
}
