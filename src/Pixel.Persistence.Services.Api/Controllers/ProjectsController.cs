using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
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
    public class ProjectsController : ControllerBase
    {
        private readonly ILogger<ProjectsController> logger;
        private readonly IProjectsRepository projectRepository;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="projectRepository"></param>
        public ProjectsController(ILogger<ProjectsController> logger, IProjectsRepository projectRepository)
        {
            this.logger = Guard.Argument(logger).NotNull().Value;
            this.projectRepository = Guard.Argument(projectRepository).NotNull().Value;
        }

        [HttpGet("id/{projectId}")]
        public async Task<ActionResult<AutomationProject>> FindByIdAsync(string projectId)
        {
            try
            {
                Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
                var result =  await projectRepository.FindByIdAsync(projectId, CancellationToken.None) ?? 
                    throw new ArgumentException($"Automation project with Id : {projectId} doesn't exist");
                return Ok(result);
            }           
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("name/{name}")]
        public async Task<ActionResult<AutomationProject>> FindByNameAsync(string name)
        {
            try
            {
                Guard.Argument(name, nameof(name)).NotNull().NotEmpty();
                var result = await projectRepository.FindByNameAsync(name, CancellationToken.None) ??
                    throw new ArgumentException($"Automation project with name : {name} doesn't exist");
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet()]
        public async Task<ActionResult<IEnumerable<AutomationProject>>> GetAllAsync()
        {
            try
            {               
                var result = await projectRepository.FindAllAsync(CancellationToken.None) ?? Enumerable.Empty<AutomationProject>();
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }


        [HttpPost]
        public async Task<ActionResult<AutomationProject>> AddProjectAsync([FromBody] AutomationProject automationProject)
        {          
            try
            {
                Guard.Argument(automationProject, nameof(automationProject)).NotNull();
                await projectRepository.AddProjectAsync(automationProject, CancellationToken.None);
                return Ok(automationProject);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }


        [HttpPost("{projectId}/versions")]
        public async Task<ActionResult<VersionInfo>> AddProjectVersionAsync([FromRoute]string projectId, [FromBody] AddProjectVersionRequest request)
        {
            try
            {
                Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
                Guard.Argument(request, nameof(request)).NotNull();
                Guard.Argument(request, nameof(request)).Require(r => r.NewVersion != null, r => "NewVersion is required");
                Guard.Argument(request, nameof(request)).Require(r => r.CloneFrom != null, r => "CloneFrom is required");
                await projectRepository.AddProjectVersionAsync(projectId, request.NewVersion, request.CloneFrom, CancellationToken.None);
                return Ok(request.NewVersion);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }


        [HttpPut("{projectId}/versions")]
        public async Task<ActionResult<VersionInfo>> UpdateProjectVersionAsync([FromRoute] string projectId, [FromBody] VersionInfo projectVersion)
        {
            try
            {
                Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
                Guard.Argument(projectVersion, nameof(projectVersion)).NotNull();
                await projectRepository.UpdateProjectVersionAsync(projectId, projectVersion, CancellationToken.None);
                return Ok(projectVersion);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
