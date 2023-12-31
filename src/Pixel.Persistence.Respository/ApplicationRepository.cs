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

        public async Task<bool> CheckApplicationExists(string applicationId)
        {
            Guard.Argument(applicationId, nameof(applicationId)).NotNull().NotWhiteSpace();
            var filter = Builders<BsonDocument>.Filter.Eq(x => x["ApplicationId"], applicationId);
            var count = await applicationsCollection.CountDocumentsAsync(filter);
            return count > 0;
        }


        async Task<BsonDocument> FindApplicationByIdAsync(string applicationId)
        {
            Guard.Argument(applicationId, nameof(applicationId)).NotNull().NotWhiteSpace();
            var filter = Builders<BsonDocument>.Filter.Eq(x => x["ApplicationId"], applicationId);
            //exclude field not known to client.
            var projection = Builders<BsonDocument>.Projection.Exclude("_id").Exclude("LastUpdated").Exclude("ApplicationId").Exclude("Revision");
            var result = applicationsCollection.Find<BsonDocument>(filter).Project(projection);
            var document = await result.SingleOrDefaultAsync();
            return document;
        }

        ///<inheritdoc/>
        public async Task<object> FindByIdAsync(string applicationId)
        {           
            var document = await FindApplicationByIdAsync(applicationId);
            return BsonTypeMapper.MapToDotNetValue(document);
        }

        ///<inheritdoc/>
        public async Task<IEnumerable<object>> GetAllApplications(string platform, DateTime laterThan)
        {
            var filter = Builders<BsonDocument>.Filter.Gt(x => x["LastUpdated"], laterThan);
            var results = await applicationsCollection.FindAsync<BsonDocument>(filter);
            List<object> applications = new List<object>();
            foreach (var application in (await results.ToListAsync()))
            {
                var supportedPlatforms = application["SupportedPlatforms"].AsBsonArray.Select(x => x.AsString);
                if (!supportedPlatforms.Contains(platform))
                {
                    continue;
                }
                applications.Add(BsonTypeMapper.MapToDotNetValue(application));
            }
            return applications;
        }
       
        public async Task AddApplication(object applicationDescription)
        {
            Guard.Argument(applicationDescription, nameof(applicationDescription)).NotNull();
            if (BsonDocument.TryParse(applicationDescription.ToString(), out BsonDocument document))
            {
                string applicationId = document["ApplicationDetails"]["ApplicationId"].AsString;               
                if (await CheckApplicationExists(applicationId))
                {
                    throw new InvalidOperationException($"Application with Id : {applicationId} already exists");
                }

                //Add these extra fields while storing. However, these will be dropped off in response while retrieving
                document.Add("ApplicationId", applicationId);
                document.Add("LastUpdated", DateTime.UtcNow);
                document.Add("Revision", 1);

                await applicationsCollection.InsertOneAsync(document);
                logger.LogInformation("Application : '{0}' of type '{1}' was added", document["ApplicationDetails"]["ApplicationName"]
                    , document["ApplicationType"]);
                return;
            }
            throw new ArgumentException("Failed to parse application description data in to BsonDocument");
        }

        ///<inheritdoc/>
        public async Task UpdateApplication(object applicationDescription)
        {
            Guard.Argument(applicationDescription, nameof(applicationDescription)).NotNull();
            if (BsonDocument.TryParse(applicationDescription.ToString(), out BsonDocument document))
            {
                string applicationId = document["ApplicationDetails"]["ApplicationId"].AsString;              
                if (!await CheckApplicationExists(applicationId))
                {
                    throw new InvalidOperationException($"Application with Id : {applicationId} doesn't exist");
                }

                var filter = Builders<BsonDocument>.Filter.Eq(x => x["ApplicationId"], applicationId);
                var updateDefinition = Builders<BsonDocument>.Update
                 .Set(x => x["ApplicationDetails"], document["ApplicationDetails"])
                 .Set(x => x["IsDeleted"], document["IsDeleted"])
                 .Set(x => x["LastUpdated"], DateTime.UtcNow)
                 .Inc(x => x["Revision"], 1);

                var result = await applicationsCollection.FindOneAndUpdateAsync(filter, updateDefinition);
                logger.LogInformation("Application : '{0}' was updated with result : '{1}'", document["ApplicationDetails"]["ApplicationName"], result);
                return;
            }
            throw new ArgumentException("Failed to parse application description data in to BsonDocument");

        }

        ///<inheritdoc/>
        public async Task DeleteAsync(string applicationId)
        {
            Guard.Argument(applicationId, nameof(applicationId)).NotNull().NotEmpty();
                      
            //Check if application is used by any of the projects
            var referenceFilter = Builders<ProjectReferences>.Filter.ElemMatch(x => x.ControlReferences, Builders<ControlReference>.Filter.Eq(c => c.ApplicationId, applicationId))
                        | Builders<ProjectReferences>.Filter.ElemMatch(x => x.PrefabReferences, Builders<PrefabReference>.Filter.Eq(p => p.ApplicationId, applicationId));
                    
            long count = await this.referencesCollection.CountDocumentsAsync(referenceFilter);
            if (count > 0)
            {
                throw new InvalidOperationException($"Application : '{applicationId}' is in use by one or more projects");
            }

            var applicationFilter = Builders<BsonDocument>.Filter.Eq(x => x["ApplicationId"], applicationId);
            var updateDefinition = Builders<BsonDocument>.Update
                .Set(x => x["IsDeleted"], true)
                .Set(x => x["LastUpdated"], DateTime.UtcNow)
                .Inc(x => x["Revision"], 1);
            await applicationsCollection.FindOneAndUpdateAsync(applicationFilter, updateDefinition);
            logger.LogInformation("Application : '{0}' was marked deleted", applicationId);

            await this.controlRepository.DeleteAllControlsForApplicationAsync(applicationId);
            await this.prefabsRepository.DeleteAllPrefabsForApplicationAsync(applicationId);
        }

        ///<inheritdoc/>
        public async Task AddScreenAsync(string applicationId, ApplicationScreen applicationScreen)
        {
            Guard.Argument(applicationId, nameof(applicationId)).NotNull().NotWhiteSpace();
            Guard.Argument(applicationScreen, nameof(applicationScreen)).NotNull();
            if(await ScreenExists(applicationId, applicationScreen.ScreenName))
            {
                throw new ArgumentException($"Application Screen already exists with name : '{applicationScreen.ScreenName}' for application : '{applicationId}'");
            }            
            var applicationFilter = Builders<BsonDocument>.Filter.Eq(x => x["ApplicationId"], applicationId);
            var updateDefinition = Builders<BsonDocument>.Update
              .Push(x => x["Screens"].AsBsonArray, applicationScreen.ToBsonDocument());
            await applicationsCollection.FindOneAndUpdateAsync(applicationFilter, updateDefinition);
            logger.LogInformation("Added screen : '{0}' to application : '{1}'", applicationScreen.ScreenName, applicationId);
        }

        ///<inheritdoc/>
        public async Task RenameScreenAsync(string applicationId, string screenId, string newScreenName)
        {
            Guard.Argument(applicationId, nameof(applicationId)).NotNull().NotWhiteSpace();
            Guard.Argument(screenId, nameof(screenId)).NotNull().NotWhiteSpace();
            Guard.Argument(newScreenName, nameof(newScreenName)).NotNull().NotWhiteSpace();
            string currentScreeName = await GetScreenNameFromScreenId(applicationId, screenId);
            var filter = Builders<BsonDocument>.Filter.Eq("ApplicationId", applicationId);
            var updateDefinition = Builders<BsonDocument>.Update.Set("Screens.$[screen].ScreenName", newScreenName);
            UpdateOptions updateOptions = new UpdateOptions
            {
                ArrayFilters = new[]
                {
                    new BsonDocumentArrayFilterDefinition<BsonDocument>(
                        new BsonDocument("screen.ScreenId", screenId)
                        )
                    }
            };
            var result = await applicationsCollection.UpdateOneAsync(filter, updateDefinition, updateOptions);
            logger.LogInformation("Renamed screen : '{0}' to : '{1}'. Result is {2}", currentScreeName, newScreenName, result);
        }       

        async Task<string> GetScreenNameFromScreenId(string applicationId, string screenId)
        {
            var application = await FindApplicationByIdAsync(applicationId);
            foreach (var screen in application["Screens"].AsBsonArray)
            {
                if (screen["ScreenId"].AsString.Equals(screenId))
                {
                    return screen["ScreenId"].ToString();
                }
            }
            return string.Empty;         
        }

        async Task<bool> ScreenExists(string applicationId, string screenName)
        {
            var filter = Builders<BsonDocument>.Filter.Eq(x => x["ApplicationId"], applicationId) & Builders<BsonDocument>.Filter.Eq(x => x["Screens"], screenName);
            var count = await applicationsCollection.CountDocumentsAsync(filter);
            return count > 0;
        }

        ///<inheritdoc/>
        public async Task AddControlToScreen(string applicationId, string controlId, string screenId)
        {
            Guard.Argument(applicationId, nameof(applicationId)).NotNull().NotWhiteSpace();
            Guard.Argument(screenId, nameof(screenId)).NotNull().NotWhiteSpace();
            Guard.Argument(controlId, nameof(controlId)).NotNull().NotWhiteSpace();
         
            //Control will already be associated with a screen when creating a revision of control
            string currentScreen = await GetScreenForControl(applicationId, controlId);
            if(string.IsNullOrEmpty(currentScreen))
            {
                var filter = Builders<BsonDocument>.Filter.Eq("ApplicationId", applicationId);
                var updateDefinition = Builders<BsonDocument>.Update.Push("Screens.$[screen].AvailableControls", controlId);
                UpdateOptions updateOptions = new UpdateOptions
                {
                    ArrayFilters = new[]
                    {
                    new BsonDocumentArrayFilterDefinition<BsonDocument>(
                        new BsonDocument("screen.ScreenId", screenId)
                        )
                    }
                };
                var result = await applicationsCollection.UpdateOneAsync(filter, updateDefinition, updateOptions);
                logger.LogInformation("Control : '{0}' was added to screen : '{1}' for application : '{2}'. Update result is : '{2}'", controlId, screenId, applicationId, result);
                return;
            }
            logger.LogWarning("Control : '{0}' already belongs to screen : '{1}'",  controlId, currentScreen);
        }

        ///<inheritdoc/>
        public async Task DeleteControlFromScreen(string applicationId, string controlId, string screenId)
        {
            Guard.Argument(applicationId, nameof(applicationId)).NotNull().NotWhiteSpace();
            Guard.Argument(screenId, nameof(screenId)).NotNull().NotWhiteSpace();
            Guard.Argument(controlId, nameof(controlId)).NotNull().NotWhiteSpace();

            string currentScreen = await GetScreenForControl(applicationId, controlId);
            if(!string.IsNullOrEmpty(currentScreen))
            {
                var filter = Builders<BsonDocument>.Filter.Eq("ApplicationId", applicationId);
                var updateDefinition = Builders<BsonDocument>.Update.Pull("Screens.$[screen].AvailableControls", controlId);
                UpdateOptions updateOptions = new UpdateOptions
                {
                    ArrayFilters = new[]
                    {
                    new BsonDocumentArrayFilterDefinition<BsonDocument>(
                        new BsonDocument("screen.ScreenId", screenId)
                        )
                }
                };
                var result = await applicationsCollection.UpdateOneAsync(filter, updateDefinition, updateOptions);
                logger.LogInformation("Control : '{0}' was deleted from screen : '{1}' for application : '{2}'. Update result is : '{2}'", controlId, screenId, applicationId, result);
                return;
            }
            logger.LogWarning("Control : '{0}' doesn't belong to any of the screens for application : '{1}'. Delete operation failed.", controlId, applicationId);
        }

        ///<inheritdoc/>
        public async Task MoveControlToScreen(string applicationId, string controlId, string targetScreenId)
        {
            Guard.Argument(applicationId, nameof(applicationId)).NotNull().NotWhiteSpace();
            Guard.Argument(targetScreenId, nameof(targetScreenId)).NotNull().NotWhiteSpace();
            Guard.Argument(controlId, nameof(controlId)).NotNull().NotWhiteSpace();       
          
            string currentScreen = await GetScreenForControl(applicationId, controlId);
            if(!string.IsNullOrEmpty(currentScreen))
            {
                await DeleteControlFromScreen(applicationId, controlId, currentScreen);
            }
            await AddControlToScreen(applicationId, controlId, targetScreenId);
        }

        public async Task<string> GetScreenForControl(string applicationId, string controlId)
        {
            var application = await FindApplicationByIdAsync(applicationId);
            foreach(var screen in application["Screens"].AsBsonArray)
            {
                if (screen["AvailableControls"].AsBsonArray.Contains(controlId))
                {
                    return screen["ScreenId"].ToString();
                }
            }
            return string.Empty;
        }

        ///<inheritdoc/>
        public async Task AddPrefabToScreen(string applicationId, string prefabId, string screenId)
        {
            Guard.Argument(applicationId, nameof(applicationId)).NotNull().NotWhiteSpace();
            Guard.Argument(screenId, nameof(screenId)).NotNull().NotWhiteSpace();
            Guard.Argument(prefabId, nameof(prefabId)).NotNull().NotWhiteSpace();
         
            var filter = Builders<BsonDocument>.Filter.Eq("ApplicationId", applicationId);
            var updateDefinition = Builders<BsonDocument>.Update.Push("Screens.$[screen].AvailablePrefabs", prefabId);
            UpdateOptions updateOptions = new UpdateOptions
            {
                ArrayFilters = new[]
                {
                    new BsonDocumentArrayFilterDefinition<BsonDocument>(
                        new BsonDocument("screen.ScreenId", screenId)
                        )
                }
            };
            var result = await applicationsCollection.UpdateOneAsync(filter, updateDefinition, updateOptions);
            logger.LogInformation("Prefab : '{0}' was added to screen : '{1}' for application : '{2}'. Update result is : '{2}'", prefabId, screenId, applicationId, result);
        }

        ///<inheritdoc/>
        public async Task DeletePrefabFromScreen(string applicationId, string prefabId, string screenId)
        {
            Guard.Argument(applicationId, nameof(applicationId)).NotNull().NotWhiteSpace();
            Guard.Argument(screenId, nameof(screenId)).NotNull().NotWhiteSpace();
            Guard.Argument(prefabId, nameof(prefabId)).NotNull().NotWhiteSpace();

            string currentScreen = await GetScreenForControl(applicationId, prefabId);
            if (!string.IsNullOrEmpty(currentScreen))
            {
                var filter = Builders<BsonDocument>.Filter.Eq("ApplicationId", applicationId);
                var updateDefinition = Builders<BsonDocument>.Update.Pull("Screens.$[screen].AvailablePrefabs", prefabId);
                UpdateOptions updateOptions = new UpdateOptions
                {
                    ArrayFilters = new[]
                    {
                    new BsonDocumentArrayFilterDefinition<BsonDocument>(
                        new BsonDocument("screen.ScreenId", screenId)
                        )
                }
                };
                var result = await applicationsCollection.UpdateOneAsync(filter, updateDefinition, updateOptions);
                logger.LogInformation("Prefab : '{0}' was deleted from screen : '{1}' for application : '{2}'. Update result is : '{2}'", prefabId, screenId, applicationId, result);
                return;
            }
            logger.LogWarning("Prefaab : '{0}' doesn't belong to any of the screens for application : '{1}'. Delete operation failed.", prefabId, applicationId);
        }

        ///<inheritdoc/>
        public async Task MovePrefabToScreen(string applicationId, string prefabId, string targetScreenId)
        {
            Guard.Argument(applicationId, nameof(applicationId)).NotNull().NotWhiteSpace();
            Guard.Argument(targetScreenId, nameof(targetScreenId)).NotNull().NotWhiteSpace();
            Guard.Argument(prefabId, nameof(prefabId)).NotNull().NotWhiteSpace();

            string currentScreen = await GetScreenForPrefab(applicationId, prefabId);
            if (!string.IsNullOrEmpty(currentScreen))
            {
                await DeletePrefabFromScreen(applicationId, prefabId, currentScreen);
            }
            await AddPrefabToScreen(applicationId, prefabId, targetScreenId);
        }

        ///<inheritdoc/>
        public async Task<string> GetScreenForPrefab(string applicationId, string prefabId)
        {
            var application = await FindApplicationByIdAsync(applicationId);
            foreach (var screen in application["Screens"].AsBsonArray)
            {
                if (screen["AvailablePrefabs"].AsBsonArray.Contains(prefabId))
                {
                    return screen["ScreenId"].ToString();
                }
            }
            return string.Empty;
        }

    }
}
