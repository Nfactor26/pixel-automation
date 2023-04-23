using Dawn;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Respository;
using Pixel.Persistence.Services.Api.Jobs;
using Quartz;
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
        private readonly ISchedulerFactory schedulerFactory;

        public QuartzJobBuilderService(ILogger<QuartzJobBuilderService> logger, ITemplateRepository templateRepository, ISchedulerFactory schedulerFactory)
        {
            this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;
            this.templateRepository = Guard.Argument(templateRepository, nameof(templateRepository)).NotNull().Value;
            this.schedulerFactory = Guard.Argument(schedulerFactory, nameof(schedulerFactory)).NotNull().Value;           
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var scheduler = await schedulerFactory.GetScheduler();
            var templates = await this.templateRepository.GetAllAsync();
            foreach (var template in templates)
            {
                foreach (var sessionTriger in template.Triggers.OfType<CronSessionTrigger>())
                {
                    try
                    {
                        logger.LogInformation("Creating a quartz job and trigger for {0} -> {1} with cron expression : {2}", template.Name, sessionTriger.Name, sessionTriger.CronExpression);
                        JobKey sendNotificationJob = new JobKey(sessionTriger.Name, template.Name);
                        var job = JobBuilder.Create<SendTriggerNotificationJob>()
                               .WithIdentity(sendNotificationJob)
                               .UsingJobData("template-name", template.Name)
                               .UsingJobData("handler-key", sessionTriger.Handler)
                               .Build();
                        var trigger = TriggerBuilder.Create().ForJob(job).WithCronSchedule(sessionTriger.CronExpression).Build();
                        await scheduler.ScheduleJob(job, trigger, cancellationToken);
                        logger.LogInformation("Job is now scheduled for {0} -> {1}", template.Name, sessionTriger.Name);
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
