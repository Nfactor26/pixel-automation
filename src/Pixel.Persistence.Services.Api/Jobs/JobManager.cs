using Dawn;
using Microsoft.Extensions.Logging;
using Pixel.Persistence.Core.Models;
using Quartz;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Jobs
{
    public interface IJobManager
    {
        Task<DateTimeOffset> AddCronJobAsync(SessionTemplate template, CronSessionTrigger cronSessionTrigger);
        Task<bool> DeleteTriggerAsync(SessionTemplate template, SessionTrigger sessionTrigger);
        Task PauseTriggerAsync(SessionTemplate template, SessionTrigger sessionTrigger);
        Task ResumeTriggerAsync(SessionTemplate template, SessionTrigger sessionTrigger);
        Task UpdateCronJobAsync(SessionTemplate template, CronSessionTrigger cronSessionTrigger);
    }

    public class JobManager : IJobManager
    {
        private readonly ISchedulerFactory schedulerFactory;
        private readonly ILogger logger;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="scheduler"></param>
        public JobManager(ILogger<JobManager> logger, ISchedulerFactory schedulerFactory)
        {
            this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;
            this.schedulerFactory = Guard.Argument(schedulerFactory, nameof(schedulerFactory)).NotNull().Value;
        }

        public async Task<DateTimeOffset> AddCronJobAsync(SessionTemplate template, CronSessionTrigger cronSessionTrigger)
        {
            var scheduler = await this.schedulerFactory.GetScheduler();
            JobKey jobKey = new JobKey(template.Name, template.Name);
            var job = await scheduler.GetJobDetail(jobKey);
            if (job == null)
            {
                job = JobBuilder.Create<ProcessTriggerJob>()
                   .WithIdentity(jobKey).StoreDurably(true)
                   .UsingJobData("template-name", template.Name)
                   .Build();
                await scheduler.AddJob(job, true);
                logger.LogInformation("Created a new quartz job for template : {0}", template.Name);
            }
            var trigger = TriggerBuilder.Create().WithIdentity(cronSessionTrigger.Name, template.Name)
                .UsingJobData("handler-key", cronSessionTrigger.Handler)
                .UsingJobData("agent-group", cronSessionTrigger.Group)
                .ForJob(job).WithCronSchedule(cronSessionTrigger.CronExpression).StartNow().Build();
            logger.LogInformation("Created a new trigger job for job : {0}, trigger : {1}", template.Name, cronSessionTrigger.Name);
            return await scheduler.ScheduleJob(trigger, CancellationToken.None);
        }

        public async Task UpdateCronJobAsync(SessionTemplate template, CronSessionTrigger cronSessionTrigger)
        {
            await DeleteTriggerAsync(template, cronSessionTrigger);
            await AddCronJobAsync(template, cronSessionTrigger);
        }

        public async Task<bool> DeleteTriggerAsync(SessionTemplate template, SessionTrigger sessionTrigger)
        {
            var scheduler = await this.schedulerFactory.GetScheduler();
            var triggerKey = new TriggerKey(sessionTrigger.Name, template.Name);
            bool wasRemoved = await scheduler.UnscheduleJob(triggerKey);
            if(wasRemoved)
            {
                logger.LogInformation("Trigger {0} was deleted from job {1}", sessionTrigger.Name, template.Name);
            }
            else
            {
                logger.LogWarning("Trigger {0} could not be deleted from job {1}", sessionTrigger.Name, template.Name);
            }
            return wasRemoved;
        }

        public async Task PauseTriggerAsync(SessionTemplate template, SessionTrigger sessionTrigger)
        {
            var scheduler = await this.schedulerFactory.GetScheduler();
            var triggerKey = new TriggerKey(sessionTrigger.Name, template.Name);
            await scheduler.PauseTrigger(triggerKey);
            logger.LogInformation("Trigger {0} was paused for job {1}", sessionTrigger.Name, template.Name);
        }

        public async Task ResumeTriggerAsync(SessionTemplate template, SessionTrigger sessionTrigger)
        {
            var scheduler = await this.schedulerFactory.GetScheduler();
            var triggerKey = new TriggerKey(sessionTrigger.Name, template.Name);
            await scheduler.ResumeTrigger(triggerKey);
            logger.LogInformation("Trigger {0} was resumed for job {1}", sessionTrigger.Name, template.Name);
        }

    }
}
