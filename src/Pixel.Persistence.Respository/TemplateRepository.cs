using Dawn;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Core.Request;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    public class TemplateRepository : ITemplateRepository
    {
        private readonly ILogger<TemplateRepository> logger;
        private readonly IMongoCollection<SessionTemplate> templates;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="dbSettings"></param>
        public TemplateRepository(ILogger<TemplateRepository> logger, IMongoDbSettings dbSettings)
        {
            this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;
            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);
            templates = database.GetCollection<SessionTemplate>(dbSettings.TemplatesCollectionName);
        }

        /// <inheritdoc/>
        public async Task<SessionTemplate> GetTemplateByIdAsync(string id)
        {
            Guard.Argument(id).NotNull().NotEmpty();
            var result = await templates.FindAsync<SessionTemplate>(s => s.Id.Equals(id));
            return await result.FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public async Task<SessionTemplate> GetTemplateByNameAsync(string name)
        {
            Guard.Argument(name).NotNull().NotEmpty();
            var result = await templates.FindAsync<SessionTemplate>(s => s.Name.Equals(name));
            return await result.FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public async Task<IEnumerable<SessionTemplate>> GetAllTemplatesAsync()
        {
            var result = await templates.FindAsync<SessionTemplate>(t => true);
            return await result.ToListAsync();
        }

        /// <inheritdoc/>
        public async Task CreateTemplateAsync(SessionTemplate template)
        {
            Guard.Argument(template).NotNull();
            await templates.InsertOneAsync(template);
        }

        /// <inheritdoc/>
        public async Task UpdateTemplateAsync(SessionTemplate template)
        {
            Guard.Argument(template).NotNull();

            var filter = Builders<SessionTemplate>.Filter.Eq(t => t.Id, template.Id);

            var updateDefinition = Builders<SessionTemplate>.Update           
            .Set(t => t.Name, template.Name)
            .Set(t => t.Selector, template.Selector)
            .Set(t => t.TargetVersion, template.TargetVersion)
            .Set(t => t.InitFunction, template.InitFunction);            

            await templates.FindOneAndUpdateAsync<SessionTemplate>(filter, updateDefinition);
        }

        /// <inheritdoc/>
        public async Task<bool> TryDeleteTemplateAsync(string id)
        {
            Guard.Argument(id).NotNull().NotEmpty();
            var result = await templates.DeleteOneAsync(s => s.Id.Equals(id));
            return result.DeletedCount == 1;
        }

        /// <inheritdoc/>
        public async Task AddTriggerAsync(SessionTemplate template, SessionTrigger trigger)
        {
            Guard.Argument(template, nameof(template)).NotNull();
            Guard.Argument(trigger, nameof(trigger)).NotNull();            
            template.Triggers.Add(trigger);
            await templates.UpdateOneAsync(x => x.Id.Equals(template.Id), Builders<SessionTemplate>.Update.Set(x => x.Triggers, template.Triggers), null, CancellationToken.None);            
        }

        /// <inheritdoc/>
        public async Task DeleteTriggerAsync(SessionTemplate template, SessionTrigger trigger)
        {
            Guard.Argument(template, nameof(template)).NotNull();
            Guard.Argument(trigger, nameof(trigger)).NotNull();
            template.Triggers.RemoveAll(x => x.Equals(trigger));         
            await templates.UpdateOneAsync(x => x.Id.Equals(template.Id), Builders<SessionTemplate>.Update.Set(x => x.Triggers, template.Triggers), null, CancellationToken.None);
        }

        /// <inheritdoc/>
        public async Task PauseTemplateAsync(string templateName)
        {
            var template = await GetTemplateByNameAsync(templateName);
            foreach(var trigger in template.Triggers) 
            {
                await ToggleTriggerStateAsync(templateName, trigger.Name, false);
                logger.LogInformation("Trigger {0} for template {1} was is now marked disabled", trigger.Name, templateName);
            }
        }

        /// <inheritdoc/>
        public async Task ResumeTemplateAsync(string templateName)
        {
            var template = await GetTemplateByNameAsync(templateName);
            foreach (var trigger in template.Triggers)
            {
                await ToggleTriggerStateAsync(templateName, trigger.Name, true);
                logger.LogInformation("Trigger {0} for template {1} was is now marked enabled", trigger.Name, templateName);
            }
        }

        /// <inheritdoc/>
        public async Task PauseTriggerAsync(string templateName, string triggerName)
        {           
            await ToggleTriggerStateAsync(templateName, triggerName, false);
            logger.LogInformation("Trigger {0} for template {1} was is now marked disabled", triggerName, templateName);
        }

        /// <inheritdoc/>
        public async Task ResumeTriggerAsync(string templateName, string triggerName)
        {
            await ToggleTriggerStateAsync(templateName, triggerName, true);
            logger.LogInformation("Trigger {0} for template {1} was is now marked enabled", triggerName, templateName);
        }

        async Task ToggleTriggerStateAsync(string templateName, string triggerName, bool state)
        {
            Guard.Argument(templateName, nameof(templateName)).NotNull().NotEmpty();
            Guard.Argument(triggerName, nameof(triggerName)).NotNull().NotEmpty();
            var template = await GetTemplateByNameAsync(templateName);
            var trigger = template.Triggers.FirstOrDefault(x => x.Name == triggerName);
            trigger.IsEnabled = state;
            var filter = Builders<SessionTemplate>.Filter.Eq(x => x.Name, templateName)
                       & Builders<SessionTemplate>.Filter.ElemMatch(x => x.Triggers, Builders<SessionTrigger>.Filter.Eq(x => x.Name, triggerName));
            var update = Builders<SessionTemplate>.Update.Set(x => x.Triggers.FirstMatchingElement(), trigger);
            await this.templates.UpdateOneAsync(filter, update);
        }
    }
}
