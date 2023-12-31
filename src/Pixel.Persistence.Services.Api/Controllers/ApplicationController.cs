using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Respository;
using System;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationController : ControllerBase
    {
        private readonly ILogger<ApplicationController> logger;
        private readonly IApplicationRepository applicationRepository;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="applicationRepository"></param>
        public ApplicationController(ILogger<ApplicationController> logger, IApplicationRepository applicationRepository)
        {
            this.logger = Guard.Argument(logger).NotNull().Value;
            this.applicationRepository = Guard.Argument(applicationRepository).NotNull().Value;                        
        }

        /// <summary>
        /// Get the application data for a given applicationId 
        /// </summary>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        [HttpGet("id/{applicationId}")]
        public async Task<ActionResult> FindByIdAsync(string applicationId)
        {
            try
            {
                var applicationData = await applicationRepository.FindByIdAsync(applicationId);              
                return Ok(applicationData);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get all applications for specified platform modified since specified datetime
        /// </summary>
        /// <param name="laterThan"></param>
        /// <returns></returns>
        [HttpGet()]
        public async Task<ActionResult> GetAllAsync([FromHeader] string platform, [FromHeader] DateTime laterThan)
        {
            try
            {
                var applicationData = await applicationRepository.GetAllApplications(platform, laterThan);
                return Ok(applicationData);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Add new application details to database
        /// </summary>
        /// <param name="applicationDescription"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddApplicationAsync([FromBody] object applicationDescription)
        {
            try
            {
                await applicationRepository.AddApplication(applicationDescription);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest(ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Update details for an existing application
        /// </summary>
        /// <param name="applicationDescription"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> UpdateApplicationAsync([FromBody] object applicationDescription)
        {
            try
            {
                await applicationRepository.UpdateApplication(applicationDescription);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest(ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }


        /// <summary>
        /// Save the application description data in to database
        /// </summary>
        /// <param name="applicationDescription"></param>
        /// <returns></returns>
        [HttpDelete("{applicationId}")]
        public async Task<IActionResult> DeleteApplicationAsync(string applicationId)
        {
            try
            {
                await applicationRepository.DeleteAsync(applicationId);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, ex.Message);
                return Conflict(ex.Message);
            }
        }

        /// <summary>
        /// Add a new screen to the application
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="screenName"></param>
        /// <returns></returns>
        [HttpPost("{applicationId}/screens")]
        public async Task<IActionResult> AddScreenAsync(string applicationId, [FromBody] ApplicationScreen screen)
        {
            try
            {
                await applicationRepository.AddScreenAsync(applicationId, screen);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message);
            }
        }

        /// <summary>
        /// Rename an existing screen of the application
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="screenId"></param>
        /// <param name="newScreenName"></param>
        /// <returns></returns>
        [HttpPut("{applicationId}/screens/rename/{screenId}/to/{newScreenName}")]
        public async Task<IActionResult> RenameScreenAsync(string applicationId, string screenId, string newScreenName)
        {
            try
            {
                await applicationRepository.RenameScreenAsync(applicationId, screenId, newScreenName);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message);
            }
        }      

        /// <summary>
        /// Move a control to another screen
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="controlId"></param>
        /// <param name="targetScreenId"></param>
        /// <returns></returns>
        [HttpPost("{applicationId}/screens/control/{controlId}/move/to/{targetScreenId}")]
        public async Task<IActionResult> MoveControlToScreen(string applicationId, string controlId, string targetScreenId)
        {
            try
            {
                await applicationRepository.MoveControlToScreen(applicationId, controlId, targetScreenId);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message);
            }
        }

        /// <summary>
        /// Move a control to another screen
        /// </summary>
        /// <param name="applicationId"></param>
        /// <param name="prefabId"></param>
        /// <param name="targetScreenId"></param>
        /// <returns></returns>
        [HttpPost("{applicationId}/screens/prefab/{prefabId}/move/to/{targetScreenId}")]
        public async Task<IActionResult> MovePrefabToScreen(string applicationId, string prefabId, string targetScreenId)
        {
            try
            {
                await applicationRepository.MovePrefabToScreen(applicationId, prefabId, targetScreenId);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message);
            }
        }
    }
}
