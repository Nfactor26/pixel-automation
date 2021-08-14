using Dawn;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Respository;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Services
{
    /// <summary>
    /// Purge file service is a hosted service that is responsible for deleting
    /// file revisions for automation project and prefab project files.
    /// As user can save multiple times during a session, there will be large number of
    /// revisions created for the same file. We want to peridically purge these files.
    /// </summary>
    public class PurgeFileService : IHostedService, IDisposable
    {
        private readonly ILogger logger;
        private readonly IProjectRepository projectRepository;
        private readonly IPrefabRepository prefabRepository;
        private readonly RetentionPolicy retentionPolicy;
        private Timer timer;

        public PurgeFileService(ILogger<PurgeFileService> logger, IProjectRepository projectRepository,
            IPrefabRepository prefabRepository, RetentionPolicy retentionPolicy)
        {
            this.logger = Guard.Argument(logger).NotNull().Value;
            this.projectRepository = Guard.Argument(projectRepository).NotNull().Value;
            this.prefabRepository = Guard.Argument(prefabRepository).NotNull().Value;
            this.retentionPolicy = Guard.Argument(retentionPolicy).NotNull();
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Purge file background service is running now.");
            timer = new Timer(PurgeFiles, null, TimeSpan.FromHours(6), TimeSpan.FromHours(6));

            #if DEBUG
            timer.Change(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
            #endif

            return Task.CompletedTask;
        }

        private void PurgeFiles(object state)
        {
            Task workerTask = new Task(async () =>
            {
                try
                {
                    await projectRepository.PurgeRevisionFiles(retentionPolicy);
                    await prefabRepository.PurgeRevisionFiles(retentionPolicy);
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
            logger.LogInformation("Purge file hosted service is stopping.");

            timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }

    }
}
