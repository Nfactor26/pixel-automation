using Dawn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Respository;
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
        private readonly IControlRepository controlsRepository;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="projectRepository"></param>
        public ReferencesController(ILogger<ReferencesController> logger, IReferencesRepository referencesRepository, IControlRepository controlsRepository)
        {
            this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;
            this.referencesRepository = Guard.Argument(referencesRepository, nameof(referencesRepository)).NotNull().Value;
            this.controlsRepository = Guard.Argument(controlsRepository, nameof(controlsRepository)).NotNull().Value;
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
            var hasExistingReference = await this.referencesRepository.HasControlReference(projectId, projectVersion, controlReference);
            if(hasExistingReference)
            {
                await this.referencesRepository.UpdateControlReference(projectId, projectVersion, controlReference);
                return Ok();
            }

            if(await this.controlsRepository.IsControlDeleted(controlReference.ApplicationId, controlReference.ControlId))
            {
                logger.LogWarning("Can't add control reference to project : {0}, version : {1}. Control {@2} is marked deleted.", projectId, projectVersion, controlReference);
                return Conflict("Can't add control reference. Control is marked deleted.");
            }
            await this.referencesRepository.AddControlReference(projectId, projectVersion, controlReference);
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
