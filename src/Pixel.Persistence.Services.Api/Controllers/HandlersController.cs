using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using Pixel.Persistence.Core.Response;
using Pixel.Persistence.Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HandlersController : ControllerBase
    {
        private readonly ILogger<HandlersController> logger;
        private readonly ITemplateHandlerRepository handlersRepository;
        
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="fixturesRepository"></param>
        public HandlersController(ILogger<HandlersController> logger, ITemplateHandlerRepository handlersRepository)
        {
            this.logger = Guard.Argument(logger).NotNull().Value;
            this.handlersRepository = Guard.Argument(handlersRepository).NotNull().Value;            
        }

        [HttpGet("name/{name}")]
        public async Task<ActionResult<TemplateHandler>> FindByNameAsync(string name)
        {
            try
            {               
                Guard.Argument(name, nameof(name)).NotNull().NotEmpty();
                var handler = await handlersRepository.FindByNameAsync(name, CancellationToken.None);               
                if(handler != null)
                {
                    return Ok(handler);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("id/{id}")]
        public async Task<TemplateHandler> FindByIdAsync(string id)
        {
            try
            {
                Guard.Argument(id, nameof(id)).NotNull().NotEmpty();
                var handler = await handlersRepository.FindByIdAsync(id, CancellationToken.None);
                return handler;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return null;
            }
        }

        [HttpGet]
        public async Task<IEnumerable<TemplateHandler>> GetAll()
        {
            var handlers = await handlersRepository.GetAllAsync(CancellationToken.None);
            return handlers;
        }

        [HttpGet("paged")]
        public async Task<PagedList<TemplateHandler>> Get([FromQuery] GetHandlersRequest queryParameter)
        {
            var handlers = await handlersRepository.GetHandlersAsync(queryParameter);
            return new PagedList<TemplateHandler>(handlers, handlers.Count(), queryParameter.CurrentPage, queryParameter.PageSize);
        }              

        [HttpPost]
        public async Task<ActionResult<TemplateHandler>> AddHandlerAsync(TemplateHandler handler)
        {
            try
            {
                Guard.Argument(handler, nameof(handler)).NotNull();             
                await handlersRepository.AddHandlerAsync(handler, CancellationToken.None);
                logger.LogInformation("Template handler {0} was added", handler.Name);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut]
        public async Task<ActionResult<TemplateHandler>> UpdateHandlerAsync(TemplateHandler handler)
        {
            try
            {
                Guard.Argument(handler, nameof(handler)).NotNull();
                await handlersRepository.UpdateHandlerAsync(handler, CancellationToken.None);
                logger.LogInformation("Template handler {0} was updated", handler.Name);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteHandlerAsync(string Id)
        {
            try
            {
                Guard.Argument(Id, nameof(Id)).NotNull().NotEmpty();
                await handlersRepository.DeleteHandlerAsync(Id, CancellationToken.None);
                logger.LogInformation("Template handler with Id : {0} was deleted", Id);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }        

    }
}
