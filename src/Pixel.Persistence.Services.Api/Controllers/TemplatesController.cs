using Dawn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pixel.Automation.Web.Portal.Helpers;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using Pixel.Persistence.Core.Response;
using Pixel.Persistence.Respository;
using Pixel.Persistence.Services.Api.Jobs;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemplatesController : ControllerBase
    {
        private readonly ILogger logger;
        private readonly ITemplateRepository templateRepository;       
        private readonly IJobManager jobManager;

        public TemplatesController(ILogger<TemplatesController> logger, ITemplateRepository templateRepository, 
           IJobManager jobManager)
        {
            this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;
            this.templateRepository = Guard.Argument(templateRepository, nameof(templateRepository)).NotNull().Value;     
            this.jobManager =  Guard.Argument(jobManager, nameof(jobManager)).NotNull().Value;
        }

        // GET: api/TestSession/5
        [HttpGet("id/{id}")]
        public async Task<ActionResult<SessionTemplate>> GetByIdAsync(string Id)
        {
           var result = await templateRepository.GetByIdAsync(Id);
            if (result != null)
            {
                return result;
            }
            return NotFound($"No template exists with Id : {Id}");
        }

        [HttpGet("name/{name}")]
        public async Task<ActionResult<SessionTemplate>> GetByNameAsync(string name)
        {
            var result = await templateRepository.GetByNameAsync(name);
            if(result != null)
            {
                return result;
            }
            return NotFound($"No template exists with name : {name}");
        }

        [HttpGet("paged")]
        public async Task<PagedList<SessionTemplate>> Get([FromQuery] GetTemplatesRequest queryParameter)
        {
            var sessions = await templateRepository.GetTemplatesAsync(queryParameter);      
            return new PagedList<SessionTemplate>(sessions, sessions.Count(), queryParameter.CurrentPage, queryParameter.PageSize);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SessionTemplate>>> GetAllAsync()
        {
            var result = await templateRepository.GetAllAsync();
            return result.ToList();
        }

        [HttpPost]        
        public async Task<IActionResult> Create([FromBody] SessionTemplate template)
        {
            Guard.Argument(template).NotNull();
            var exists = await templateRepository.GetByNameAsync(template.Name);
            if(exists != null)
            {
                return BadRequest($"A template already exists with name : {template.Name}");
            }
            await templateRepository.CreateAsync(template);
            return Ok();
        }

        [HttpPut]      
        public async Task<IActionResult> Update([FromBody] SessionTemplate template)
        {
            Guard.Argument(template).NotNull();
            var existing = await templateRepository.GetByIdAsync(template.Id);
            if(existing == null)
            {
                return BadRequest($"Template with Id : {template.Id} doesn't exist");
            }           
            if(!existing.Name.Equals(template.Name))
            {
                var anotherWithName = await templateRepository.GetByNameAsync(template.Name);
                if (anotherWithName != null)
                {
                    return BadRequest($"A template already exists with name : {template.Name}. Name can't be duplicate.");
                }

            }
            await templateRepository.UpdateAsync(template);
            return Ok();
        }

        [HttpDelete]      
        public async Task<IActionResult> Delete(string Id)
        {
           if(await templateRepository.TryDeleteAsync(Id))
           {
                return Ok();
           }
           return BadRequest($"Template with Id : {Id} doesn't exist");
        }


        [HttpPost("triggers/add")]
        public async Task<IActionResult> AddTriggerToTemplate([FromBody] AddTriggerRequest addTriggerRequest)
        {           
            Guard.Argument(addTriggerRequest, nameof(addTriggerRequest)).NotNull();
            var template = await templateRepository.GetByIdAsync(addTriggerRequest.TemplateId);
            if(template != null)
            {
                if (template.Triggers.Any(a => a.Equals(addTriggerRequest.Trigger)))
                {
                    return BadRequest(new BadRequestResponse(new[] { "Trigger with same details already exists for template" }));
                }
                await templateRepository.AddTriggerAsync(template, addTriggerRequest.Trigger);
                logger.LogInformation("A new trigger @{0} was added to template {1}", addTriggerRequest.Trigger, template.Name);
                if(addTriggerRequest.Trigger is CronSessionTrigger cronSessionTrigger && cronSessionTrigger.IsEnabled)
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

        [HttpPut("triggers/update")]
        public async Task<IActionResult> UpdateTriggerForTemplate([FromBody] UpdateTriggerRequest updateTriggerRequest)
        {        
            Guard.Argument(updateTriggerRequest, nameof(updateTriggerRequest)).NotNull();           
            var template = await templateRepository.GetByIdAsync(updateTriggerRequest.TemplateId);
            if (template != null)
            {
                if (template.Triggers.Any(a => a.Equals(updateTriggerRequest.Original)))
                {
                    await templateRepository.DeleteTriggerAsync(template, updateTriggerRequest.Original);
                    await jobManager.DeleteTriggerAsync(template, updateTriggerRequest.Original);
                    await templateRepository.AddTriggerAsync(template, updateTriggerRequest.Updated);
                    if(updateTriggerRequest.Updated is CronSessionTrigger cronSessionTrigger)
                    {
                        await jobManager.AddCronJobAsync(template, cronSessionTrigger);
                    }
                    return Ok();
                }
                return NotFound(new NotFoundResponse($"No matching trigger was found to  update"));
            }
            return NotFound(new NotFoundResponse($"Template with Id: {updateTriggerRequest.TemplateId} not found."));
        }

        [HttpDelete("triggers/delete/{templateId}/{triggerName}")]
        public async Task<IActionResult> DeleteTriggerFromTemplate(string templateId, string triggerName)
        {          
            Guard.Argument(templateId, nameof(templateId)).NotNull().NotEmpty();
            Guard.Argument(triggerName, nameof(triggerName)).NotNull().NotEmpty();
            var template = await templateRepository.GetByIdAsync(templateId);
            if (template != null)
            {
                if (template.Triggers.Any(a => a.Name.Equals(triggerName)))
                {
                    var triggerToDelete = template.Triggers.First(t => t.Name.Equals(triggerName));
                    await templateRepository.DeleteTriggerAsync(template, triggerToDelete);
                    logger.LogInformation("Deleted trigger {0} from template {1}", triggerToDelete.Name, template.Name);
                    bool wasJobDeleted = await jobManager.DeleteTriggerAsync(template, triggerToDelete);
                    if(!wasJobDeleted)
                    {                     
                        return NotFound(new NotFoundResponse($"Trigger was deleted. However, job associated with trigger could not be deleted."));
                    }                                          
                    return Ok();
                }
                return NotFound(new NotFoundResponse($"No matching trigger was found to  delete"));
            }
            return NotFound(new NotFoundResponse($"Template with Id: {templateId} not found."));
        }
    }
}
