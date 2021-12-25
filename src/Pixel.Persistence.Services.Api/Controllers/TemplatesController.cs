﻿using Dawn;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Security;
using Pixel.Persistence.Respository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Controllers
{
    [Authorize(Policy = Policies.ReadProcessDataPolicy)]
    [Route("api/[controller]")]
    [ApiController]
    public class TemplatesController : ControllerBase
    {
        private readonly ITemplateRepository templateRepository;

        public TemplatesController(ITemplateRepository templateRepository)
        {
            this.templateRepository = templateRepository;
        }

        // GET: api/TestSession/5
        [HttpGet("id/{id}")]
        public async Task<ActionResult<SessionTemplate>> GetByIdAsync(string Id)
        {
           var result = await templateRepository.GetByIdAsync(Id);
            if (result != null)
            {
                return result;
            }
            return NotFound($"No template exists with Id : {Id}");
        }

        [HttpGet("name/{name}")]
        public async Task<ActionResult<SessionTemplate>> GetByNameAsync(string name)
        {
            var result = await templateRepository.GetByNameAsync(name);
            if(result != null)
            {
                return result;
            }
            return NotFound($"No template exists with name : {name}");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SessionTemplate>>> GetAllAsync()
        {
            var result = await templateRepository.GetAllAsync();
            return result.ToList();
        }

        [HttpPost]
        [Authorize(Policy = Policies.WriteProcessDataPolicy)]       
        public async Task<IActionResult> Create([FromBody] SessionTemplate template)
        {
            Guard.Argument(template).NotNull();
            var exists = await templateRepository.GetByNameAsync(template.Name);
            if(exists != null)
            {
                return BadRequest($"A template already exists with name : {template.Name}");
            }
            await templateRepository.CreateAsync(template);
            return Ok();
        }

        [HttpPut]
        [Authorize(Policy = Policies.WriteProcessDataPolicy)]      
        public async Task<IActionResult> Update([FromBody] SessionTemplate template)
        {
            Guard.Argument(template).NotNull();
            var existing = await templateRepository.GetByIdAsync(template.Id);
            if(existing == null)
            {
                return BadRequest($"Template with Id : {template.Id} doesn't exist");
            }           
            if(!existing.Name.Equals(template.Name))
            {
                var anotherWithName = await templateRepository.GetByNameAsync(template.Name);
                if (anotherWithName != null)
                {
                    return BadRequest($"A template already exists with name : {template.Name}. Name can't be duplicate.");
                }

            }
            await templateRepository.UpdateAsync(template);
            return Ok();
        }

        [HttpDelete]
        [Authorize(Policy = Policies.WriteProcessDataPolicy)]
        public async Task<IActionResult> Delete(string Id)
        {
           if(await templateRepository.TryDeleteAsync(Id))
           {
                return Ok();
           }
           return BadRequest($"Template with Id : {Id} doesn't exist");
        }
    }
}
