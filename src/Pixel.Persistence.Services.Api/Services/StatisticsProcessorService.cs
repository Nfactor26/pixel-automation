using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pixel.Persistence.Respository;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Services
{
    public class StatisticsProcessorService : IHostedService, IDisposable
    {
        private readonly ILogger logger;
        private readonly ITestSessionRepository testSessionRepository;    
        private readonly ITestStatisticsRepository testStatisticsRepository;
        private readonly IProjectStatisticsRepository projectStatisticsRepository;
        private Timer timer;
    
        public StatisticsProcessorService(ILogger<StatisticsProcessorService> logger, 
            ITestSessionRepository testSessionRepository, ITestStatisticsRepository testStatisticsRepository, IProjectStatisticsRepository projectStatisticsRepository)
        {
            this.logger = logger;
            this.testSessionRepository = testSessionRepository;          
            this.testStatisticsRepository = testStatisticsRepository;
            this.projectStatisticsRepository = projectStatisticsRepository;
        }

     
        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Statistics processor hosted service is running now.");
            timer = new Timer(ProcessStatistics, null, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));

            #if DEBUG
            timer.Change(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));          
            #endif

            return Task.CompletedTask;
        }

        private void ProcessStatistics(object state)
        {
            Task workerTask = new Task(async () =>
            {
                try
                {
                    var unprocessedSessions = await testSessionRepository.GetUnprocessedSessionsAsync();
                    logger.LogInformation($"There are {unprocessedSessions.Count()} unprocessed sessions");
                    if (unprocessedSessions.Any())
                    {
                        var sessionToProcess = unprocessedSessions.Last();
                        logger.LogInformation($"Start to process statistics for session : {sessionToProcess}");
                        await projectStatisticsRepository.AddOrUpdateStatisticsAsync(sessionToProcess);
                        await testStatisticsRepository.AddOrUpdateStatisticsAsync(sessionToProcess);
                        await testSessionRepository.MarkSessionProcessedAsync(sessionToProcess);
                        logger.LogInformation($"Finished processing session {sessionToProcess}");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                }
            });
            workerTask.Start();
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Statistics processor hosted service is stopping.");

            timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }

    }
}
