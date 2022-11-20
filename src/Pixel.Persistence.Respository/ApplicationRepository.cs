using Dawn;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly IMongoCollection<BsonDocument> applicationsCollection;

        public ApplicationRepository(IMongoDbSettings dbSettings)
        {
            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);
            applicationsCollection = database.GetCollection<BsonDocument>(dbSettings.ApplicationsCollectionName);
        }

        ///<inheritdoc/>
        public async Task<object> GetApplication(string applicationId)
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
        public async Task<IEnumerable<object>> GetAllApplications(DateTime laterThan)
        {
            var filter = Builders<BsonDocument>.Filter.Gt(x => x["LastUpdated"], laterThan);
            var results = await applicationsCollection.FindAsync<BsonDocument>(filter);
            List<object> applications = new List<object>();
            foreach(var application in (await results.ToListAsync()))
            {
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
                document.Add("LastUpdated", DateTime.Now.ToUniversalTime());

                var filter = Builders<BsonDocument>.Filter.Eq(x => x["ApplicationDetails.ApplicationId"], applicationId);

                //if document with application id already exists, replace it
                if (applicationsCollection.CountDocuments<BsonDocument>(x => x["ApplicationDetails.ApplicationId"].Equals(applicationId),
                    new CountOptions { Limit = 1 }) > 0)
                {
                    await applicationsCollection.FindOneAndReplaceAsync<BsonDocument>(filter, document);                  
                }
                else
                {
                    await applicationsCollection.InsertOneAsync(document);                  
                }
                return;

            }
            throw new ArgumentException("Failed to parse application data in to BsonDocument");
        }
    }
}
