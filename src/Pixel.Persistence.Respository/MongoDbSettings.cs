namespace Pixel.Persistence.Respository
{
    public interface IMongoDbSettings
    {
        string ConnectionString { get; set; }

        string DatabaseName { get; set; }

        string ApplicationsCollectionName { get; set; }

        string ControlsCollectionName { get; set; }

        string AutomationProjectCollectionName { get; set; }

        string ProjectReferencesCollectionName { get; set; }

        string FixturesCollectionName { get; set; }

        string TestsCollectionName { get; set; }

        string TestDataCollectionName { get; set; }

        string ProjectFilesBucketName { get; set; }

        string ImagesBucketName { get; set; }

        string ProjectsBucketName { get; set; }

        string PrefabProjectsCollectionName { get; set; }

        string PrefabFilesBucketName { get; set; }

        string TemplatesCollectionName { get; set; }

        string TemplateHandlersName { get; set; }

        string SessionsCollectionName { get; set; }

        string TestResultsCollectionName { get; set; }

        string TraceImagesBucketName { get; set; }

        string TestStatisticsCollectionName { get; set; }

        string ProjectStatisticsCollectionName { get; set; }

        string ComposeFilesBucketName { get; set; }

    }

    public class MongoDbSettings : IMongoDbSettings
    {
        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }

        public string ApplicationsCollectionName { get; set; }

        public string ControlsCollectionName { get; set; }

        public string AutomationProjectCollectionName { get; set; }

        public string ProjectReferencesCollectionName { get; set; }

        public string ProjectStatisticsCollectionName { get; set; }

        public string FixturesCollectionName { get; set; }

        public string TestsCollectionName { get; set; }

        public string TestDataCollectionName { get; set; }

        public string ProjectFilesBucketName { get; set; }

        public string PrefabFilesBucketName { get; set; }

        public string ImagesBucketName { get; set; }

        public string ProjectsBucketName { get; set; }

        public string PrefabProjectsCollectionName { get; set; }

        public string TemplatesCollectionName { get; set; }

        public string TemplateHandlersName { get; set; }

        public string SessionsCollectionName { get; set; }

        public string TestResultsCollectionName { get; set; }

        public string TraceImagesBucketName { get; set; }

        public string TestStatisticsCollectionName { get; set; }

        public string ComposeFilesBucketName { get; set; }

    }

}
