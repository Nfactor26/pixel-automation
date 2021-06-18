using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public interface ITemplateClient
    {
        Task<SessionTemplate> GetByIdAsync(string Id);

        Task<SessionTemplate> GetByNameAsync(string name);

        Task<IEnumerable<SessionTemplate>> GetAllAsync();

        Task CreateAsync(SessionTemplate sessionTemplate);

        Task UpdateAsync(SessionTemplate sessionTemplate);

        Task DeleteAsync(SessionTemplate sessionTemplate);

        Task DeleteAsync(string Id);
    }
}
