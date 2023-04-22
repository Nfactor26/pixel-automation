using Pixel.Automation.Web.Portal.Helpers;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
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
            var result = await httpClient.PostAsJsonAsync<AddTriggerRequest>($"api/templates/triggers/add", new AddTriggerRequest(templateId, trigger));
            return await OperationResult.FromResponseAsync(result);
        }

        /// <inheritdoc/>
        public async Task<OperationResult> DeleteTriggerAsync(string templateId, SessionTrigger trigger)
        {
            var result = await httpClient.PostAsJsonAsync<DeleteTriggerRequest>($"api/templates/triggers/delete", new DeleteTriggerRequest(templateId, trigger));
            return await OperationResult.FromResponseAsync(result);
        }

        /// <inheritdoc/>
        public async Task<OperationResult> UpdateTriggerAsync(string templateId, SessionTrigger original, SessionTrigger modified)
        {            
            var result = await httpClient.PutAsJsonAsync<UpdateTriggerRequest>($"api/templates/triggers/update", new UpdateTriggerRequest(templateId, original, modified));
            return await OperationResult.FromResponseAsync(result);
        }
    }
}
