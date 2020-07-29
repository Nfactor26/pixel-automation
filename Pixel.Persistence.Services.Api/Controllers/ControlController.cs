using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Respository;
using Pixel.Persistence.Services.Api.Extensions;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ControlController : ControllerBase
    {
        private readonly IControlRepository controlRepository;

        public ControlController(IControlRepository controlRepository)
        {
            this.controlRepository = controlRepository;
        }

        //[HttpGet]
        //public async Task<ActionResult> Get(GetControlDataForMultipleApplicationRequest controlDataReqest)
        //{
        //    using (var stream = new MemoryStream())
        //    {
        //        using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Create))
        //        {
        //            foreach (var request in controlDataReqest.ControlDataRequestCollection)
        //            {
        //                foreach (var controlId in request.ControlIdCollection)
        //                {
        //                    await foreach (var file in this.controlRepository.GetControlFiles(request.ApplicationId, controlId))
        //                    {
        //                        var zipEntry = zipArchive.CreateEntry(file.FileName);
        //                        using (var zipEntryStream = zipEntry.Open())
        //                        {
        //                            zipEntryStream.Write(file.Bytes);
        //                        }
        //                    }
        //                }
        //            }

        //        }
        //        return File(stream.ToArray(), "application/zip", "ControlData.zip");
        //    }
        //}


        [HttpGet]
        public async Task<ActionResult> Get([FromQuery]GetControlDataForApplicationRequest controlDataReqest)
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


        [HttpPost]
        public async Task<IActionResult> Post([FromBody][ModelBinder(typeof(JsonModelBinder), Name = nameof(ControlMetaData))] ControlMetaData controlDescription, [FromForm(Name = "file")] IFormFile controlFile)
        {
            using (var ms = new MemoryStream())
            {
                controlFile.CopyTo(ms);
                var fileBytes = ms.ToArray();
                await controlRepository.AddOrUpdateControl(controlDescription, controlFile.FileName, fileBytes);
            }

            return CreatedAtAction(nameof(Get), new { applicationId = controlDescription.ApplicationId, controlId = controlDescription.ControlId }, controlDescription);
        }

        [Route("image")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody][ModelBinder(typeof(JsonModelBinder), Name = nameof(ControlImageMetaData))] ControlImageMetaData controlImage, [FromForm(Name = "file")] IFormFile imageFile)
        {
            using (var ms = new MemoryStream())
            {
                imageFile.CopyTo(ms);
                var fileBytes = ms.ToArray();
                await controlRepository.AddOrUpdateControlImage(controlImage, imageFile.FileName, fileBytes);
            }

            return CreatedAtAction(nameof(Get), new { applicationId = controlImage.ApplicationId, controlId = controlImage.ControlId }, controlImage);
        }

    }
}