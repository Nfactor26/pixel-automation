using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pixel.Persistence.Respository;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api
{
    public class StatisticsProcessorService : IHostedService, IDisposable
    {
        private readonly ILogger logger;
        private readonly ITestSessionRepository testSessionRepository;    
        private readonly ITestStatisticsRepository testStatisticsRepository;
        private Timer timer;
    
        public StatisticsProcessorService(ILogger<StatisticsProcessorService> logger, ITestSessionRepository testSessionRepository,
            ITestStatisticsRepository testStatisticsRepository)
        {
            this.logger = logger;
            this.testSessionRepository = testSessionRepository;          
            this.testStatisticsRepository = testStatisticsRepository;
        }

     
        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("statistics processor background service is running now.");

            timer = new Timer(ProcessStatistics, null, TimeSpan.Zero, TimeSpan.FromMinutes(10));
            return Task.CompletedTask;
        }

        private void ProcessStatistics(object state)
        {
            Task wokerTask = new Task(async () =>
            {
                logger.LogInformation("Start to process statistics");
                var unprocessedSessions = await testSessionRepository.GetUnprocessedSessionsAsync();
                logger.LogInformation($"There are {unprocessedSessions} unprocessed sessions");
                if(unprocessedSessions.Any())
                {                   
                    await testStatisticsRepository.AddOrUpdateStatisticsAsync(unprocessedSessions.Last());
                    logger.LogInformation($"Finished processing session {unprocessedSessions.Last()}");
                }
            });
          
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Timed Hosted Service is stopping.");

            timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }

    }
}
