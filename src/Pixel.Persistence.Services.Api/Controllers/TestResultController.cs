using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using Pixel.Persistence.Core.Response;
using Pixel.Persistence.Respository;
using Pixel.Persistence.Services.Api.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestResultController : ControllerBase
    {
        private readonly ILogger<TestResultController> logger;
        private readonly ITestResultsRepository testResultsRepository;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="testResultsRepository"></param>
        public TestResultController(ILogger<TestResultController> logger, ITestResultsRepository testResultsRepository)
        {
            this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;
            this.testResultsRepository = Guard.Argument(testResultsRepository, nameof(testResultsRepository)).NotNull().Value;
        }

        /// <summary>
        /// Get all the TestResult matching the request filter
        /// </summary>
        /// <param name="queryParameter"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<PagedList<TestResult>> Get([FromQuery] TestResultRequest queryParameter)
        {
            var count = await testResultsRepository.GetCountAsync(queryParameter);
            var sessions = await testResultsRepository.GetTestResultsAsync(queryParameter);
            return new PagedList<TestResult>(sessions, (int)count, queryParameter.CurrentPage, queryParameter.PageSize);
        }

        /// <summary>
        /// Get TestResult with a given identifier
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<TestResult> GetById(string id)
        {
            return await testResultsRepository.GetTestResultAsync(id);           ;
        }

        /// <summary>
        /// Get all the TestResult belonging to a given session 
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        [HttpGet("session/{sessionId}")]
        public async Task<ActionResult<IEnumerable<TestResult>>> GetAllForSession(string sessionId)
        {
            var results = await testResultsRepository.GetTestResultsForSessionAsync(sessionId);
            return results.ToList();
        }

        /// <summary>
        /// Add a new TestResult 
        /// </summary>
        /// <param name="testResult"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TestResult testResult)
        {
            Guard.Argument(testResult).NotNull();
            await testResultsRepository.AddTestResultAsync(testResult);
            return CreatedAtAction(nameof(Get), new { testResultId = testResult.Id }, testResult);
        }

        /// <summary>
        /// Add a failure reason to a test result
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("failure/reason")]
        public async Task<IActionResult> Update([FromBody] UpdateFailureReasonRequest request)
        {
            Guard.Argument(request).NotNull();
            await testResultsRepository.UpdateFailureReasonAsync(request);
            return Ok();
        }

        /// <summary>
        /// Get all the trace image files belonging to a given test result
        /// </summary>
        /// <param name="testResultId"></param>
        /// <returns></returns>
        [HttpGet("trace/images/{testResultId}")]
        public async Task<IActionResult> GetTraceImageFiles(string testResultId)
        {
            try
            {
                var result =  await this.testResultsRepository.GetTraceImages(testResultId);      
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "There was an error while retrieving trace image files for test result : '{0}'", testResultId);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get the trace image file with specified name belonging to a given test result
        /// </summary>
        /// <param name="testResultId">Identifier of the test result</param>
        /// <param name="imageFile">Name of the image file</param>
        /// <returns></returns>
        [HttpGet("trace/{testResultId}/image/{imageFile}")]
        public async Task<IActionResult> GetTraceImageFile(string testResultId, string imageFile)
        {
            try
            {
                var result = await this.testResultsRepository.GetTraceImage(testResultId, imageFile);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "There was an error while retrieving trace image file : '{0}' for test result : '{1}'", imageFile, testResultId);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Add a image trace file for a TestResult
        /// </summary>
        /// <param name="traceImage"></param>
        /// <param name="imageFile"></param>
        /// <returns></returns>
        [HttpPost("trace/images")]
        public async Task<IActionResult> AddTraceImage([FromBody][ModelBinder(typeof(JsonModelBinder), Name = nameof(TraceImageMetaData))] TraceImageMetaData traceImage, [FromForm(Name = "files")] IEnumerable<IFormFile> imageFiles)
        {
            Guard.Argument(traceImage, nameof(traceImage)).NotNull();          
            Guard.Argument(imageFiles, nameof(imageFiles)).NotNull();
            try
            {
                foreach(var imageFile in imageFiles)
                {
                    using (var ms = new MemoryStream())
                    {
                        imageFile.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        await testResultsRepository.AddTraceImage(traceImage, imageFile.FileName, fileBytes);                       
                    }                  
                }
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "There was an error while saving the trace image files for SesionId : '{0}' and TestResultId : '{1}'.", traceImage.SessionId, traceImage.TestResultId);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
