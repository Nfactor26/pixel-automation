using Microsoft.AspNetCore.Mvc;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using Pixel.Persistence.Core.Response;
using Pixel.Persistence.Respository;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestSessionController : ControllerBase
    {

        private readonly ITestSessionRepository testSessionRepository;

        public TestSessionController(ITestSessionRepository testSessionRepository)
        {
            this.testSessionRepository = testSessionRepository;
        }
        
        [HttpGet]       
        public async Task<PagedList<TestSession>> Get([FromQuery] TestSessionRequest queryParameter)
        {
            var count = await testSessionRepository.GetCountAsync(queryParameter);
            var sessions = await testSessionRepository.GetTestSessionsAsync(queryParameter);           
            return new PagedList<TestSession>(sessions, (int)count, queryParameter.CurrentPage, queryParameter.PageSize);
        }

        // GET: api/TestSession/5
        [HttpGet("{sessionId}")]  
        public async Task<ActionResult<TestSession>> Get(string sessionId)
        {
            var testSession = await testSessionRepository.GetTestSessionAsync(sessionId);
            if (testSession != null)
            {
                return testSession;
            }

            return NotFound();
        }             
       
        // POST: api/TestSession
        [HttpPost]      
        public async Task<IActionResult> Post([FromBody] TestSession testSession)
        {
            await testSessionRepository.AddTestSessionAsync(testSession);
            return CreatedAtAction(nameof(Get), new { sessionId = testSession.Id }, testSession);
        }

        /// <summary>
        /// Update a TestSession with a given SessionId
        /// </summary>
        /// <param name="sessionId">SessionId of the TestSession to be updated</param>
        /// <param name="testSession">Updated data for the TestSession</param>
        /// <returns></returns>
        [HttpPost("{sessionId}")]        
        public async Task<IActionResult> Post([FromRoute] string sessionId, [FromBody] TestSession testSession)
        {
            await testSessionRepository.UpdateTestSessionAsync(sessionId, testSession);
            return CreatedAtAction(nameof(Get), new { sessionId = testSession.Id }, testSession);
        }

        // DELETE: api/TestSession/sessionId
        [HttpDelete("{id}")]       
        public async Task<IActionResult> Delete(string sessionId)
        {
            bool isDeleted = await testSessionRepository.TryDeleteTestSessionAsync(sessionId);
            if (isDeleted)
            {
                return Ok();
            }
            return NotFound();
        }
    }
}
