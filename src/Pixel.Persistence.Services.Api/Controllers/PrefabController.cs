using Dawn;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Security;
using Pixel.Persistence.Respository;
using Pixel.Persistence.Services.Api.Extensions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{
    [Authorize(Policy = Policies.ReadProcessDataPolicy)]
    [Route("api/[controller]")]
    [ApiController]
    public class PrefabController : ControllerBase
    {
        private readonly ILogger<PrefabController> logger;
        private readonly IPrefabRepository prefabRepository;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="prefabRepository"></param>
        public PrefabController(ILogger<PrefabController> logger, IPrefabRepository prefabRepository)
        {
            this.logger = Guard.Argument(logger).NotNull().Value;
            this.prefabRepository = Guard.Argument(prefabRepository).NotNull().Value;
        }

        /// <summary>
        /// Get the prefab description file given it's prefabId
        /// </summary>
        /// <param name="prefabId"></param>
        /// <returns></returns>
        [HttpGet("{prefabId}")]
        public async Task<ActionResult> Get(string prefabId)
        {
            try
            {
                var bytes = await prefabRepository.GetPrefabFileAsync(prefabId);
                return File(bytes, "application/octet-stream", prefabId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get all the files (process, template, scripts, etc) for a specific version of a prefab as a zipped file
        /// </summary>
        /// <param name="prefabId"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        [HttpGet("{prefabId}/{version}")]
        public async Task<ActionResult> Get(string prefabId, string version)
        {
            try
            {
                var bytes = await prefabRepository.GetPrefabDataFilesAsync(prefabId, version);
                return File(bytes, "application/octet-stream", prefabId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Add or update prefab file  or all the prefab data files (process, templates, scripts, etc) as a zipped file for a specific version of a prefab 
        /// </summary>
        /// <param name="projectDescription"></param>
        /// <param name="prefabFile"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Policy = Policies.WriteProcessDataPolicy)]       
        public async Task<IActionResult> Post([FromBody][ModelBinder(typeof(JsonModelBinder), Name = nameof(PrefabMetaData))] PrefabMetaData prefabDescription, [FromForm(Name = "file")] IFormFile prefabFile)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    prefabFile.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    await prefabRepository.AddOrUpdatePrefabAsync(prefabDescription, prefabFile.FileName, fileBytes);
                }
                if (string.IsNullOrEmpty(prefabDescription.Version))
                {
                    return CreatedAtAction(nameof(Get), new { prefabId = prefabDescription.PrefabId }, prefabDescription);
                }
                else
                {
                    return CreatedAtAction(nameof(Get), new { prefabId = prefabDescription.PrefabId, version = prefabDescription.Version }, prefabDescription);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
