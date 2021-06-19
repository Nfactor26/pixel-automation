using Microsoft.AspNetCore.Mvc;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Respository;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        [HttpGet("recent/failures/{projectId}")]
        public async Task<ActionResult<IEnumerable<TestStatistics>>> GetRecentFailures(string projectId)
        {
            var recentFailures = await this.repository.GetRecentFailures(projectId);
            if (recentFailures != null)
            {
                return recentFailures.ToList();
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
