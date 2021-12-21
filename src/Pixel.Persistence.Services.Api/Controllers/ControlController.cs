using Dawn;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Respository;
using Pixel.Persistence.Services.Api.Extensions;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{
    [AllowAnonymous]
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
        /// Get all the control data including control images as a zipped file for the control id's 
        /// specified in request
        /// </summary>
        /// <param name="controlDataReqest"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] GetControlDataForApplicationRequest controlDataReqest)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Create))
                    {
                        foreach (var controlId in controlDataReqest.ControlIdCollection)
                        {
                            await foreach (var file in this.controlRepository.GetControlFiles(controlDataReqest.ApplicationId, controlId))
                            {
                                var zipEntry = zipArchive.CreateEntry(Path.Combine(controlId, file.FileName));
                                using (var zipEntryStream = zipEntry.Open())
                                {
                                    zipEntryStream.Write(file.Bytes);
                                }
                            }
                        }

                    }
                    return File(stream.ToArray(), "application/zip", "ControlData.zip");
                }
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
        [Route("image")]
        [HttpPost]
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