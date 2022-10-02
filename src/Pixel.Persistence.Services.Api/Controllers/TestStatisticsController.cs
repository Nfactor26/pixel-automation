using Microsoft.AspNetCore.Mvc;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Respository;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{

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
