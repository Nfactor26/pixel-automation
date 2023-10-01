using Dawn;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Respository;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Services;

/// <summary>
/// Purge data service is a hosted service that is responsible for cleaning up
/// test sessions along with it's related data ( test result + trace images) after a certain number of
/// days have passed since test session was executed. Number of days to retain is configured using the
/// <see cref="RetentionPolicy"/>
/// </summary>
public class PurgeDataService : IHostedService, IDisposable
{
    private readonly ILogger logger;
    private readonly IMongoClient mongoClient;  
    private readonly RetentionPolicy retentionPolicy;

    private readonly IMongoCollection<TestSession> sessionsCollection;
    private readonly IMongoCollection<TestResult> testResults;
    private readonly IGridFSBucket traceImagesBucket;

    private Timer timer;

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="dbSettings"></param>
    /// <param name="retentionPolicy"></param>
    public PurgeDataService(ILogger<PurgeDataService> logger, IOptions<MongoDbSettings> dbSettings, IOptions<RetentionPolicy> retentionPolicy)
    {
        this.logger = Guard.Argument(logger).NotNull().Value;           
        this.retentionPolicy = Guard.Argument(retentionPolicy).NotNull().Value.Value;
        var mongoDbSettings = Guard.Argument(dbSettings).NotNull().Value.Value;
       
        this.mongoClient = new MongoClient(mongoDbSettings.ConnectionString);
        var database = mongoClient.GetDatabase(mongoDbSettings.DatabaseName);
        sessionsCollection = database.GetCollection<TestSession>(mongoDbSettings.SessionsCollectionName);
        testResults = database.GetCollection<TestResult>(mongoDbSettings.TestResultsCollectionName);
        traceImagesBucket = new GridFSBucket(database, new GridFSBucketOptions
        {
            BucketName = mongoDbSettings.TraceImagesBucketName
        });
        logger.LogInformation("Retention policy : {@RetentionPolicy}", this.retentionPolicy);
    }

    ///<inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Purge data background service is running now.");
        timer = new Timer(PurgeData, null, TimeSpan.FromHours(24), TimeSpan.FromHours(24));

        #if DEBUG
        timer.Change(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        #endif

        return Task.CompletedTask;
    }

    /// <summary>
    /// Cleanup TestSessions that are older than n number of days as configured in RetentionPolicy
    /// </summary>
    /// <param name="state"></param>
    private void PurgeData(object state)
    {
        Task workerTask = new Task(async () =>
        {
            try
            {
                //using (var session = await mongoClient.StartSessionAsync())
                //{
                //    try
                //    {
                //        session.StartTransaction();

                //        await session.CommitTransactionAsync();
                //    }
                //    catch (Exception ex)
                //    {
                //        logger.LogError(ex, "There was an error while trying to cleanup older test session data");
                //        await session.AbortTransactionAsync();
                //    }
                //}
                await DeleteSesssions(DateTime.Now.Subtract(TimeSpan.FromDays(retentionPolicy.TestSessionRetentionPeriod)));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "There was an error while trying to cleanup older test session data");               
            }
        });
        workerTask.Start();
    }

    /// <summary>
    /// Find and delete TestSessions that are older than the specified DateTime
    /// </summary>
    /// <param name="olderThan"></param>
    /// <returns></returns>
    async Task DeleteSesssions(DateTime olderThan)
    {
        logger.LogInformation("Clean up all test sessions before : {0}", olderThan);
        var filterBuilder = Builders<TestSession>.Filter;
        var filter = filterBuilder.Lt(t => t.CreatedAt, olderThan);
        var all = await sessionsCollection.FindAsync(filter);
        await all.ForEachAsync(async session =>
        {
            await sessionsCollection.DeleteOneAsync(s => s.Id.Equals(session.Id));
            await DeleteSessionData(session.Id);
            logger.LogInformation(" {@TestSession} was deleted", session);
        });
        logger.LogInformation("Clean up of  test sessions completed");
    }

    /// <summary>
    /// Delete test results and trace images for a given test session Id.
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    async Task DeleteSessionData(string sessionId)
    {
        var filter = Builders<TestResult>.Filter.Eq(t => t.SessionId, sessionId);
        var result = await this.testResults.DeleteManyAsync(filter);

        var imageFilter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Metadata["sessionId"], sessionId);
        using (var cursor = await traceImagesBucket.FindAsync(imageFilter, new GridFSFindOptions()))
        {
            await cursor.ForEachAsync(async x =>
            {
                await traceImagesBucket.DeleteAsync(x.Id);
            });
        }
    }

    ///<inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Purge file hosted service is stopping.");

        timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    ///<inheritdoc/>
    public void Dispose()
    {
        timer?.Dispose();
    }

}
