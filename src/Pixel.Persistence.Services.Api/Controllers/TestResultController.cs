using Dawn;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using Pixel.Persistence.Core.Response;
using Pixel.Persistence.Core.Security;
using Pixel.Persistence.Respository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{
    [Authorize(Policy = Policies.ReadTestDataPolicy)]
    [Route("api/[controller]")]
    [ApiController]
    public class TestResultController : ControllerBase
    {

        private readonly ITestResultsRepository testResultsRepository;

        public TestResultController(ITestResultsRepository testResultsRepository)
        {
            this.testResultsRepository = testResultsRepository;
        }


        [HttpGet]      
        public async Task<PagedList<TestResult>> Get([FromQuery] TestResultRequest queryParameter)
        {
            var count = await testResultsRepository.GetCountAsync(queryParameter);
            var sessions = await testResultsRepository.GetTestResultsAsync(queryParameter);
            return new PagedList<TestResult>(sessions, (int)count, queryParameter.CurrentPage, queryParameter.PageSize);
        }


        // GET: api/TestSession/5
        [HttpGet("{sessionId}")]      
        public async Task<ActionResult<IEnumerable<TestResult>>> Get(string sessionId)
        {
            var results = await testResultsRepository.GetTestResultsAsync(sessionId);
            return results.ToList();
        }

        // POST: api/TestSession
        [HttpPost]
        [Authorize(Policy = Policies.WriteTestDataPolicy)]
        public async Task<IActionResult> Post([FromBody] TestResult testResult)
        {
            Guard.Argument(testResult).NotNull();
            await testResultsRepository.AddTestResultAsync(testResult);
            return Ok();
        }

        [HttpPut("failure/reason")]        
        public async Task<IActionResult> Update([FromBody] UpdateFailureReasonRequest request)
        {
            Guard.Argument(request).NotNull();
            await testResultsRepository.UpdateFailureReasonAsync(request);
            return Ok();
        }
    }
}
