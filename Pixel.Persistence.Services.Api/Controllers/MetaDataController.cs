using Microsoft.AspNetCore.Mvc;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Respository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetaDataController : ControllerBase
    {

        private readonly IApplicationRepository applicationRepository;
        private readonly IControlRepository controlRepository;
        private readonly IProjectRepository projectRepository;

        public MetaDataController(IApplicationRepository applicationRepository, IControlRepository controlRepository, IProjectRepository projectRepository)
        {
            this.applicationRepository = applicationRepository;
            this.controlRepository = controlRepository;
            this.projectRepository = projectRepository;
        }


        [Route("application")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationMetaData>>> GetMetaData()
        {
            List<ApplicationMetaData> applications = new List<ApplicationMetaData>();
            await foreach (var applicationMetadata in applicationRepository.GetMetadataAsync())
            {
                applications.Add(applicationMetadata);
                List<ControlMetaData> controlMetaDatas = new List<ControlMetaData>();
                await foreach (var controlMetaData in this.controlRepository.GetMetadataAsync(applicationMetadata.ApplicationId))
                {
                    controlMetaDatas.Add(controlMetaData);
                }
                applicationMetadata.ControlsMeta = controlMetaDatas;
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