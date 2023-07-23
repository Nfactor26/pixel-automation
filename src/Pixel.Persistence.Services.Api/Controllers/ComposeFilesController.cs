using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pixel.Persistence.Respository;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComposeFilesController : ControllerBase
    {
        private readonly ILogger<ComposeFilesController> logger;      
        private readonly IComposeFilesRepository composeFilesRepository;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="composeFilesRepository"></param>
        public ComposeFilesController(ILogger<ComposeFilesController> logger, IComposeFilesRepository composeFilesRepository)
        {
            this.logger = Guard.Argument(logger).NotNull().Value;          
            this.composeFilesRepository = Guard.Argument(composeFilesRepository).NotNull().Value;
        }

        [HttpGet("names")]
        public async Task<IEnumerable<string>> GetAllFileNames()
        {
            var files = composeFilesRepository.GetAllFileNamesAsync();
            var names = new List<string>();
            await foreach(var fileName in files)
            {
                names.Add(fileName);
            }
            return names;
        }
             
        [HttpGet]
        public async Task<ActionResult> GetAllFilesAsync()
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Create))
                    {
                        await foreach (var file in this.composeFilesRepository.GetAllFilesAsync())
                        {
                            var zipEntry = zipArchive.CreateEntry(Path.Combine(file.FilePath));
                            using (var zipEntryStream = zipEntry.Open())
                            {
                                zipEntryStream.Write(file.Bytes);
                            }
                        }
                    }
                    return File(stream.ToArray(), "application/zip", "compose-files.zip");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }
             
        [HttpGet("name/{fileName}")]
        public async Task<ActionResult> GetFileByName(string fileName)
        {
            try
            {
                var file = await composeFilesRepository.GetFileAsync(fileName);
                if (file != null)
                {
                    return Ok(file);
                }
                return NotFound();

            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost()]
        public async Task<IActionResult> AddComposeFileAsync(IFormFile composeFile)
        {
            try
            {
                Guard.Argument(composeFile, nameof(composeFile)).NotNull();
                if (await composeFilesRepository.CheckFileExistsAsync(composeFile.FileName))
                {
                    logger.LogInformation("File {0} already exists and will be replaced", composeFile.FileName);
                }
                Byte[] fileBytes;
                using (var ms = new MemoryStream())
                {
                    composeFile.CopyTo(ms);
                    fileBytes = ms.ToArray();
                }
                await composeFilesRepository.AddOrUpdateFileAsync(composeFile.FileName, fileBytes);
                logger.LogInformation("File {0} was uploaded", composeFile.FileName);
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
