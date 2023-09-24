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
    public class TestStatisticsRepository : ITestStatisticsRepository
    {
        private readonly ITestResultsRepository testResultsRepository;
        private readonly ITestSessionRepository testSessionRepository;
        private readonly IMongoCollection<TestStatistics> testStatistics;

        public TestStatisticsRepository(IMongoDbSettings dbSettings, ITestSessionRepository testSessionRepository, ITestResultsRepository testResultsRepository)
        {
            Guard.Argument(dbSettings).NotNull();
            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DatabaseName);
            this.testStatistics = database.GetCollection<TestStatistics>(dbSettings.TestStatisticsCollectionName);
            this.testSessionRepository = Guard.Argument(testSessionRepository).NotNull().Value;
            this.testResultsRepository = Guard.Argument(testResultsRepository).NotNull().Value;
        }

        public async Task AddOrUpdateStatisticsAsync(string sessionId)
        {
            Guard.Argument(sessionId).NotNull().NotEmpty();
            var session = await testSessionRepository.GetTestSessionAsync(sessionId);
            var testsInSession = await testResultsRepository.GetTestResultsForSessionAsync(sessionId);
            var groupedTests = testsInSession.GroupBy(t => t.TestId);
            int month = session.SessionStartTime.Month;
            int year = session.SessionStartTime.Year;
            int lastDayOfMonth = DateTime.DaysInMonth(year, month);
            int firstDayOfMonth = 1;
            foreach (var group in groupedTests)
            {
                await this.AddOrUpdateStatisticsAsync(group.ToList(), session.ProjectVersion, new DateTime(year, month, firstDayOfMonth, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(year, month, lastDayOfMonth, 23, 59, 59, DateTimeKind.Utc));
            }           
        }

        public async Task AddOrUpdateStatisticsAsync(IEnumerable<TestResult> testResults, string projectVersion, DateTime fromTime, DateTime toTime)
        {
            Guard.Argument(testResults).NotNull().NotEmpty();
            var first = testResults.First();
            if(!testResults.All(t => t.TestId.Equals(first.TestId)))
            {
                throw new ArgumentException("All tests must have same Id");
            }

            //Create a TestStatistics entry if not exists
            TestStatistics statistics = default;
            bool exists = testStatistics.AsQueryable().Any(t => t.TestId.Equals(first.TestId));
            if (!exists)
            {
                statistics = new TestStatistics()
                {
                    ProjectId = first.ProjectId,
                    ProjectName = first.ProjectName,
                    TestId = first.TestId,
                    TestName = first.TestName,
                    FixtureId = first.FixtureId,
                    FixtureName = first.FixtureName
                };
                await testStatistics.InsertOneAsync(statistics);
            }
            else
            {
               var result = await testStatistics.FindAsync(t => t.TestId.Equals(first.TestId), new FindOptions<TestStatistics, TestStatistics>() { Limit = 1 });
               statistics = await result.FirstOrDefaultAsync();
            }

         
            if(!statistics.MonthlyStatistics.Any(s => s.FromTime.Equals(fromTime.ToUniversalTime()) && s.ToTime.Equals(toTime.ToUniversalTime())
                && s.ProjectVersion.Equals(projectVersion)))
            {
                statistics.MonthlyStatistics.Add(new TestExecutionStatistics(projectVersion, fromTime, toTime) { MinExecutionTime = 1000 });
            }

            var passed = testResults.Where(t => t.Result.Equals(TestStatus.Success)) ?? Enumerable.Empty<TestResult>();
            var failed = testResults.Where(t => t.Result.Equals(TestStatus.Failed)) ?? Enumerable.Empty<TestResult>();
           
            var executionStatistics = statistics.MonthlyStatistics.First(s => s.FromTime.Equals(fromTime.ToUniversalTime()) && s.ToTime.Equals(toTime.ToUniversalTime())
                && s.ProjectVersion.Equals(projectVersion));
            executionStatistics.NumberOfTimesExecuted += testResults.Count();
            executionStatistics.NumberOfTimesPassed += passed.Count();
            executionStatistics.NumberOfTimesFailed += failed.Count();
            executionStatistics.TotalExecutionTime += passed.Sum(t => t.ExecutionTime);
            if (testResults.Any(t => t.Result.Equals(TestStatus.Success)))
            {
                executionStatistics.MinExecutionTime = Math.Min(passed.Min(t => t.ExecutionTime), executionStatistics.MinExecutionTime);
                executionStatistics.MaxExecutionTime = Math.Max(passed.Max(t => t.ExecutionTime), executionStatistics.MaxExecutionTime);
            }         

            await testStatistics.ReplaceOneAsync(t => t.TestId.Equals(first.TestId), statistics);
            //Todo : Can this be done as batch update
            foreach(var test in testResults)
            {
                await testResultsRepository.MarkTestProcessedAsync(test.SessionId, test.TestId);
            }

            //var filter = Builders<TestStatistics>.Filter.And(Builders<TestStatistics>.Filter.Where(t => t.TestId == first.TestId),
            //             Builders<TestStatistics>.Filter.ElemMatch(t => t.MonthlyStatistics, s => s.FromTime.Equals(fromTime.ToUniversalTime())
            //             && s.ToTime.Equals(toTime.ToUniversalTime())));
            //exists = await testStatistics.CountDocumentsAsync(filter, new CountOptions() { Limit = 1 }) == 1;

            //if (!exists)
            //{
            //    var testExecStatistics = new TestExecutionStatistics(fromTime, toTime) { MinExecutionTime = 1000 };
            //    var update = Builders<TestStatistics>.Update.Push(x => x.MonthlyStatistics, testExecStatistics);
            //    await testStatistics.FindOneAndUpdateAsync(Builders<TestStatistics>.Filter.Where(t => t.TestId.Equals(first.TestId)), update);
            //}         
            //var updateBuilder = Builders<TestStatistics>.Update;
            //var statsUpdate = updateBuilder.Inc(t => t.MonthlyStatistics[-1].NumberOfTimesExecuted, testResults.Count())
            //    .Inc(t => t.MonthlyStatistics[-1].NumberOfTimesPassed, testResults.Where(t => t.Result.Equals(TestState.Success))?.Count() ?? 0)
            //    .Inc(t => t.MonthlyStatistics[-1].NumberOfTimesFailed, testResults.Where(t => t.Result.Equals(TestState.Failed))?.Count() ?? 0);
            //if(testResults.Any(t => t.Result.Equals(TestState.Success)))
            //{
            //   statsUpdate =  statsUpdate.Min(t => t.MonthlyStatistics[-1].MinExecutionTime, testResults.Where(t => t.Result.Equals(TestState.Success))?.Min(t => t.ExecutionTime))
            //    .Max(t => t.MonthlyStatistics[-1].MaxExecutionTime, testResults.Where(t => t.Result.Equals(TestState.Success)).Max(t => t.ExecutionTime));
            //}

            //await testStatistics.FindOneAndUpdateAsync(filter, statsUpdate);

            ////update unique failures
            //foreach(var test in testResults.Where(t => t.Result.Equals(TestState.Failed)))
            //{
            //    var failureFilter = Builders<TestStatistics>.Filter.And(Builders<TestStatistics>.Filter.Where(t => t.TestId == first.TestId),
            //           Builders<TestStatistics>.Filter.ElemMatch(t => t.UniqueFailures, f => f.Exception.Equals(test.FailureDetails.Exception) && f.Message.Equals(test.FailureDetails.Message)));
            //    await testStatistics.FindOneAndUpdateAsync(failureFilter, Builders<TestStatistics>.Update.Push(x => x.UniqueFailures, test.FailureDetails) , new FindOneAndUpdateOptions<TestStatistics, FailureDetails>() { IsUpsert = true });
            //}         

        }

        public async Task<TestStatistics> GetTestStatisticsByIdAsync(string testId)
        {
            Guard.Argument(testId).NotNull().NotEmpty();
            var result = await testStatistics.FindAsync<TestStatistics>(s => s.TestId.Equals(testId));
            return await result.FirstOrDefaultAsync();
        }

        public async Task<TestStatistics> GetTestStatisticsByNameAsync(string testName)
        {
            Guard.Argument(testName).NotNull().NotEmpty();
            var result = await testStatistics.FindAsync<TestStatistics>(s => s.TestName.Equals(testName));
            return await result.FirstOrDefaultAsync();
        }
    }
}
