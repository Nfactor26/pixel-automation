using Pixel.Persistence.Services.Client.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public interface ITemplateClient
    {
        /// <summary>
        /// Get <see cref="SessionTemplate"/> with a given Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        Task<SessionTemplate> GetByIdAsync(string Id);

        /// <summary>
        /// Get <see cref="SessionTemplate"/> with a given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<SessionTemplate> GetByNameAsync(string name);

        /// <summary>
        /// Get all the available <see cref="SessionTemplate"/> in database
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<SessionTemplate>> GetAllAsync();

        /// <summary>
        /// Store the <see cref="SessionTemplate"/> in database as a new entry
        /// </summary>
        /// <param name="sessionTemplate"></param>
        /// <returns></returns>
        Task CreateAsync(SessionTemplate sessionTemplate);

        /// <summary>
        /// Update the details of existing <see cref="SessionTemplate"/> in database
        /// </summary>
        /// <param name="sessionTemplate"></param>
        /// <returns></returns>
        Task UpdateAsync(SessionTemplate sessionTemplate);
       
        /// <summary>
        /// Delete <see cref="SessionTemplate"/> with a given Id from database
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        Task DeleteAsync(string Id);
       
        /// <summary>
        /// Delete <see cref="SessionTemplate"/> from database
        /// </summary>
        /// <param name="sessionTemplate"></param>
        /// <returns></returns>
        Task DeleteAsync(SessionTemplate sessionTemplate);

      
    }
}
