using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Security;
using Pixel.Persistence.Respository;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{   
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectStatisticsController : ControllerBase
    {
        private readonly IProjectStatisticsRepository repository;

        public ProjectStatisticsController(IProjectStatisticsRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet("{projectId}")]
        public async Task<ActionResult<ProjectStatistics>> Get(string projectId)
        {
            var projectStatistics = await this.repository.GetProjectStatisticsByIdAsync(projectId);
            if (projectStatistics != null)
            {
                return projectStatistics;
            }
            return NotFound();
        }      

        [HttpPost("process/{sessionId}")]
        public ActionResult Post(string sessionId)
        {
            Task statsProcessor = new Task(async () =>
            {
                await this.repository.AddOrUpdateStatisticsAsync(sessionId);
            });
            statsProcessor.Start();
            return Ok();
        }

    }
}
