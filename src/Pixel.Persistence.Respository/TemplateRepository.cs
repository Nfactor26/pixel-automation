using Dawn;
using MongoDB.Driver;
using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    public class TemplateRepository : ITemplateRepository
    {
        private readonly IMongoCollection<SessionTemplate> templates;

        public TemplateRepository(IMongoDbSettings dbSettings)
        {
            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);
            templates = database.GetCollection<SessionTemplate>(dbSettings.TemplatesCollectionName);
        }

        public async Task<SessionTemplate> GetByIdAsync(string id)
        {
            Guard.Argument(id).NotNull().NotEmpty();
            var result = await templates.FindAsync<SessionTemplate>(s => s.Id.Equals(id));
            return await result.FirstOrDefaultAsync();
        }

        public async Task<SessionTemplate> GetByNameAsync(string name)
        {
            Guard.Argument(name).NotNull().NotEmpty();
            var result = await templates.FindAsync<SessionTemplate>(s => s.Name.Equals(name));
            return await result.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<SessionTemplate>> GetAllAsync()
        {
            var result = await templates.FindAsync<SessionTemplate>(t => true);
            return await result.ToListAsync();
        }

        public async Task CreateAsync(SessionTemplate template)
        {
            Guard.Argument(template).NotNull();
            await templates.InsertOneAsync(template);
        }

        public async Task UpdateAsync(SessionTemplate template)
        {
            Guard.Argument(template).NotNull();

            var filter = Builders<SessionTemplate>.Filter.Eq(t => t.Id, template.Id);

            var updateDefinition = Builders<SessionTemplate>.Update           
            .Set(t => t.ProjectVersion, template.ProjectVersion)
            .Set(t => t.Selector, template.Selector)
            .Set(t => t.InitializeScript, template.InitializeScript);            

            await templates.FindOneAndUpdateAsync<SessionTemplate>(filter, updateDefinition);
        }

        public async Task<bool> TryDeleteAsync(string id)
        {
            Guard.Argument(id).NotNull().NotEmpty();
            var result = await templates.DeleteOneAsync(s => s.Id.Equals(id));
            return result.DeletedCount == 1;
        }
    }
}
