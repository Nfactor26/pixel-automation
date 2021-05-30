using Dawn;
using MongoDB.Driver;
using Pixel.Persistence.Core.Models;
using System;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    public class ProjectStatisticsRepository : IProjectStatisticsRepository
    {
        private readonly IMongoCollection<ProjectStatistics> projectStatistics;

        public ProjectStatisticsRepository(IMongoDbSettings dbSettings)
        {
            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);
            projectStatistics = database.GetCollection<ProjectStatistics>(dbSettings.ProjectStatisticsCollectionName);
        }

        public Task AddOrUpdateStatisticsAsync(TestResult testResult, DateTime fromTime, DateTime toTime)
        {
            throw new NotImplementedException();
        }

        public async Task<ProjectStatistics> GetProjectStatisticsByIdAsync(string projectId)
        {
            Guard.Argument(projectId).NotNull().NotEmpty();
            var result = await projectStatistics.FindAsync<ProjectStatistics>(s => s.ProjectId.Equals(projectId));
            return await result.FirstOrDefaultAsync();
        }

        public async Task<ProjectStatistics> GetProjectStatisticsByNameAsync(string projectName)
        {
            Guard.Argument(projectName).NotNull().NotEmpty();
            var result = await projectStatistics.FindAsync<ProjectStatistics>(s => s.ProjectName.Equals(projectName));
            return await result.FirstOrDefaultAsync();
        }
    }
}
