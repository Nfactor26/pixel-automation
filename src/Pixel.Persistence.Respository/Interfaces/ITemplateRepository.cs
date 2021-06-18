using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    public interface ITemplateRepository
    {
        Task<SessionTemplate> GetByIdAsync(string id);
        Task<SessionTemplate> GetByNameAsync(string name);
        Task<IEnumerable<SessionTemplate>> GetAllAsync();
        Task CreateAsync(SessionTemplate template);
        Task UpdateAsync(SessionTemplate template);
        Task<bool> TryDeleteAsync(string id);       
    }
}