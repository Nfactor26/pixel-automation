using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pixel.Persistence.Core.Models;
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
        private readonly IControlRepository controlRepository;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="controlRepository"></param>
        public ControlController(ILogger<ControlController> logger, IControlRepository controlRepository)
        {
            this.logger = Guard.Argument(logger).NotNull().Value;
            this.controlRepository = Guard.Argument(controlRepository).NotNull().Value;   
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
        public async Task<IActionResult> Post([FromBody] object controlDescription)
        {
            try
            {
                await controlRepository.AddOrUpdateControl(controlDescription.ToString());
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
        /// Save the control image in database
        /// </summary>
        /// <param name="controlImage">Metadata for the image</param>
        /// <param name="imageFile">Image file</param>
        /// <returns></returns>        
        [HttpPost("image")]    
        public async Task<IActionResult> Post([FromBody][ModelBinder(typeof(JsonModelBinder), Name = nameof(ControlImageMetaData))] ControlImageMetaData controlImage, [FromForm(Name = "file")] IFormFile imageFile)
        {
            try
            {
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
        [HttpPost]       
        public async Task<IActionResult> Delete([FromBody] ControlImageMetaData controlImage)
        {
            try
            {
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