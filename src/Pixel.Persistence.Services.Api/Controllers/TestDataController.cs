using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pixel.Persistence.Respository.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using Pixel.Persistence.Core.Models;

namespace Pixel.Persistence.Services.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestDataController : ControllerBase
    {
        private readonly ILogger<TestDataController> logger;
        private readonly ITestDataRepository testDataRepository;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="fixturesRepository"></param>
        public TestDataController(ILogger<TestDataController> logger, ITestDataRepository testDataRepository)
        {
            this.logger = Guard.Argument(logger).NotNull().Value;
            this.testDataRepository = Guard.Argument(testDataRepository).NotNull().Value;
        }

        [HttpGet("{projectId}/{projectVersion}/id/{dataSourceId}")]
        public async Task<ActionResult<TestDataSource>> FindByIdAsync(string projectId, string projectVersion, string dataSourceId)
        {
            try
            {
                Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
                Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
                Guard.Argument(dataSourceId, nameof(dataSourceId)).NotNull().NotEmpty();
                var result = await testDataRepository.FindByIdAsync(projectId, projectVersion, dataSourceId, CancellationToken.None) ??
                    throw new ArgumentException($"Test Data Source with Id : {dataSourceId} doesn't exist");
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("{projectId}/{projectVersion}/name/{name}")]
        public async Task<ActionResult<TestDataSource>> FindByNameAsync(string projectId, string projectVersion, string name)
        {
            try
            {
                Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
                Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
                Guard.Argument(name, nameof(name)).NotNull().NotEmpty();
                var result = await testDataRepository.FindByNameAsync(projectId, projectVersion, name, CancellationToken.None) ??
                    throw new ArgumentException($"Test Data Source with name : {name} doesn't exist");
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("{projectId}/{projectVersion}")]
        public async Task<ActionResult<List<TestDataSource>>> GetAllForProjectAsync(string projectId, string projectVersion)
        {
            try
            {
                Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
                Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
                var result = await testDataRepository.GetDataSourcesAsync(projectId, projectVersion, CancellationToken.None) ?? Enumerable.Empty<TestDataSource>();
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("{projectId}/{projectVersion}")]
        public async Task<ActionResult<TestDataSource>> AddDataSourceAsync(string projectId, string projectVersion, [FromBody] TestDataSource dataSource)
        {
            try
            {
                Guard.Argument(dataSource, nameof(dataSource)).NotNull();
                dataSource.ProjectId = Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
                dataSource.ProjectVersion = Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
                await testDataRepository.AddDataSourceAsync(projectId, projectVersion, dataSource, CancellationToken.None);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut("{projectId}/{projectVersion}")]
        public async Task<ActionResult<TestDataSource>> UpdateDataSourceAsync(string projectId, string projectVersion, [FromBody] TestDataSource dataSource)
        {
            try
            {
                Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
                Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
                Guard.Argument(dataSource, nameof(dataSource)).NotNull();
                await testDataRepository.UpdateDataSourceAsync(projectId, projectVersion, dataSource, CancellationToken.None);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("{projectId}/{projectVersion}/{dataSourceId}")]
        public async Task<ActionResult<TestCase>> DeleteDataSourceAsync(string projectId, string projectVersion, string dataSourceId)
        {
            try
            {
                Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
                Guard.Argument(projectVersion, nameof(projectVersion)).NotNull().NotEmpty();
                Guard.Argument(dataSourceId, nameof(dataSourceId)).NotNull().NotEmpty();
                await testDataRepository.DeleteDataSourceAsync(projectId, projectVersion, dataSourceId, CancellationToken.None);
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
