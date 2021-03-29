﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Respository;
using Pixel.Persistence.Services.Api.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationRepository applicationRepository;

        public ApplicationController(IApplicationRepository applicationRepository)
        {
            this.applicationRepository = applicationRepository;
        }


        [HttpGet("{applicationId}")]
        public async Task<ActionResult> Get(string applicationId)
        {
            var applicationBytes = await applicationRepository.GetApplicationFile(applicationId);
            return File(applicationBytes, "application/octet-stream", applicationId);
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody][ModelBinder(typeof(JsonModelBinder), Name = nameof(ApplicationMetaData))] ApplicationMetaData applicationDescription, [FromForm(Name = "file")] IFormFile applicationFile)
        {
            using (var ms = new MemoryStream())
            {
                applicationFile.CopyTo(ms);
                var fileBytes = ms.ToArray();
                await applicationRepository.AddOrUpdate(applicationDescription, applicationFile.FileName, fileBytes);
            }

            return CreatedAtAction(nameof(Get), new { applicationId = applicationDescription.ApplicationId }, applicationDescription);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] ApplicationMetaData applicationDescription, IFormFile applicationFile)
        {
            using (var ms = new MemoryStream())
            {
                applicationFile.CopyTo(ms);
                var fileBytes = ms.ToArray();
                await applicationRepository.AddOrUpdate(applicationDescription, applicationFile.FileName, fileBytes);
            }

            return CreatedAtAction(nameof(Get), new { applicationId = applicationDescription.ApplicationId }, applicationDescription);
        }
    }
}