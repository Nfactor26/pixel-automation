using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using Pixel.Persistence.Respository.Interfaces;
using Pixel.Persistence.Services.Api.Extensions;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectFilesController : FilesController
    {
        public ProjectFilesController(ILogger<ProjectFilesController> logger, IProjectFilesRepository filesRepository)
            :base(logger, filesRepository)
        {
            
        }
    }


    [Route("api/[controller]")]
    [ApiController]
    public class PrefabFilesController : FilesController
    {
        public PrefabFilesController(ILogger<PrefabFilesController> logger, IPrefabFilesRepository filesRepository)
           : base(logger, filesRepository)
        {

        }
    }


    public abstract class FilesController : ControllerBase
    {
        protected readonly ILogger<FilesController> logger;
        protected readonly IFilesRepository filesRepository;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="projectRepository"></param>
        public FilesController(ILogger<FilesController> logger, IFilesRepository filesRepository)
        {
            this.logger = Guard.Argument(logger).NotNull().Value;
            this.filesRepository = Guard.Argument(filesRepository).NotNull().Value;
        }

        /// <summary>
        /// Get a file with specified name for a given version of project
        /// </summary>
        /// <param name="projectId">Id of the Project</param>
        /// <param name="projectversion">Version of the Project</param>
        /// <param name="fileName">Name of the file to be retrieved</param>
        /// <returns></returns>
        [HttpGet("{projectId}/{projectVersion}/name/{fileName}")]
        public async Task<ActionResult> GetFileByName(string projectId, string projectversion, string fileName)
        {
            try
            {
                var file = await filesRepository.GetFileAsync(projectId, projectversion, fileName);
                if(file != null)
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

        /// <summary>
        /// Get all the files having one of the specified tags for a given version of project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="projectVersion"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        [HttpGet("{projectId}/{projectVersion}/tags")]
        public async Task<ActionResult> GetFilesWithTag(string projectId, string projectVersion, [FromQuery(Name = "tag")] string[] tags)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Create))
                    {
                        await foreach (var file in this.filesRepository.GetFilesAsync(projectId, projectVersion, tags))
                        {
                            var zipEntry = zipArchive.CreateEntry(Path.Combine(file.FilePath));
                            using (var zipEntryStream = zipEntry.Open())
                            {
                                zipEntryStream.Write(file.Bytes);
                            }
                        }
                    }
                    return File(stream.ToArray(), "application/zip", $"{projectId}.zip");

                }          

            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("{projectId}/{projectVersion}/type/{fileExtension}")]
        public async Task<ActionResult> GetFilesOfType(string projectId, string projectVersion, string fileExtension)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Create))
                    {
                        await foreach (var file in this.filesRepository.GetFilesOfTypeAsync(projectId, projectVersion, fileExtension))
                        {
                            var zipEntry = zipArchive.CreateEntry(Path.Combine(file.FilePath));
                            using (var zipEntryStream = zipEntry.Open())
                            {
                                zipEntryStream.Write(file.Bytes);
                            }
                        }
                    }
                    return File(stream.ToArray(), "application/zip", $"{projectId}.zip");

                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }


        /// <summary>
        /// Add a new file to a given version of project
        /// </summary>
        /// <param name="addFileRequest">Meta data for file</param>
        /// <param name="file">File to be added</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> AddFile([FromBody][ModelBinder(typeof(JsonModelBinder), Name = nameof(AddProjectFileRequest))] AddProjectFileRequest addFileRequest, [FromForm(Name = "file")] IFormFile file)
        {
            try
            {
                Byte[] fileBytes;
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    fileBytes = ms.ToArray();                             
                }
                string filePath = addFileRequest.FilePath.Replace("\\", "/");
                await this.filesRepository.AddOrUpdateFileAsync(addFileRequest.ProjectId, addFileRequest.ProjectVersion, new ProjectDataFile()
                {
                    ProjectId = addFileRequest.ProjectId,
                    ProjectVersion = addFileRequest.ProjectVersion,
                    Tag = addFileRequest.Tag,
                    FileName = addFileRequest.FileName,
                    FilePath = filePath,
                    Bytes = fileBytes
                });
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Delete a file with specified name from a given version of project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="projectVersion"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<ActionResult> DeleteFileAsync([FromBody][ModelBinder(typeof(JsonModelBinder), Name = nameof(DeleteProjectFileRequest))] DeleteProjectFileRequest deleteFileRequest)
        {
            try
            {
                await this.filesRepository.DeleteFileAsync(deleteFileRequest.ProjectId, deleteFileRequest.ProjectVersion, deleteFileRequest.FileName);
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
