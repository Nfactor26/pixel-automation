using Dawn;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Respository;
using Pixel.Persistence.Services.Api.Jobs;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Services
{
    public class QuartzJobBuilderService : IHostedService
    {
        private readonly ILogger logger;
        private readonly ITemplateRepository templateRepository;       
        private readonly IJobManager jobManager;

        public QuartzJobBuilderService(ILogger<QuartzJobBuilderService> logger, ITemplateRepository templateRepository,
            IJobManager jobManager)
        {
            this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;
            this.templateRepository = Guard.Argument(templateRepository, nameof(templateRepository)).NotNull().Value;           
            this.jobManager = Guard.Argument(jobManager, nameof(jobManager)).NotNull().Value;   
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var templates = await this.templateRepository.GetAllTemplatesAsync();
            foreach (var template in templates)
            {
                foreach (var sessionTriger in template.Triggers.OfType<CronSessionTrigger>())
                {
                    try
                    {
                        await jobManager.AddCronJobAsync(template, sessionTriger);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An exception occured while trying to create a quart job and trigger for {0} -> {1}", template.Name, sessionTriger.Name);                     
                    }
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
