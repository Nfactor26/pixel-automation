using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using Pixel.Persistence.Respository;
using Pixel.Persistence.Services.Api.Extensions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ControlController : ControllerBase
    {
        private readonly ILogger<ControlController> logger;
        private readonly IApplicationRepository applicationRepository;
        private readonly IControlRepository controlRepository;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="controlRepository"></param>
        public ControlController(ILogger<ControlController> logger, IControlRepository controlRepository, IApplicationRepository applicationRepository)
        {
            this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;
            this.controlRepository = Guard.Argument(controlRepository, nameof(controlRepository)).NotNull().Value;
            this.applicationRepository = Guard.Argument(applicationRepository, nameof(applicationRepository)).NotNull().Value;
        }        

        /// <summary>
        /// Get all the controls for a given application which have beem modified since specified datetime
        /// </summary>
        /// <param name="controlDataReqest"></param>
        /// <returns></returns>
        [HttpGet("{applicationId}")]
        public async Task<ActionResult> GetControls(string applicationId, [FromHeader] DateTime laterThan)
        {
            try
            {
                var controls = await this.controlRepository.GetAllControlsForApplication(applicationId, laterThan);
                return Ok(controls);
                
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }


        /// <summary>
        /// Get all the control images for a given application which have beem modified since specified datetime
        /// </summary>
        /// <param name="controlDataReqest"></param>
        /// <returns></returns>       
        [HttpGet("image/{applicationId}")]
        public async Task<ActionResult> GetControlImages(string applicationId, [FromHeader] DateTime laterThan)
        {
            try
            {
                var images = await this.controlRepository.GetAllControlImagesForApplication(applicationId, laterThan);
                return Ok(images);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Save the control details in database
        /// </summary>
        /// <param name="controlDescription"></param>
        /// <returns></returns>
        [HttpPost]       
        public async Task<IActionResult> AddControlAsync([FromBody] AddControlRequest addControlRequest)
        {
            try
            { 
                await controlRepository.AddControl(addControlRequest.ControlData);
                await applicationRepository.AddControlToScreen(addControlRequest.ApplicationId, addControlRequest.ControlId, addControlRequest.ScreenId);
                return Ok();
            }         
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }         
        }

        /// <summary>
        /// Save the control details in database
        /// </summary>
        /// <param name="controlDescription"></param>
        /// <returns></returns>
        [HttpPut()]
        public async Task<IActionResult> UpdateControlAsync([FromBody] object controlDescription)
        {
            try
            {
                await controlRepository.UpdateControl(controlDescription);
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("{applicationId}/{controlId}")]
        public async Task<IActionResult> DeleteControl(string applicationId, string controlId)
        {
            try
            {
                await controlRepository.DeleteControlAsync(applicationId, controlId);
                string screenId = await applicationRepository.GetScreenForControl(applicationId, controlId);
                await applicationRepository.DeleteControlFromScreen(applicationId, controlId, screenId);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        /// <summary>
        /// Save the control image in database
        /// </summary>
        /// <param name="controlImage">Metadata for the image</param>
        /// <param name="imageFile">Image file</param>
        /// <returns></returns>        
        [HttpPost("image")]    
        public async Task<IActionResult> AddImage([FromBody][ModelBinder(typeof(JsonModelBinder), Name = nameof(ControlImageMetaData))] ControlImageMetaData controlImage, [FromForm(Name = "file")] IFormFile imageFile)
        {
            try
            {
                if (await controlRepository.IsControlDeleted(controlImage.ApplicationId, controlImage.ControlId))
                {
                    return Conflict("Can't add image. Control is marked deleted.");
                }
                using (var ms = new MemoryStream())
                {
                    imageFile.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    await controlRepository.AddOrUpdateControlImage(controlImage, fileBytes);
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Delete an image from the database
        /// </summary>
        /// <param name="controlImage"></param>
        /// <returns></returns>
        [Route("image/delete")]
        [HttpDelete]       
        public async Task<IActionResult> DeleteImage([FromBody] ControlImageMetaData controlImage)
        {
            try
            {
                if (await controlRepository.IsControlDeleted(controlImage.ApplicationId, controlImage.ControlId))
                {
                    return Conflict("Can't delete image. Control is marked deleted.");
                }
                await controlRepository.DeleteImageAsync(controlImage);
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