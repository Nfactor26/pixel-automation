﻿using Microsoft.AspNetCore.Mvc;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Respository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetaDataController : ControllerBase
    {

        private readonly IApplicationRepository applicationRepository;
        private readonly IControlRepository controlRepository;
        public MetaDataController(IApplicationRepository applicationRepository, IControlRepository controlRepository)
        {
            this.applicationRepository = applicationRepository;
            this.controlRepository = controlRepository;
        }


        [Route("application")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationMetaData>>> GetMetaData()
        {
            List<ApplicationMetaData> applications = new List<ApplicationMetaData>();
            await foreach (var applicationMetadata in applicationRepository.GetMetadataAsync())
            {
                applications.Add(applicationMetadata);
                List<ControlMetaData> controlMetaDatas = new List<ControlMetaData>();
                await foreach (var controlMetaData in this.controlRepository.GetMetadataAsync(applicationMetadata.ApplicationId))
                {
                    controlMetaDatas.Add(controlMetaData);
                }
                applicationMetadata.ControlsMeta = controlMetaDatas;
            }
            return applications;
        }
    }
}