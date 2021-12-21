using Dawn;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Respository;
using Pixel.Persistence.Services.Api.Extensions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly ILogger<ProjectController> logger;
        private readonly IProjectRepository projectRepository;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="projectRepository"></param>
        public ProjectController(ILogger<ProjectController> logger, IProjectRepository projectRepository)
        {            
            this.logger = Guard.Argument(logger).NotNull().Value;
            this.projectRepository = Guard.Argument(projectRepository).NotNull().Value;
        }

        /// <summary>
        /// Get the project file (.atm) given it's project id
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpGet("{projectId}")]
        public async Task<ActionResult> Get(string projectId)
        {
            try
            {
                var bytes = await projectRepository.GetProjectFile(projectId);
                return File(bytes, "application/octet-stream", projectId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get all the files (process, test cases, scripts, etc) for a specific version of a project as a zipped file
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        [HttpGet("{projectId}/{version}")]
        public async Task<ActionResult> Get(string projectId, string version)
        {
            try
            {
                var bytes = await projectRepository.GetProjectDataFiles(projectId, version);
                return File(bytes, "application/octet-stream", projectId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Add or update project file (.atm) or all the files (process, test cases, scripts, etc) for a specific version of a project as a zipped file
        /// </summary>
        /// <param name="projectDescription"></param>
        /// <param name="projectFile"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody][ModelBinder(typeof(JsonModelBinder), Name = nameof(ProjectMetaData))] ProjectMetaData projectDescription, [FromForm(Name = "file")] IFormFile projectFile)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    projectFile.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    await projectRepository.AddOrUpdateProject(projectDescription, projectFile.FileName, fileBytes);
                }

                return CreatedAtAction(nameof(Get), new { projectId = projectDescription.ProjectId, version = projectDescription.Version }, projectDescription);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}