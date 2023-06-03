using Pixel.Automation.Web.Portal.Helpers;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Portal.Services
{
    public interface ITriggerService
    {
        /// <summary>
        /// Add a new trigger to a template
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="trigger"></param>
        /// <returns></returns>
        Task<OperationResult> AddTriggerAsync(string templateId, SessionTrigger trigger);

        /// <summary>
        /// Remove trigger from template
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="trigger"></param>
        /// <returns></returns>
        Task<OperationResult> DeleteTriggerAsync(string templateId, SessionTrigger trigger);

        /// <summary>
        /// Update details of an existing trigger on template
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="original"></param>
        /// <param name="modified"></param>
        /// <returns></returns>
        Task<OperationResult> UpdateTriggerAsync(string templateId, SessionTrigger original, SessionTrigger modified);

        /// <summary>
        /// Get the next fire time of a given trigger belonging to given template
        /// </summary>
        /// <param name="templateName"></param>
        /// <param name="triggerName"></param>
        /// <returns></returns>
        Task<DateTimeOffset?> GetNextFireTimeAsync(string templateName, string triggerName);

        /// <summary>
        /// Pause all the triggers belonging to given template
        /// </summary>
        /// <param name="templateName"></param>
        /// <returns></returns>
        Task<OperationResult> PauseTemplateAsync(string templateName);

        /// <summary>
        /// Resume all the triggers belonging to given template
        /// </summary>
        /// <param name="templateName"></param>
        /// <returns></returns>
        Task<OperationResult> ResumeTemplateAsync(string templateName);

        /// <summary>
        /// Pause a given trigger belonging to a template 
        /// </summary>
        /// <param name="templateName"></param>
        /// <param name="triggerName"></param>
        /// <returns></returns>
        Task<OperationResult> PauseTriggerAsync(string templateName, string triggerName);

        /// <summary>
        /// Resume a given trigger belonging to a template
        /// </summary>
        /// <param name="templateName"></param>
        /// <param name="triggerName"></param>
        /// <returns></returns>
        Task<OperationResult> ResumeTriggerAsync(string templateName, string triggerName);

        /// <summary>
        /// Schedule the trigger to execute now
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="trigger"></param>
        /// <returns></returns>
        Task<OperationResult> ScheduleTriggerNowAsync(string templateId, SessionTrigger trigger);

    }

    public class TriggerService : ITriggerService
    {

        private readonly HttpClient httpClient;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="httpClient"></param>
        public TriggerService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <inheritdoc/>
        public async Task<OperationResult> AddTriggerAsync(string templateId, SessionTrigger trigger)
        {
            var result = await httpClient.PostAsJsonAsync<AddTriggerRequest>($"api/triggers/add", new AddTriggerRequest(templateId, trigger));
            return await OperationResult.FromResponseAsync(result);
        }

        /// <inheritdoc/>
        public async Task<OperationResult> UpdateTriggerAsync(string templateId, SessionTrigger original, SessionTrigger modified)
        {
            var result = await httpClient.PutAsJsonAsync<UpdateTriggerRequest>($"api/triggers/update", new UpdateTriggerRequest(templateId, original, modified));
            return await OperationResult.FromResponseAsync(result);
        }

        /// <inheritdoc/>
        public async Task<OperationResult> DeleteTriggerAsync(string templateId, SessionTrigger trigger)
        {
            var result = await httpClient.DeleteAsync($"api/triggers/delete/{templateId}/{trigger.Name}");
            return await OperationResult.FromResponseAsync(result);
        }

        /// <inheritdoc/>
        public async Task<DateTimeOffset?> GetNextFireTimeAsync(string templateName, string triggerName)
        {
            return await httpClient.GetFromJsonAsync<DateTimeOffset?>($"api/triggers/next/{templateName}/{triggerName}");         
        }

        /// <inheritdoc/>
        public async Task<OperationResult> PauseTemplateAsync(string templateName)
        {
            var result = await httpClient.GetAsync($"api/triggers/pause/{templateName}");
            return await OperationResult.FromResponseAsync(result);
        }

        /// <inheritdoc/>
        public async Task<OperationResult> ResumeTemplateAsync(string templateName)
        {
            var result = await httpClient.GetAsync($"api/triggers/resume/{templateName}");
            return await OperationResult.FromResponseAsync(result);
        }

        /// <inheritdoc/>
        public async Task<OperationResult> PauseTriggerAsync(string templateName, string triggerName)
        {
            var result = await httpClient.GetAsync($"api/triggers/pause/{templateName}/{triggerName}");
            return await OperationResult.FromResponseAsync(result);
        }

        /// <inheritdoc/>
        public async Task<OperationResult> ResumeTriggerAsync(string templateName, string triggerName)
        {
            var result = await httpClient.GetAsync($"api/triggers/resume/{templateName}/{triggerName}");
            return await OperationResult.FromResponseAsync(result);
        }

        /// <inheritdoc/>
        public async Task<OperationResult> ScheduleTriggerNowAsync(string templateId, SessionTrigger trigger)
        {
            var result = await httpClient.PostAsJsonAsync($"api/triggers/run", new RunTriggerRequest(templateId, trigger));
            return await OperationResult.FromResponseAsync(result);
        }
    }
}
