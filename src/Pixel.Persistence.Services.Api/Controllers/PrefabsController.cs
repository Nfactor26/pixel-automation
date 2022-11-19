﻿using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using Pixel.Persistence.Respository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrefabsController : ControllerBase
    {
        private readonly ILogger<PrefabsController> logger;
        private readonly IPrefabsRepository prefabsRepository;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="projectRepository"></param>
        public PrefabsController(ILogger<PrefabsController> logger, IPrefabsRepository prefabsRepository)
        {
            this.logger = Guard.Argument(logger).NotNull().Value;
            this.prefabsRepository = Guard.Argument(prefabsRepository).NotNull().Value;
        }

        [HttpGet("id/{prefabId}")]
        public async Task<ActionResult<PrefabProject>> FindByIdAsync(string prefabId)
        {
            try
            {
                Guard.Argument(prefabId, nameof(prefabId)).NotNull().NotEmpty();
                var result = await prefabsRepository.FindByIdAsync(prefabId, CancellationToken.None) ??
                    throw new ArgumentException($"Prefab project with Id : {prefabId} doesn't exist");
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("name/{name}")]
        public async Task<ActionResult<PrefabProject>> FindByNameAsync(string name)
        {
            try
            {
                Guard.Argument(name, nameof(name)).NotNull().NotEmpty();
                var result = await prefabsRepository.FindByNameAsync(name, CancellationToken.None) ??
                    throw new ArgumentException($"Prefab project with name : {name} doesn't exist");
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet()]
        public async Task<ActionResult<IEnumerable<PrefabProject>>> GetAllAsync()
        {
            try
            {
                var result = await prefabsRepository.FindAllAsync(CancellationToken.None) ?? Enumerable.Empty<PrefabProject>();
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }


        [HttpPost]
        public async Task<ActionResult<PrefabProject>> AddPrefabAsync([FromBody] PrefabProject prefabProject)
        {
            try
            {
                Guard.Argument(prefabProject, nameof(prefabProject)).NotNull();
                await prefabsRepository.AddPrefabAsync(prefabProject, CancellationToken.None);
                return Ok(prefabProject);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }


        [HttpPost("{prefabId}/versions")]
        public async Task<ActionResult<ProjectVersion>> AddPrefabVersionAsync([FromRoute] string prefabId, [FromBody] AddPrefabVersionRequest request)
        {
            try
            {
                Guard.Argument(prefabId, nameof(prefabId)).NotNull().NotEmpty();
                Guard.Argument(request, nameof(request)).NotNull();
                Guard.Argument(request, nameof(request)).Require(r => r.NewVersion != null, r => "NewVersion is required");
                Guard.Argument(request, nameof(request)).Require(r => r.CloneFrom != null, r => "CloneFrom is required");
                await prefabsRepository.AddPrefabVersionAsync(prefabId, request.NewVersion, request.CloneFrom, CancellationToken.None);
                return Ok(request.NewVersion);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }


        [HttpPut("{prefabId}/versions")]
        public async Task<ActionResult<PrefabVersion>> UpdatePrefabVersionAsync([FromRoute] string prefabId, [FromBody] PrefabVersion prefabVersion)
        {
            try
            {
                Guard.Argument(prefabId, nameof(prefabId)).NotNull().NotEmpty();
                Guard.Argument(prefabVersion, nameof(prefabVersion)).NotNull();
                await prefabsRepository.UpdatePrefabVersionAsync(prefabId, prefabVersion, CancellationToken.None);
                return Ok(prefabVersion);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}