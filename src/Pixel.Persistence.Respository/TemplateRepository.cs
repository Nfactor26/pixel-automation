using Dawn;
using MongoDB.Driver;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        public async Task<IEnumerable<SessionTemplate>> GetTemplatesAsync(GetTemplatesRequest queryParameter)
        {
            Guard.Argument(queryParameter).NotNull();

            var filterBuilder = Builders<SessionTemplate>.Filter;
            var filter = filterBuilder.Empty;
            if (!string.IsNullOrEmpty(queryParameter.TemplateFilter))
            {
                filter = filterBuilder.And(filter, filterBuilder.Regex(t => t.Name, new MongoDB.Bson.BsonRegularExpression(queryParameter.TemplateFilter)));
                filter = filterBuilder.Or(filter, filterBuilder.Regex(t => t.ProjectName, new MongoDB.Bson.BsonRegularExpression(queryParameter.TemplateFilter)));
            }           
            var sort = Builders<SessionTemplate>.Sort.Descending(nameof(SessionTemplate.Name));
            var all = templates.Find(filter).Sort(sort).Skip(queryParameter.Skip).Limit(queryParameter.Take);
            var result = await all.ToListAsync();
            return result ?? Enumerable.Empty<SessionTemplate>();
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
            .Set(t => t.Name, template.Name)
            .Set(t => t.Selector, template.Selector)
            .Set(t => t.TargetVersion, template.TargetVersion)
            .Set(t => t.InitializeScript, template.InitializeScript);            

            await templates.FindOneAndUpdateAsync<SessionTemplate>(filter, updateDefinition);
        }

        public async Task<bool> TryDeleteAsync(string id)
        {
            Guard.Argument(id).NotNull().NotEmpty();
            var result = await templates.DeleteOneAsync(s => s.Id.Equals(id));
            return result.DeletedCount == 1;
        }

        public async Task AddTriggerAsync(SessionTemplate template, SessionTrigger trigger)
        {
            Guard.Argument(template, nameof(template)).NotNull();
            Guard.Argument(trigger, nameof(trigger)).NotNull();            
            template.Triggers.Add(trigger);
            await templates.UpdateOneAsync(x => x.Id.Equals(template.Id), Builders<SessionTemplate>.Update.Set(x => x.Triggers, template.Triggers), null, CancellationToken.None);            
        }      

        public async Task DeleteTriggerAsync(SessionTemplate template, SessionTrigger trigger)
        {
            Guard.Argument(template, nameof(template)).NotNull();
            Guard.Argument(trigger, nameof(trigger)).NotNull();
            template.Triggers.RemoveAll(x => x.Equals(trigger));         
            await templates.UpdateOneAsync(x => x.Id.Equals(template.Id), Builders<SessionTemplate>.Update.Set(x => x.Triggers, template.Triggers), null, CancellationToken.None);
        }
    }
}
