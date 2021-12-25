using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Security;
using Pixel.Persistence.Respository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{
    [Authorize(Policy = Policies.ReadProcessDataPolicy)]
    [Route("api/[controller]")]
    [ApiController]
    public class MetaDataController : ControllerBase
    {

        private readonly IApplicationRepository applicationRepository;
        private readonly IControlRepository controlRepository;
        private readonly IProjectRepository projectRepository;
        private readonly IPrefabRepository prefabRepository;

        public MetaDataController(IApplicationRepository applicationRepository, IControlRepository controlRepository,
            IProjectRepository projectRepository, IPrefabRepository prefabRepository)
        {
            this.applicationRepository = applicationRepository;
            this.controlRepository = controlRepository;
            this.projectRepository = projectRepository;
            this.prefabRepository = prefabRepository;
        }


        [Route("application")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationMetaData>>> GetMetaData()
        {
            List<ApplicationMetaData> applications = new List<ApplicationMetaData>();
            await foreach (var applicationMetadata in applicationRepository.GetMetadataAsync())
            {
                applications.Add(applicationMetadata);
                
                //Get the metadata for controls belonging to application
                List<ControlMetaData> controlsMetaData = new List<ControlMetaData>();
                await foreach (var controlMetaData in this.controlRepository.GetMetadataAsync(applicationMetadata.ApplicationId))
                {
                    controlsMetaData.Add(controlMetaData);
                }
                applicationMetadata.ControlsMeta = controlsMetaData;

                //Get the metadata for prefabs belonging to application
                List<PrefabMetaDataCompact> prefabsMetaData = new List<PrefabMetaDataCompact>();
                await foreach (var prefabMetaData in this.prefabRepository.GetPrefabsMetadataForApplicationAsync(applicationMetadata.ApplicationId))
                {
                    prefabsMetaData.Add(prefabMetaData);
                }
                applicationMetadata.PrefabsMeta = prefabsMetaData;
            }
            return applications;
        }

        [Route("project")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectMetaData>>> GetProjectsMetaData()
        {
            List<ProjectMetaData> projects = new List<ProjectMetaData>();
            await foreach (var projectMetaData in projectRepository.GetProjectsMetadataAsync())
            {
                projects.Add(projectMetaData);
            }
            return projects;
        }


        [Route("project/{projectId}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectMetaData>>> GetProjectMetaData(string projectId)
        {
            List<ProjectMetaData> projects = new List<ProjectMetaData>();
            await foreach (var projectMetaData in projectRepository.GetProjectMetadataAsync(projectId))
            {
                projects.Add(projectMetaData);             
            }
            return projects;
        }
    }
}