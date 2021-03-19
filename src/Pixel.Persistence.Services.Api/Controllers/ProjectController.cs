using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Respository;
using Pixel.Persistence.Services.Api.Extensions;
using System.IO;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectRepository projectRepository;

        public ProjectController(IProjectRepository projectRepository)
        {
            this.projectRepository = projectRepository;
        }

        /// <summary>
        /// Get the project file (.atm) given it's project id
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpGet("{projectId}")]
        public async Task<ActionResult> Get(string projectId)
        {
            var bytes = await projectRepository.GetProjectFile(projectId);
            return File(bytes, "application/octet-stream", projectId);
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
            var bytes = await projectRepository.GetProjectDataFiles(projectId, version);
            return File(bytes, "application/octet-stream", projectId);
        }

        /// <summary>
        /// Add or update project file (.atm) or all the files (process, test cases, scripts, etc) for a specific version of a project as a zipped file
        /// </summary>
        /// <param name="projectDescription"></param>
        /// <param name="projectFile"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody][ModelBinder(typeof(JsonModelBinder), Name = nameof(ProjectMetaData))] ProjectMetaData projectDescription, [FromForm(Name = "file")] IFormFile projectFile)
        {
            using (var ms = new MemoryStream())
            {
                projectFile.CopyTo(ms);
                var fileBytes = ms.ToArray();
                await projectRepository.AddOrUpdateProject(projectDescription, projectFile.FileName, fileBytes);
            }

            return CreatedAtAction(nameof(Get), new { projectId = projectDescription.ProjectId, version = projectDescription.Version }, projectDescription);
        }
    }
}