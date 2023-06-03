using Dawn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger logger;
        private readonly ITemplateRepository templateRepository;       
      
        public TemplatesController(ILogger<TemplatesController> logger, ITemplateRepository templateRepository)
        {
            this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;
            this.templateRepository = Guard.Argument(templateRepository, nameof(templateRepository)).NotNull().Value;            
        }

        // GET: api/TestSession/5
        [HttpGet("id/{id}")]
        public async Task<ActionResult<SessionTemplate>> GetByIdAsync(string Id)
        {
           var result = await templateRepository.GetTemplateByIdAsync(Id);
            if (result != null)
            {
                return result;
            }
            return NotFound($"No template exists with Id : {Id}");
        }

        [HttpGet("name/{name}")]
        public async Task<ActionResult<SessionTemplate>> GetByNameAsync(string name)
        {
            var result = await templateRepository.GetTemplateByNameAsync(name);
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
            var result = await templateRepository.GetAllTemplatesAsync();
            return result.ToList();
        }

        [HttpPost]        
        public async Task<IActionResult> Create([FromBody] SessionTemplate template)
        {
            Guard.Argument(template).NotNull();
            var exists = await templateRepository.GetTemplateByNameAsync(template.Name);
            if(exists != null)
            {
                return BadRequest($"A template already exists with name : {template.Name}");
            }
            await templateRepository.CreateTemplateAsync(template);
            return Ok();
        }

        [HttpPut]      
        public async Task<IActionResult> Update([FromBody] SessionTemplate template)
        {
            Guard.Argument(template).NotNull();
            var existing = await templateRepository.GetTemplateByIdAsync(template.Id);
            if(existing == null)
            {
                return BadRequest($"Template with Id : {template.Id} doesn't exist");
            }           
            if(!existing.Name.Equals(template.Name))
            {
                var anotherWithName = await templateRepository.GetTemplateByNameAsync(template.Name);
                if (anotherWithName != null)
                {
                    return BadRequest($"A template already exists with name : {template.Name}. Name can't be duplicate.");
                }

            }
            await templateRepository.UpdateTemplateAsync(template);
            return Ok();
        }

        [HttpDelete]      
        public async Task<IActionResult> Delete(string Id)
        {
           if(await templateRepository.TryDeleteTemplateAsync(Id))
           {
                return Ok();
           }
           return BadRequest($"Template with Id : {Id} doesn't exist");
        }        
    }
}
