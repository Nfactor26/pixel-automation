using System;
using System.Collections.Generic;
using System.Text;

namespace Pixel.Persistence.Respository
{
    public interface IMongoDbSettings
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }        
        string ApplicationsCollectionName { get; set; }
        string ApplicationsBucketName { get; set; }
        string ControlsCollectionName { get; set; }
        string ControlsBucketName { get; set; }
        string ImagesBucketName { get; set; }
        string ProjectsBucketName { get; set; }
        string PrefabsBucketName { get; set; }
        string SessionsCollectionName { get; set; }
        string TestResultsCollectionName { get; set; }
        string TestStatisticsCollectionName { get; set; }
        string ProjectStatisticsCollectionName { get; set; }

    }

    public class MongoDbSettings : IMongoDbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }      
        public string ApplicationsCollectionName { get; set; }
        public string ApplicationsBucketName { get; set; }
        public string ControlsCollectionName { get; set; }
        public string ControlsBucketName { get; set; }
        public string ImagesBucketName { get; set; }
        public string ProjectsBucketName { get; set; }
        public string PrefabsBucketName { get; set; }
        public string SessionsCollectionName { get; set; }
        public string TestResultsCollectionName { get; set; }
        public string TestStatisticsCollectionName { get; set; }
        public string ProjectStatisticsCollectionName { get; set; }
    }

}
