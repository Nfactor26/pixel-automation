using Dawn;
using Microsoft.AspNetCore.Mvc;
using Pixel.Automation.Web.Portal.Helpers;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using Pixel.Persistence.Core.Response;
using Pixel.Persistence.Respository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemplatesController : ControllerBase
    {
        private readonly ITemplateRepository templateRepository;

        public TemplatesController(ITemplateRepository templateRepository)
        {
            this.templateRepository = templateRepository;
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
                    await templateRepository.AddTriggerAsync(template, updateTriggerRequest.Updated);
                    return Ok();
                }
                return NotFound(new NotFoundResponse($"No matching trigger was found to  update"));
            }
            return NotFound(new NotFoundResponse($"Template with Id: {updateTriggerRequest.TemplateId} not found."));
        }

        [HttpDelete("triggers/delete")]
        public async Task<IActionResult> DeleteTriggerFromTemplate( [FromBody] DeleteTriggerRequest deleteTriggerRequest)
        {          
            Guard.Argument(deleteTriggerRequest, nameof(deleteTriggerRequest)).NotNull();
            var template = await templateRepository.GetByIdAsync(deleteTriggerRequest.TemplateId);
            if (template != null)
            {
                if (template.Triggers.Any(a => a.Equals(deleteTriggerRequest.Trigger)))
                {
                    await templateRepository.DeleteTriggerAsync(template, deleteTriggerRequest.Trigger);
                    return Ok();
                }
                return NotFound(new NotFoundResponse($"No matching trigger was found to  delete"));
            }
            return NotFound(new NotFoundResponse($"Template with Id: {deleteTriggerRequest.TemplateId} not found."));
        }
    }
}
