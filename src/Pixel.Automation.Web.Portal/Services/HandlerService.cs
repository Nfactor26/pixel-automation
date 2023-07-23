using Microsoft.AspNetCore.WebUtilities;
using Pixel.Automation.Web.Portal.Helpers;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using Pixel.Persistence.Core.Response;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Services
{
    public interface IHandlerService
    {
        /// <summary>
        /// Get all the available handlers
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TemplateHandler>> GetAllAsync();

        /// <summary>
        /// Get all the available template handlers based on request
        /// </summary>
        /// <returns></returns>
        Task<PagedList<TemplateHandler>> GetHandlersAsync(GetHandlersRequest getHandlersRequest);

        /// <summary>
        /// Get template handler with a given template name
        /// </summary>
        /// <param name="handlerName"></param>
        /// <returns></returns>
        Task<TemplateHandler> GetHandlerByNameAsync(string handlerName);

        /// <summary>
        /// Get template handler with a given Id
        /// </summary>
        /// <param name="handlerId"></param>
        /// <returns></returns>
        Task<TemplateHandler> GetHandlerByIdAsync(string handlerId);

        /// <summary>
        /// Add a new template handler
        /// </summary>
        /// <param name="templateHandler"></param>
        /// <returns></returns>
        Task<OperationResult> AddHandlerAsync(TemplateHandler templateHandler);

        /// <summary>
        /// Update details of an existing template handler
        /// </summary>
        /// <param name="templateHandler"></param>
        /// <returns></returns>
        Task<OperationResult> UpdateHandlerAsync(TemplateHandler templateHandler);      

        /// <summary>
        /// Add a new template handler
        /// </summary>
        /// <param name="templateHandler"></param>
        /// <returns></returns>
        //Task<OperationResult> AddHandlerAsync(TemplateHandlerViewModel templateHandler, IBrowserFile browserFile);

        /// <summary>
        /// Update details of an existing template handler
        /// </summary>
        /// <param name="templateHandler"></param>
        /// <returns></returns>
        //Task<OperationResult> UpdateHandlerAsync(TemplateHandlerViewModel templateHandler, IBrowserFile browserFile);

        /// <summary>
        /// Delete an existing template handler
        /// </summary>
        /// <param name="templateHandler"></param>
        /// <returns></returns>
        Task<OperationResult> DeleteHandlerAsync(TemplateHandler templateHandler);
        
    }

    public class HandlerService : IHandlerService
    {
        private readonly HttpClient httpClient;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="httpClient"></param>
        public HandlerService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TemplateHandler>> GetAllAsync()
        {
            return await this.httpClient.GetFromJsonAsync<IEnumerable<TemplateHandler>>("api/handlers");
        }

        /// <inheritdoc/>
        public async Task<PagedList<TemplateHandler>> GetHandlersAsync(GetHandlersRequest request)
        {
            var queryStringParam = new Dictionary<string, string>
            {
                ["currentPage"] = request.CurrentPage.ToString(),
                ["pageSize"] = request.PageSize.ToString()
            };
            if (!string.IsNullOrEmpty(request.HandlerFilter))
            {
                queryStringParam.Add("handlerFilter", request.HandlerFilter);
            }
            return await this.httpClient.GetFromJsonAsync<PagedList<TemplateHandler>>(QueryHelpers.AddQueryString("api/handlers/paged", queryStringParam));
        }

        /// <inheritdoc/>
        public async Task<TemplateHandler> GetHandlerByNameAsync(string handlerName)
        {
            return await httpClient.GetFromJsonAsync<TemplateHandler>($"api/handlers/name/{handlerName}");
        }

        /// <inheritdoc/>
        public async Task<TemplateHandler> GetHandlerByIdAsync(string handlerId)
        {
            return await httpClient.GetFromJsonAsync<TemplateHandler>($"api/handlers/id/{handlerId}");
        }
        
        /// <inheritdoc/>
        public async Task<OperationResult> DeleteHandlerAsync(TemplateHandler templateHandler)
        {
            var result = await httpClient.DeleteAsync($"api/handlers/{templateHandler.Id}");
            return await OperationResult.FromResponseAsync(result);
        }

        /// <inheritdoc/>
        public async Task<OperationResult> AddHandlerAsync(TemplateHandler templateHandler)
        {
            var result = await httpClient.PostAsJsonAsync<TemplateHandler>($"api/handlers", templateHandler);
            return await OperationResult.FromResponseAsync(result);
        }

        /// <inheritdoc/>
        public async Task<OperationResult> UpdateHandlerAsync(TemplateHandler templateHandler)
        {
            var result = await httpClient.PutAsJsonAsync<TemplateHandler>($"api/handlers", templateHandler);
            return await OperationResult.FromResponseAsync(result);
        }       
    }
}
