using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Respository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestsController : ControllerBase
    {
        private readonly ILogger<TestsController> logger;
        private readonly ITestCaseRepository testsRespository;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="fixturesRepository"></param>
        public TestsController(ILogger<TestsController> logger, ITestCaseRepository testsRespository)
        {
            this.logger = Guard.Argument(logger).NotNull().Value;
            this.testsRespository = Guard.Argument(testsRespository).NotNull().Value;
        }

        [HttpGet("{projectId}/{projectVersion}/id/{testId}")]
        public async Task<ActionResult<TestCase>> FindByIdAsync(string projectId, string projectVersion, string testId)
        {
            try
            {
                Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
                Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
                Guard.Argument(testId, nameof(testId)).NotNull().NotEmpty();
                var result = await testsRespository.FindByIdAsync(projectId, projectVersion, testId, CancellationToken.None) ??
                    throw new ArgumentException($"TestCase with Id : {testId} doesn't exist");
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("{projectId}/{projectVersion}/name/{name}")]
        public async Task<ActionResult<TestCase>> FindByNameAsync(string projectId, string projectVersion, string name)
        {
            try
            {
                Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
                Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
                Guard.Argument(name, nameof(name)).NotNull().NotEmpty();
                var result = await testsRespository.FindByNameAsync(projectId, projectVersion, name, CancellationToken.None) ??
                    throw new ArgumentException($"TestCase with name : {name} doesn't exist");
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("{projectId}/{projectVersion}")]
        public async Task<ActionResult<List<TestCase>>> GetAllForProjectAsync(string projectId, string projectVersion, [FromHeader] DateTime laterThan)
        {
            try
            {
                Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
                Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
                var result = await testsRespository.GetTestCasesAsync(projectId, projectVersion, laterThan, CancellationToken.None) ?? Enumerable.Empty<TestCase>();
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("{projectId}/{projectVersion}/{fixtureId}")]
        public async Task<ActionResult<List<TestCase>>> GetAllForFixtureAsync(string projectId, string projectVersion, string fixtureId, [FromHeader] DateTime laterThan)
        {
            try
            {
                Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
                Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
                Guard.Argument(projectVersion, nameof(fixtureId)).NotNull().NotEmpty();
                var result = await testsRespository.GetTestCasesAsync(projectId, projectVersion, fixtureId, laterThan, CancellationToken.None) ?? Enumerable.Empty<TestCase>();
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("{projectId}/{projectVersion}")]
        public async Task<ActionResult<TestCase>> AddTestCaseAsync(string projectId, string projectVersion, [FromBody] TestCase testCase)
        {
            try
            {
                Guard.Argument(testCase, nameof(testCase)).NotNull();
                testCase.ProjectId = Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
                testCase.ProjectVersion = Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
                await testsRespository.AddTestCaseAsync(projectId, projectVersion, testCase, CancellationToken.None);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut("{projectId}/{projectVersion}")]
        public async Task<ActionResult<TestCase>> UpdateTestCaseAsync(string projectId, string projectVersion, [FromBody] TestCase testCase)
        {
            try
            {
                Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
                Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
                Guard.Argument(testCase, nameof(testCase)).NotNull();
                await testsRespository.UpdateTestCaseAsync(projectId, projectVersion, testCase, CancellationToken.None);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("{projectId}/{projectVersion}/{testCaseId}")]
        public async Task<ActionResult<TestCase>> DeleteTestCaseAsync(string projectId, string projectVersion, string testCaseId)
        {
            try
            {
                Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
                Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
                Guard.Argument(testCaseId, nameof(testCaseId)).NotNull().NotEmpty();              
                await testsRespository.DeleteTestCaseAsync(projectId, projectVersion, testCaseId, CancellationToken.None);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

    }
}
