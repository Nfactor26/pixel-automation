using Dawn;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Pixel.Persistence.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly ILogger logger;
        private readonly IMongoCollection<BsonDocument> applicationsCollection;
        private readonly IMongoCollection<ProjectReferences> referencesCollection;
        private readonly IControlRepository controlRepository;
        private readonly IPrefabsRepository prefabsRepository;

        public ApplicationRepository(ILogger<ApplicationRepository> logger, IMongoDbSettings dbSettings, 
            IControlRepository controlRepository, IPrefabsRepository prefabsRepository)
        {
            this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;
            this.controlRepository = Guard.Argument(controlRepository, nameof(controlRepository)).NotNull().Value;
            this.prefabsRepository = Guard.Argument(prefabsRepository, nameof(prefabsRepository)).NotNull().Value;
            
            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);
            applicationsCollection = database.GetCollection<BsonDocument>(dbSettings.ApplicationsCollectionName);
            referencesCollection = database.GetCollection<ProjectReferences>(dbSettings.ProjectReferencesCollectionName);         
        }

        ///<inheritdoc/>
        public async Task<object> FindByIdAsync(string applicationId)
        {
            Guard.Argument(applicationId).NotNull().NotEmpty();
          
            var filter = Builders<BsonDocument>.Filter.Eq(x => x["ApplicationId"], applicationId);
            //exclude _id and LastUpdated as client will fail to deserialize this in to ApplicationDescription
            var projection = Builders<BsonDocument>.Projection.Exclude("_id").Exclude("LastUpdated").Exclude("ApplicationId");
            var result = applicationsCollection.Find<BsonDocument>(filter).Project(projection);
            var document = await result.SingleOrDefaultAsync();
            return BsonTypeMapper.MapToDotNetValue(document);
        }

        ///<inheritdoc/>
        public async Task<IEnumerable<object>> GetAllApplications(string platform, DateTime laterThan)
        {
            var filter = Builders<BsonDocument>.Filter.Gt(x => x["LastUpdated"], laterThan);
            var results = await applicationsCollection.FindAsync<BsonDocument>(filter);
            List<object> applications = new List<object>();
            foreach(var application in (await results.ToListAsync()))
            {
                var supportedPlatforms = application["SupportedPlatforms"].AsBsonArray.Select(x => x.AsString);
                if(!supportedPlatforms.Contains(platform))
                {
                    continue;
                }
                applications.Add(BsonTypeMapper.MapToDotNetValue(application));
            }
            return applications;
        }
       
        ///<inheritdoc/>
        public async Task AddOrUpdate(string applicationDescriptionJson)
        {
            Guard.Argument(applicationDescriptionJson).NotNull().NotEmpty();
            if (BsonDocument.TryParse(applicationDescriptionJson, out BsonDocument document))
            {
                string applicationId = document["ApplicationDetails"]["ApplicationId"].AsString;

                //Add these extra fields while storing. However, these will be dropped off in response while retrieving
                document.Add("ApplicationId", applicationId);
                document.Add("LastUpdated", DateTime.UtcNow);

                var filter = Builders<BsonDocument>.Filter.Eq(x => x["ApplicationDetails.ApplicationId"], applicationId);

                //if document with application id already exists, replace it
                if (applicationsCollection.CountDocuments<BsonDocument>(x => x["ApplicationDetails.ApplicationId"].Equals(applicationId),
                    new CountOptions { Limit = 1 }) > 0)
                {
                    await applicationsCollection.FindOneAndReplaceAsync<BsonDocument>(filter, document);         
                    logger.LogInformation("Application : '{0}' was updated", document["ApplicationDetails"]["ApplicationName"]);
                }
                else
                {
                    await applicationsCollection.InsertOneAsync(document);  
                    logger.LogInformation("Application : '{0}' was added", document["ApplicationDetails"]["ApplicationName"]);
                }
                return;

            }
            throw new ArgumentException("Failed to parse application data in to BsonDocument");
        }

        ///<inheritdoc/>
        public async Task DeleteAsync(string applicationId)
        {
            Guard.Argument(applicationId, nameof(applicationId)).NotNull().NotEmpty();

            var applicationFilter = Builders<BsonDocument>.Filter.Eq(x => x["ApplicationId"], applicationId);          
            var application = await (applicationsCollection.Find<BsonDocument>(applicationFilter)).SingleOrDefaultAsync();
        
            //Check if application is used by any of the projects
            var referenceFilter = Builders<ProjectReferences>.Filter.ElemMatch(x => x.ControlReferences, Builders<ControlReference>.Filter.Eq(c => c.ApplicationId, applicationId))
                        | Builders<ProjectReferences>.Filter.ElemMatch(x => x.PrefabReferences, Builders<PrefabReference>.Filter.Eq(p => p.ApplicationId, applicationId));
                    
            long count = await this.referencesCollection.CountDocumentsAsync(referenceFilter);
            if (count > 0)
            {
                throw new InvalidOperationException($"Application : '{application["ApplicationDetails"]["ApplicationName"]}' is in use by one or more projects");
            }

            var updateDefinition = Builders<BsonDocument>.Update
                .Set("ApplicationDetails.IsDeleted", true)
                .Set("ApplicationDetails.LastUpdated", DateTime.UtcNow);
            await applicationsCollection.FindOneAndUpdateAsync(applicationFilter, updateDefinition);
            logger.LogInformation("Application : '{0}' was marked deleted", application["ApplicationDetails"]["ApplicationName"]);

            await this.controlRepository.DeleteAllControlsForApplicationAsync(applicationId);
            await this.prefabsRepository.DeleteAllPrefabsForApplicationAsync(applicationId);
        }
    }
}
