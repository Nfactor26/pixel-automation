using Dawn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Respository.Interfaces;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReferencesController : ControllerBase
    {
        private readonly ILogger<ReferencesController> logger;
        private readonly IReferencesRepository referencesRepository;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="projectRepository"></param>
        public ReferencesController(ILogger<ReferencesController> logger, IReferencesRepository referencesRepository)
        {
            this.logger = Guard.Argument(logger).NotNull().Value;
            this.referencesRepository = Guard.Argument(referencesRepository).NotNull().Value;
        }

        [HttpGet("{projectId}/{projectVersion}")]
        public async Task<ProjectReferences> GetProjectReferences(string projectId, string projectVersion)
        {
            return await this.referencesRepository.GetProjectReferences(projectId, projectVersion);
        }

        [HttpPost("{projectId}/{projectVersion}")]
        public async Task<IActionResult> AddProjectReferences(string projectId, string projectVersion, [FromBody] ProjectReferences projectReferences)
        {
            await this.referencesRepository.AddProjectReferences(projectId, projectVersion, projectReferences);
            return Ok();
        }

        [HttpPost("controls/{projectId}/{projectVersion}")]
        public async Task<IActionResult> AddOrUpdateControlReference(string projectId, string projectVersion, [FromBody] ControlReference controlReference)
        {
            await this.referencesRepository.AddOrUpdateControlReference(projectId, projectVersion, controlReference);
            return Ok();
        }


        [HttpPost("prefabs/{projectId}/{projectVersion}")]
        public async Task<IActionResult> AddOrUpdatePrefabReference(string projectId, string projectVersion, [FromBody] PrefabReference prefabReference)
        {
            await this.referencesRepository.AddOrUpdatePrefabReference(projectId, projectVersion, prefabReference);
            return Ok();
        }

        [HttpPost("editors/{projectId}/{projectVersion}")]
        public async Task<IActionResult> SetEditorReference(string projectId, string projectVersion, [FromBody] EditorReferences editorReferences)
        {
            await this.referencesRepository.SetEditorReferences(projectId, projectVersion, editorReferences);
            return Ok();
        }
    }
}
