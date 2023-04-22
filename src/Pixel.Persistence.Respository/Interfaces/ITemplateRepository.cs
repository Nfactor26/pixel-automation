﻿using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    /// <summary>
    /// ITemplateRepository is used to manage <see cref="SessionTemplate"/> stored in database
    /// </summary>
    public interface ITemplateRepository
    {
        /// <summary>
        /// Get a <see cref="SessionTemplate"/> by Id
        /// </summary>
        /// <param name="id">Id of the SessionTemplate</param>
        /// <returns>SessionTemplate with matching Id</returns>
        Task<SessionTemplate> GetByIdAsync(string id);

        /// <summary>
        /// Get a <see cref="SessionTemplate"/> by Name
        /// </summary>
        /// <param name="name">Name of the SessionTemplate</param>
        /// <returns>SessionTemplate with matching Name</returns>
        Task<SessionTemplate> GetByNameAsync(string name);

        /// <summary>
        /// Get all templates matching request parameters
        /// </summary>
        /// <param name="queryParameter"></param>
        /// <returns></returns>
        Task<IEnumerable<SessionTemplate>> GetTemplatesAsync(GetTemplatesRequest queryParameter);

        /// <summary>
        /// Get all the <see cref="SessionTemplate"/> stored in database
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<SessionTemplate>> GetAllAsync();

        /// <summary>
        /// Add a new <see cref="SessionTemplate"/>
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        Task CreateAsync(SessionTemplate template);

        /// <summary>
        /// Update the details of an existing <see cref="SessionTemplate"/>
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        Task UpdateAsync(SessionTemplate template);

        /// <summary>
        /// Delete a <see cref="SessionTemplate"/> by Id
        /// </summary>
        /// <param name="id">Id of the SessionTemplate to be deleted</param>
        /// <returns></returns>
        Task<bool> TryDeleteAsync(string id);

        /// <summary>
        /// Add a new trigger to the template
        /// </summary>
        /// <param name="template"></param>
        /// <param name="trigger"></param>
        /// <returns></returns>
        Task AddTriggerAsync(SessionTemplate template, SessionTrigger trigger);

        /// <summary>
        /// Delete an existing trigger from template
        /// </summary>
        /// <param name="template"></param>
        /// <param name="trigger"></param>
        /// <returns></returns>
        Task DeleteTriggerAsync(SessionTemplate template, SessionTrigger trigger);
    }
}