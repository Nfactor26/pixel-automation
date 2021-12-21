﻿using Dawn;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pixel.Persistence.Respository;
using System;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{
    [AllowAnonymous]
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
        /// Get the applicatoin data for a given applicationId 
        /// </summary>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        [HttpGet("{applicationId}")]
        public async Task<ActionResult> Get(string applicationId)
        {
            try
            {
                var applicationData = await applicationRepository.GetApplicationData(applicationId);              
                return Ok(applicationData);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Save the application description data in to database
        /// </summary>
        /// <param name="applicationDescription"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] object applicationDescription)
        {
            try
            {
                await applicationRepository.AddOrUpdate(applicationDescription.ToString());
                return Ok();
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, ex.Message);
                return BadRequest(ex);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

    }
}
