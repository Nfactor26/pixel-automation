using Microsoft.AspNetCore.Mvc;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Respository;
using System;
using System.Collections.Generic;
using System.Linq;
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

        // GET: api/TestSession
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestSession>>> Get()
        {
            var testSessions  = await testSessionRepository.GetTestSessionsAsync();
            if(testSessions != null)
            {
                return testSessions.ToList();
            }
            return NoContent();
        }

        // GET: api/TestSession/5
        [HttpGet("{sessionId}", Name = "Get")]
        public async Task<ActionResult<TestSession>> Get(string sessionId)
        {
            var  testSession = await testSessionRepository.GetTestSessionAsync(sessionId);
            if(testSession != null)
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
            return CreatedAtAction(nameof(Get), new { sessionId = testSession.SessionId }, testSession);
        }


        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string sessionId)
        {
            var testSession = await testSessionRepository.GetTestSessionAsync(sessionId);
            if(testSession == null)
            {
                return NotFound();
            }

            await testSessionRepository.DeleteTestSessionAsync(testSession.SessionId);
            return NoContent();
        }
    }
}
