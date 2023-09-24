using Dawn;
using MongoDB.Driver;
using Pixel.Persistence.Core.Enums;
using Pixel.Persistence.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Persistence.Respository
{
    public class ProjectStatisticsRepository : IProjectStatisticsRepository
    {
        private readonly ITestResultsRepository testResultsRepository;
        private readonly ITestSessionRepository testSessionRepository;      
        private readonly IMongoCollection<ProjectStatistics> projectStatisticsCollection;     

        public ProjectStatisticsRepository(IMongoDbSettings dbSettings, ITestSessionRepository testSessionRepository, ITestResultsRepository testResultsRepository)
        {
            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);
          
            this.projectStatisticsCollection = database.GetCollection<ProjectStatistics>(dbSettings.ProjectStatisticsCollectionName);          
            this.testSessionRepository = Guard.Argument(testSessionRepository).NotNull().Value;
            this.testResultsRepository = Guard.Argument(testResultsRepository).NotNull().Value;
        }


        public async Task<ProjectStatistics> GetProjectStatisticsByIdAsync(string projectId)
        {
            Guard.Argument(projectId).NotNull().NotEmpty();
            var result = await projectStatisticsCollection.FindAsync<ProjectStatistics>(s => s.ProjectId.Equals(projectId));
            return await result.FirstOrDefaultAsync();
        }

        public async Task<ProjectStatistics> GetProjectStatisticsByNameAsync(string projectName)
        {
            Guard.Argument(projectName).NotNull().NotEmpty();
            var result = await projectStatisticsCollection.FindAsync<ProjectStatistics>(s => s.ProjectName.Equals(projectName));
            return await result.FirstOrDefaultAsync();
        }

        public async Task AddOrUpdateStatisticsAsync(string sessionId)
        {
            Guard.Argument(sessionId).NotNull().NotEmpty();
            var session = await testSessionRepository.GetTestSessionAsync(sessionId);
            var testsInSession = await testResultsRepository.GetTestResultsForSessionAsync(sessionId);         
          
            int month = session.SessionStartTime.Month;
            int year = session.SessionStartTime.Year;
            int lastDayOfMonth = DateTime.DaysInMonth(year, month);
            int firstDayOfMonth = 1;
            var fromTime = new DateTime(year, month, firstDayOfMonth, 0, 0, 0, DateTimeKind.Utc);
            var toTime = new DateTime(year, month, lastDayOfMonth, 23, 59, 59, DateTimeKind.Utc);

            var projectStatistics = await GetProjectStatisticsByIdAsync(session.ProjectId);
            if(projectStatistics == null)
            {
                projectStatistics = new ProjectStatistics()
                {
                    ProjectId = session.ProjectId,
                    ProjectName = session.ProjectName,
                    MonthlyStatistics = new List<ProjectExecutionStatistics>()
                    {
                        new ProjectExecutionStatistics(session.ProjectVersion, fromTime, toTime)
                    }
                };
                await CreateAsync(projectStatistics);
            }

            if (!projectStatistics.MonthlyStatistics.Any(s => s.FromTime.Equals(fromTime.ToUniversalTime()) && 
                        s.ToTime.Equals(toTime.ToUniversalTime()) && s.ProjectVersion.Equals(session.ProjectVersion)))
            {
                projectStatistics.MonthlyStatistics.Add(new ProjectExecutionStatistics(session.ProjectVersion, fromTime, toTime));
            }

            //we need to update the execution statistics for the session (month, year)
            var executionStatistics = projectStatistics.MonthlyStatistics.FirstOrDefault(s => s.FromTime.Equals(fromTime.ToUniversalTime()) && s.ToTime.Equals(toTime.ToUniversalTime())
                && s.ProjectVersion.Equals(session.ProjectVersion));
         
            var passed = testsInSession.Where(t => t.Result.Equals(TestStatus.Success)) ?? Enumerable.Empty<TestResult>();
            var failed = testsInSession.Where(t => t.Result.Equals(TestStatus.Failed)) ?? Enumerable.Empty<TestResult>();
            executionStatistics.NumberOfTestsExecuted += testsInSession.Count();
            executionStatistics.NumberOfTestsPassed += passed.Count();
            executionStatistics.NumberOfTestsFailed += failed.Count();
            executionStatistics.TotalExecutionTime += passed.Sum(t => t.ExecutionTime);

            await ReplaceAsync(projectStatistics);

        }

        public async Task CreateAsync(ProjectStatistics statistics)
        {
            Guard.Argument(statistics, nameof(statistics)).NotNull();
            var exists = await projectStatisticsCollection.CountDocumentsAsync<ProjectStatistics>(s => s.ProjectId.Equals(statistics.ProjectId)) == 1;          
            if(exists)
            {
                throw new ArgumentException($"ProjectStatistics with projectId : {statistics.ProjectId} already exists");
            }
            await projectStatisticsCollection.InsertOneAsync(statistics);
        }

        public async Task ReplaceAsync(ProjectStatistics statistics)
        {
            Guard.Argument(projectStatisticsCollection).NotNull();
            var result = await projectStatisticsCollection.ReplaceOneAsync(p => p.ProjectId.Equals(statistics.ProjectId), statistics);
        }    
    }
}
