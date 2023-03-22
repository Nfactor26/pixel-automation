using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.RestApi.Shared;
using RestSharp;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;

namespace Pixel.Automation.RestApi.Components
{
    /// <summary>
    /// Actor component to execute a http request using 
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Http Request", "Web Services", iconSource: null, description: "Perform a http operation", tags: new string[] { "Http", "Rest" })]
    public class HttpRequestActorComponent : ActorComponent, IHttpRequestExecutor
    {
        HttpRequest httpRequest = new HttpRequest();
        /// <summary>
        /// Request configuration
        /// </summary>
        [DataMember(Order = 1000)]
        [Browsable(false)]
        public HttpRequest HttpRequest
        {
            get => httpRequest;
            set => httpRequest = value;
        }

        ResponseContentHandling responseContentSettings = new ResponseContentHandling();
        /// <summary>
        /// Response configuration
        /// </summary>
        [DataMember(Order = 1010)]
        [Browsable(false)]
        public ResponseContentHandling ResponseContentSettings
        {
            get => responseContentSettings;
            set => responseContentSettings = value;
        }

        /// <summary>
        /// When supplied, the function will be called before making a request
        /// </summary>
        [DataMember(Order = 1020)]
        [Display(Name = "Before Request", GroupName = "Extension", Order = 40, Description = "[Optional]  When supplied, the function will be called before making a request")]
        public Argument BeforeRequest { get; set; } = new FuncArgument<Func<HttpRequestMessage, ValueTask>>();

        /// <summary>
        /// When supplied, the function will be called after making a request
        /// </summary>
        [DataMember(Order = 1030)]
        [Display(Name = "after Request", GroupName = "Extension", Order = 40, Description = "[Optional]  When supplied, the function will be called after making a request")]
        public Argument AfterRequest { get; set; } = new FuncArgument<Func<HttpResponseMessage, ValueTask>>();

        /// <summary>
        /// constructor
        /// </summary>
        public HttpRequestActorComponent() : base("Http Request", "HttpRequest")
        {

        }

        /// </inheritdoc>
        public async override Task ActAsync()
        {
            await ExecuteRequestAsync(this.httpRequest, this.responseContentSettings);
        }

        /// </inheritdoc>
        public bool CanExecuteRequest
        {
            get => this.EntityManager.GetOwnerApplication<RestApiApplication>(this).RestClient != null;
        }

        /// </inheritdoc>
        public async Task<HttpResponse> ExecuteRequestAsync(HttpRequest httpRequest, ResponseContentHandling responseContentSettings)
        {
            var applicationEntity = this.EntityManager.GetApplicationEntity(this);
            if (applicationEntity is RestApiApplicationEntity restApiApplicationEntity)
            {
                var restClient = this.EntityManager.GetOwnerApplication<RestApiApplication>(this).RestClient;
                string configuredUri = await this.ArgumentProcessor.GetValueAsync<string>(httpRequest.TargetUrl);
                RestRequest restRequest = new RestRequest(configuredUri, (Method)((int)httpRequest.RequestType));
                var scriptEngine = this.EntityManager.GetScriptEngine();

                if (restApiApplicationEntity.ConfigureRequest.IsConfigured())
                {
                    var fn = await scriptEngine.CreateDelegateAsync<Func<RestRequest, ValueTask>>(restApiApplicationEntity.ConfigureRequest.ScriptFile);
                    await fn(restRequest);
                }

                if(this.BeforeRequest.IsConfigured())
                {
                   restRequest.OnBeforeRequest = await scriptEngine.CreateDelegateAsync<Func<HttpRequestMessage, ValueTask>>(this.BeforeRequest.ScriptFile);
                }

                if (this.AfterRequest.IsConfigured())
                {
                    restRequest.OnAfterRequest = await scriptEngine.CreateDelegateAsync<Func<HttpResponseMessage, ValueTask>>(this.AfterRequest.ScriptFile);
                }

                await ConfigurePathSegmentsAsync(httpRequest, restRequest);

                await ConfigureQueryStringsAsync(httpRequest, restRequest);

                await ConfigureHeadersAsync(httpRequest, restRequest);

                await ConfigureRequestBodyAsync(httpRequest, restRequest);

                RestResponse restResponse = null;
                object responseData;

                //execute request
                switch (responseContentSettings.ExpectedResponseType)
                {
                    case ExpectedResponse.Json:
                    case ExpectedResponse.Xml:
                        restResponse = await restClient.ExecuteAsync(restRequest);
                        MethodInfo deserialize = typeof(RestClientExtensions).GetMethod("Deserialize");
                        Type expectedObjectType = responseContentSettings.SaveTo.GetArgumentType();
                        MethodInfo genericDeserialize = deserialize.MakeGenericMethod(expectedObjectType);
                        restResponse = genericDeserialize.Invoke(null, new object[] { restClient, restResponse }) as RestResponse;
                        responseData = restResponse.GetType().GetProperty("Data").GetValue(restResponse);
                        await this.ArgumentProcessor.SetValueAsync<object>(responseContentSettings.SaveTo, responseData);
                        break;
                    case ExpectedResponse.Text:
                        restResponse = await restClient.ExecuteAsync(restRequest);
                        responseData = restResponse.Content;
                        await this.ArgumentProcessor.SetValueAsync<string>(responseContentSettings.SaveTo, responseData.ToString());
                        break;
                    case ExpectedResponse.File:
                        var fileData = await restClient.DownloadDataAsync(restRequest);
                        string saveLocation = await this.ArgumentProcessor.GetValueAsync<string>(responseContentSettings.SaveTo);
                        await File.WriteAllBytesAsync(saveLocation, fileData);
                        restResponse = new RestResponse();
                        break;
                    case ExpectedResponse.Stream:
                        var stream = await restClient.DownloadStreamAsync(restRequest);
                        await this.ArgumentProcessor.SetValueAsync<Stream>(responseContentSettings.SaveTo, stream);
                        restResponse = new RestResponse();
                        break;
                    case ExpectedResponse.Custom:
                        restResponse = await restClient.ExecuteAsync(restRequest);
                        break;
                }

                HttpResponse response = new HttpResponse()
                {
                    Content = restResponse.Content,
                    ContentEncoding = restResponse.ContentEncoding,
                    ContentLength = restResponse.ContentLength,
                    ContentType = restResponse.ContentType,
                    StatusCode = restResponse.StatusCode,
                    RawBytes = restResponse.RawBytes,
                    ResponseUri = restResponse.ResponseUri
                };

                foreach (var header in restResponse.Headers)
                {
                    response.Headers.Add(new ResponseHeader() { HeaderKey = header.Name, HeaderValue = header.Value.ToString() });
                }

                foreach (Cookie cookie in restResponse.Cookies)
                {
                    response.Cookies.Add(cookie);
                }

                if (responseContentSettings.ExpectedResponseType == ExpectedResponse.Custom)
                {
                    await this.ArgumentProcessor.SetValueAsync<HttpResponse>(responseContentSettings.SaveTo, response);
                }

                return response;
            }
            throw new ConfigurationException("Application entity for Http Request actor components should be of type RestApiApplicationEntity");
        }

        private async Task ConfigurePathSegmentsAsync(HttpRequest httpRequest, RestRequest restRequest)
        {
            foreach (var segment in httpRequest.PathSegments)
            {
                if (!segment.IsEnabled || string.IsNullOrEmpty(segment.SegmentKey))
                {
                    continue;
                }
                string segmentValue = await this.ArgumentProcessor.GetValueAsync<string>(segment.SegmentValue);
                restRequest.AddUrlSegment(segment.SegmentKey, segmentValue);
            }
        }

        private async Task ConfigureQueryStringsAsync(HttpRequest httpRequest, RestRequest restRequest)
        {
            foreach (var parameter in httpRequest.RequestParameters)
            {
                if (!parameter.IsEnabled || string.IsNullOrEmpty(parameter.QueryStringKey))
                {
                    continue;
                }
                string parameterValue = await this.ArgumentProcessor.GetValueAsync<string>(parameter.QueryStringValue);
                restRequest.AddQueryParameter(parameter.QueryStringKey, parameterValue);
            }
        }

        private async Task ConfigureHeadersAsync(HttpRequest httpRequest, RestRequest restRequest)
        {
            foreach (var header in httpRequest.RequestHeaders)
            {
                if (!header.IsEnabled || string.IsNullOrEmpty(header.HeaderKey))
                {
                    continue;
                }
                string headerValue = await this.ArgumentProcessor.GetValueAsync<string>(header.HeaderValue);
                restRequest.AddHeader(header.HeaderKey, headerValue);
            }
        }

        private async Task ConfigureRequestBodyAsync(HttpRequest httpRequest, RestRequest restRequest)
        {
            switch (httpRequest.RequestBody)
            {
                case FormDataBodyContent formDataContent:
                    foreach (var formField in formDataContent.FormFields)
                    {
                        if (!formField.IsEnabled || string.IsNullOrEmpty(formField.DataKey))
                        {
                            continue;
                        }
                        string formFieldValue = await this.ArgumentProcessor.GetValueAsync<string>(formField.DataValue);
                        switch (formField.DataType)
                        {
                            case FormDataType.Text:
                                restRequest.AddParameter(formField.DataKey, formFieldValue, ParameterType.RequestBody, formField.Encode);
                                break;
                            case FormDataType.File:
                                if (!File.Exists(formFieldValue))
                                {
                                    throw new FileNotFoundException($"Failed to locate file {formFieldValue} configured for form-data with key {formField.DataKey}");
                                }
                                restRequest.AddFile(formField.DataKey, formFieldValue, formField.ContentType ?? null);
                                break;
                        }

                    }
                    break;
                case RawBodyContent rawContent:
                    switch (rawContent.ContentType)
                    {
                        case "application/json":
                            var jObject = await this.ArgumentProcessor.GetValueAsync<object>(rawContent.Content);
                            restRequest.AddJsonBody(jObject, rawContent.ContentType);
                            break;
                        case "application/xml":
                            var xObject = await this.ArgumentProcessor.GetValueAsync<object>(rawContent.Content);
                            restRequest.AddXmlBody(xObject, rawContent.ContentType);
                            break;
                        default:
                            var stringObject = await this.ArgumentProcessor.GetValueAsync<string>(rawContent.Content);
                            restRequest.AddStringBody(stringObject, rawContent.ContentType);
                            break;
                    }
                    break;
                case BinaryBodyContent binaryContent:
                    var filePath = await this.ArgumentProcessor.GetValueAsync<string>(binaryContent.Content);
                    restRequest.AddBody(File.ReadAllBytes(filePath), binaryContent.ContentType);
                    break;

            }
        }
    }
}
