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
    public class FixturesController : ControllerBase
    {
        private readonly ILogger<FixturesController> logger;
        private readonly ITestFixtureRepository fixturesRepository;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="fixturesRepository"></param>
        public FixturesController(ILogger<FixturesController> logger, ITestFixtureRepository fixturesRepository)
        {
            this.logger = Guard.Argument(logger).NotNull().Value;
            this.fixturesRepository = Guard.Argument(fixturesRepository).NotNull().Value;
        }

        [HttpGet("{projectId}/{projectVersion}/id/{fixtureId}")]
        public async Task<ActionResult<TestFixture>> FindByIdAsync(string projectId, string projectVersion, string fixtureId)
        {
            try
            {
                Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
                Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
                Guard.Argument(fixtureId, nameof(fixtureId)).NotNull().NotEmpty();
                var result = await fixturesRepository.FindByIdAsync(projectId, projectVersion, fixtureId, CancellationToken.None) ??
                    throw new ArgumentException($"Fixture with Id : {fixtureId} doesn't exist");
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("{projectId}/{projectVersion}/name/{name}")]
        public async Task<ActionResult<TestFixture>> FindByNameAsync(string projectId, string projectVersion, string name)
        {
            try
            {
                Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
                Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
                Guard.Argument(name, nameof(name)).NotNull().NotEmpty();
                var result = await fixturesRepository.FindByNameAsync(projectId, projectVersion, name, CancellationToken.None) ??
                    throw new ArgumentException($"Fixture with name : {name} doesn't exist");
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }
        

        [HttpGet("{projectId}/{projectVersion}")]
        public async Task<ActionResult<List<TestFixture>>> GetAllForProjectAsync(string projectId, string projectVersion, [FromHeader] DateTime laterThan)
        {
            try
            {
                Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
                Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
                var result = await fixturesRepository.GetFixturesAsync(projectId, projectVersion, laterThan, CancellationToken.None) ?? Enumerable.Empty<TestFixture>();
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }


        [HttpPost("{projectId}/{projectVersion}")]
        public async Task<ActionResult<TestFixture>> AddFixtureAsync(string projectId, string projectVersion, [FromBody] TestFixture testFixture)
        {
            try
            {
                Guard.Argument(testFixture, nameof(testFixture)).NotNull();
                testFixture.ProjectId = Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
                testFixture.ProjectVersion = Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
                await fixturesRepository.AddFixtureAsync(projectId, projectVersion, testFixture, CancellationToken.None);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut("{projectId}/{projectVersion}")]
        public async Task<ActionResult<TestFixture>> UpdateFixtureAsync(string projectId, string projectVersion, [FromBody] TestFixture testFixture)
        {
            try
            {
                Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
                Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
                Guard.Argument(testFixture, nameof(testFixture)).NotNull();
                await fixturesRepository.UpdateFixtureAsync(projectId, projectVersion, testFixture, CancellationToken.None);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("{projectId}/{projectVersion}/{fixtureId}")]
        public async Task<ActionResult<TestFixture>> DeleteFixtureAsync(string projectId, string projectVersion, string fixtureId)
        {
            try
            {
                Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
                Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();             
                Guard.Argument(fixtureId, nameof(fixtureId)).NotNull().NotEmpty();               
                await fixturesRepository.DeleteFixtureAsync(projectId, projectVersion, fixtureId, CancellationToken.None);
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
