using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Security;
using Pixel.Persistence.Respository;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{

    [Authorize(Policy = Policies.ReadTestDataPolicy)]
    [Route("api/[controller]")]
    [ApiController]
    public class TestStatisticsController : ControllerBase
    {

        private readonly ITestStatisticsRepository testStatisticsRepository;

        public TestStatisticsController(ITestStatisticsRepository testStatisticsRepository)
        {
            this.testStatisticsRepository = testStatisticsRepository;
        }

        // GET: api/TestStatistics/5
        [HttpGet("{testId}")]
        public async Task<ActionResult<TestStatistics>> Get(string testId)
        {
            var testStatistics = await testStatisticsRepository.GetTestStatisticsByIdAsync(testId);
            if (testStatistics != null)
            {
                return testStatistics;
            }
            return NotFound();
        }

        [HttpGet("process/{sessionId}")]
        [Authorize(Policy = Policies.WriteTestDataPolicy)]
        public ActionResult Post(string sessionId)
        {
            Task statsProcessor = new Task(async () =>
            {
                await this.testStatisticsRepository.AddOrUpdateStatisticsAsync(sessionId);
            });
            statsProcessor.Start();
            return Ok();
        }
    }
}
