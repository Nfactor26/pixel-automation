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
    public interface ITemplateService
    {
        /// <summary>
        /// Get all the available session templates based on request
        /// </summary>
        /// <returns></returns>
        Task<PagedList<SessionTemplate>> GetTemplatesAsync(GetTemplatesRequest getTemplatesRequest);

        /// <summary>
        /// Get session template with a given template id
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns></returns>
        Task<SessionTemplate> GetTemplateByIdAsync(string templateId);

        /// <summary>
        /// Get session template with a given template name
        /// </summary>
        /// <param name="templateName"></param>
        /// <returns></returns>
        Task<SessionTemplate> GetTemplateByNameAsync(string templateName);

        /// <summary>
        /// Add a new session template
        /// </summary>
        /// <param name="applicationDescriptor"></param>
        /// <returns></returns>
        Task<OperationResult> AddTemplateAsync(SessionTemplate sessionTemplate);

        /// <summary>
        /// Update details of an existing session template
        /// </summary>
        /// <param name="sessionTemplate"></param>
        /// <returns></returns>
        Task<OperationResult> UpdateTemplateAsync(SessionTemplate sessionTemplate);

        /// <summary>
        /// Delete an existing session template
        /// </summary>
        /// <param name="sessionTemplate"></param>
        /// <returns></returns>
        Task<OperationResult> DeleteTemplateAsync(SessionTemplate sessionTemplate);
    }

    public class TemplateService : ITemplateService
    {
        private readonly HttpClient httpClient;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="httpClient"></param>
        public TemplateService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <inheritdoc/>
        public async Task<PagedList<SessionTemplate>> GetTemplatesAsync(GetTemplatesRequest request)
        {
            var queryStringParam = new Dictionary<string, string>
            {
                ["currentPage"] = request.CurrentPage.ToString(),
                ["pageSize"] = request.PageSize.ToString()
            };
            if (!string.IsNullOrEmpty(request.TemplateFilter))
            {
                queryStringParam.Add("templateFilter", request.TemplateFilter);
            }
            return await this.httpClient.GetFromJsonAsync<PagedList<SessionTemplate>>(QueryHelpers.AddQueryString("api/templates/paged", queryStringParam));
        }

        /// <inheritdoc/>
        public async Task<SessionTemplate> GetTemplateByIdAsync(string templateId)
        {
            return await httpClient.GetFromJsonAsync<SessionTemplate>($"api/templates/id/{templateId}");
        }

        /// <inheritdoc/>
        public async Task<SessionTemplate> GetTemplateByNameAsync(string templateName)
        {
            return await httpClient.GetFromJsonAsync<SessionTemplate>($"api/templates/name/{templateName}");
        }

        /// <inheritdoc/>
        public async Task<OperationResult> AddTemplateAsync(SessionTemplate sessionTemplate)
        {
            var result = await httpClient.PostAsJsonAsync<SessionTemplate>("api/templates", sessionTemplate);
            return await OperationResult.FromResponseAsync(result);
        }

        /// <inheritdoc/>
        public async Task<OperationResult> UpdateTemplateAsync(SessionTemplate sessionTemplate)
        {
            var result = await httpClient.PutAsJsonAsync<SessionTemplate>("api/templates", sessionTemplate);
            return await OperationResult.FromResponseAsync(result);
        }

        /// <inheritdoc/>
        public async Task<OperationResult> DeleteTemplateAsync(SessionTemplate sessionTemplate)
        {
            var result = await httpClient.DeleteAsync($"api/templates/{sessionTemplate.Id}");
            return await OperationResult.FromResponseAsync(result);
        }       
    }
}
