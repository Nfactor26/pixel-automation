using Dawn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pixel.Automation.Web.Portal.Helpers;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using Pixel.Persistence.Respository;
using Pixel.Persistence.Services.Api.Jobs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TriggersController : ControllerBase
    {
        private readonly ILogger logger;      
        private readonly ITemplateRepository templateRepository;
        private readonly IJobManager jobManager;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="schedulerFactory"></param>
        /// <param name="templateRepository"></param>
        /// <param name="jobManager"></param>
        public TriggersController(ILogger<TriggersController> logger, 
            ITemplateRepository templateRepository, IJobManager jobManager)
        {
            this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;           
            this.templateRepository = Guard.Argument(templateRepository, nameof(templateRepository)).NotNull().Value;   
            this.jobManager = Guard.Argument(jobManager, nameof(jobManager)).NotNull().Value;
        }

        /// <summary>
        /// Add a new trigger to template
        /// </summary>
        /// <param name="addTriggerRequest"></param>
        /// <returns></returns>
        [HttpPost("add")]
        public async Task<IActionResult> AddTriggerToTemplate([FromBody] AddTriggerRequest addTriggerRequest)
        {
            Guard.Argument(addTriggerRequest, nameof(addTriggerRequest)).NotNull();
            var template = await templateRepository.GetTemplateByIdAsync(addTriggerRequest.TemplateId);
            if (template != null)
            {
                if (template.Triggers.Any(a => a.Equals(addTriggerRequest.Trigger)))
                {
                    return BadRequest(new BadRequestResponse(new[] { "Trigger with same details already exists for template" }));
                }
                await templateRepository.AddTriggerAsync(template, addTriggerRequest.Trigger);
                logger.LogInformation("A new trigger @{0} was added to template {1}", addTriggerRequest.Trigger, template.Name);
                if (addTriggerRequest.Trigger is CronSessionTrigger cronSessionTrigger && cronSessionTrigger.IsEnabled)
                {
                    try
                    {
                        await jobManager.AddCronJobAsync(template, cronSessionTrigger);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occured while trying to schedule a new job for trigger : {0} beloning to template {1}", cronSessionTrigger.Name, template.Name);
                        return Problem($"An error occured while trying to schedule job for trigger. {ex.Message}");
                    }
                }
                return Ok();
            }
            return NotFound(new NotFoundResponse($"Template with Id: {addTriggerRequest.TemplateId} not found."));

        }

        /// <summary>
        /// Update details of an existing trigger
        /// </summary>
        /// <param name="updateTriggerRequest"></param>
        /// <returns></returns>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateTriggerForTemplate([FromBody] UpdateTriggerRequest updateTriggerRequest)
        {
            Guard.Argument(updateTriggerRequest, nameof(updateTriggerRequest)).NotNull();
            var template = await templateRepository.GetTemplateByIdAsync(updateTriggerRequest.TemplateId);
            if (template != null)
            {
                if (template.Triggers.Any(a => a.Equals(updateTriggerRequest.Original)))
                {
                    await templateRepository.DeleteTriggerAsync(template, updateTriggerRequest.Original);
                    await jobManager.DeleteTriggerAsync(template, updateTriggerRequest.Original);
                    await templateRepository.AddTriggerAsync(template, updateTriggerRequest.Updated);
                    if (updateTriggerRequest.Updated is CronSessionTrigger cronSessionTrigger)
                    {
                        await jobManager.AddCronJobAsync(template, cronSessionTrigger);
                    }
                    return Ok();
                }
                return NotFound(new NotFoundResponse($"No matching trigger was found to  update"));
            }
            return NotFound(new NotFoundResponse($"Template with Id: {updateTriggerRequest.TemplateId} not found."));
        }

        /// <summary>
        /// Delete an existing trigger
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="triggerName"></param>
        /// <returns></returns>
        [HttpDelete("delete/{templateId}/{triggerName}")]
        public async Task<IActionResult> DeleteTriggerFromTemplate(string templateId, string triggerName)
        {
            Guard.Argument(templateId, nameof(templateId)).NotNull().NotEmpty();
            Guard.Argument(triggerName, nameof(triggerName)).NotNull().NotEmpty();
            var template = await templateRepository.GetTemplateByIdAsync(templateId);
            if (template != null)
            {
                if (template.Triggers.Any(a => a.Name.Equals(triggerName)))
                {
                    var triggerToDelete = template.Triggers.First(t => t.Name.Equals(triggerName));
                    await templateRepository.DeleteTriggerAsync(template, triggerToDelete);
                    logger.LogInformation("Deleted trigger {0} from template {1}", triggerToDelete.Name, template.Name);
                    bool wasJobDeleted = await jobManager.DeleteTriggerAsync(template, triggerToDelete);
                    if (!wasJobDeleted)
                    {
                        return NotFound(new NotFoundResponse($"Trigger was deleted. However, job associated with trigger could not be deleted."));
                    }
                    return Ok();
                }
                return NotFound(new NotFoundResponse($"No matching trigger was found to  delete"));
            }
            return NotFound(new NotFoundResponse($"Template with Id: {templateId} not found."));
        }

        /// <summary>
        /// Run trigger on demand
        /// </summary>
        /// <param name="addTriggerRequest"></param>
        /// <returns></returns>
        [HttpPost("run")]
        public async Task<IActionResult> RunTriggerAsync([FromBody] RunTriggerRequest request)
        {
            try
            {
                Guard.Argument(request, nameof(request)).NotNull();
                var template = await templateRepository.GetTemplateByIdAsync(request.TemplateId);
                await jobManager.RunTriggerAsync(template, request.Trigger);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "There was an error while trying to run on-demand trigger");
                return Problem("Failed to run on-demand trigger");
            }
        }

        /// <summary>
        /// Get next fire time for a given trigger
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="triggerName"></param>
        /// <returns></returns>
        [HttpGet("next/{jobName}/{triggerName}")]
        public async Task<ActionResult> GetNextFireTimeUtcAsync(string jobName, string triggerName)
        {
            Guard.Argument(jobName, nameof(jobName)).NotNull().NotEmpty();
            Guard.Argument(triggerName, nameof(triggerName)).NotNull().NotEmpty();
            var nextFireTime = await jobManager.GetNextFireTimeUtcAsync(jobName, triggerName);
            return Ok(nextFireTime);
        }

        /// <summary>
        /// Pause a given trigger
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="triggerName"></param>
        /// <returns></returns>
        [HttpGet("pause/{jobName}/{triggerName}")]
        public async Task<ActionResult> PauseTriggerAsync(string jobName, string triggerName)
        {
            Guard.Argument(jobName, nameof(jobName)).NotNull().NotEmpty();
            Guard.Argument(triggerName, nameof(triggerName)).NotNull().NotEmpty();
            await jobManager.PauseTriggerAsync(jobName, triggerName);
            await templateRepository.PauseTriggerAsync(jobName, triggerName);
            return Ok();
        }

        /// <summary>
        /// Resume a given trigger
        /// </summary>
        /// <param name="jobName"></param>
        /// <param name="triggerName"></param>
        /// <returns></returns>
        [HttpGet("resume/{jobName}/{triggerName}")]
        public async Task<ActionResult> ResumeTriggerAsync(string jobName, string triggerName)
        {
            Guard.Argument(jobName, nameof(jobName)).NotNull().NotEmpty();
            Guard.Argument(triggerName, nameof(triggerName)).NotNull().NotEmpty();
            await jobManager.ResumeTriggerAsync(jobName, triggerName);
            await templateRepository.ResumeTriggerAsync(jobName, triggerName);
            return Ok();
        }

        /// <summary>
        /// Pause all the triggers for a given job
        /// </summary>
        /// <param name="jobName"></param>
        /// <returns></returns>
        [HttpGet("pause/{jobName}")]
        public async Task<ActionResult> PauseJobAsync(string jobName)
        {
            Guard.Argument(jobName, nameof(jobName)).NotNull().NotEmpty();           
            await jobManager.PauseJobAsync(jobName);
            await templateRepository.PauseTemplateAsync(jobName);
            return Ok();
        }

        /// <summary>
        /// Resume all the triggers for a given job
        /// </summary>
        /// <param name="jobName"></param>
        /// <returns></returns>
        [HttpGet("resume/{jobName}")]
        public async Task<ActionResult> ResumeJobAsync(string jobName)
        {
            Guard.Argument(jobName, nameof(jobName)).NotNull().NotEmpty();          
            await jobManager.ResumeJobAsync(jobName);
            await templateRepository.ResumeTemplateAsync(jobName);
            return Ok();
        }
    }
}
