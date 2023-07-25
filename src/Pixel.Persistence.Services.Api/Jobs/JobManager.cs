using Dawn;
using Microsoft.Extensions.Logging;
using Pixel.Automation.Core;
using Pixel.Persistence.Core.Models;
using Quartz;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Jobs
{
    /// <summary>
    /// Contract to manage triggers and jobs using Quartz scheduler
    /// </summary>
    public interface IJobManager
    {
        /// <summary>
        /// Add a new trigger and create a job if required
        /// </summary>
        /// <param name="template"></param>
        /// <param name="cronSessionTrigger"></param>
        /// <returns></returns>
        Task<DateTimeOffset> AddCronJobAsync(SessionTemplate template, CronSessionTrigger cronSessionTrigger);

        /// <summary>
        /// Delete an existing trigger belonging to template
        /// </summary>
        /// <param name="template"></param>
        /// <param name="sessionTrigger"></param>
        /// <returns></returns>
        Task<bool> DeleteTriggerAsync(SessionTemplate template, SessionTrigger sessionTrigger);       
      
        /// <summary>
        /// Update trigger
        /// </summary>
        /// <param name="template"></param>
        /// <param name="cronSessionTrigger"></param>
        /// <returns></returns>
        Task UpdateCronJobAsync(SessionTemplate template, CronSessionTrigger cronSessionTrigger);

        /// <summary>
        /// Create a new one time trigger and fire it
        /// </summary>
        /// <param name="template"></param>
        /// <param name="cronSessionTrigger"></param>
        /// <returns></returns>
        Task RunTriggerAsync(SessionTemplate template, SessionTrigger cronSessionTrigger);

        /// <summary>
        /// Get next fimre time in Utc for a given trigger belonging to a given job
        /// </summary>
        /// <param name="jobName">Name of the job to which trigger belongs</param>
        /// <param name="triggerName">Name of the trigger to be paused</param>
        /// <returns></returns>
        Task<DateTimeOffset?> GetNextFireTimeUtcAsync(string jobName, string triggerName);

        /// <summary>
        /// Pause trigger with a given name belonging to a given job
        /// </summary>
        /// <param name="jobName">Name of the job to which trigger belongs</param>
        /// <param name="triggerName">Name of the trigger to be paused</param>
        /// <returns></returns>
        Task PauseTriggerAsync(string jobName, string triggerName);

        /// <summary>
        /// Resume trigger with a given name belonging to a given job
        /// </summary>
        /// <param name="jobName">Name of the job to which trigger belongs</param>
        /// <param name="triggerName">Name of the trigger to be resumed</param>
        /// <returns></returns>
        Task ResumeTriggerAsync(string jobName, string triggerName);
        
        /// <summary>
        /// Pause a job with a given name
        /// </summary>
        /// <param name="jobName">Name of the job to pause</param>
        /// <returns></returns>
        Task PauseJobAsync(string jobName);
      
        /// <summary>
        /// Resume a job with a given name
        /// </summary>
        /// <param name="jobName">Name of the job to resume</param>
        /// <returns></returns>
        Task ResumeJobAsync(string jobName);
       
    }

    /// <summary>
    /// Implementation of <see cref="IJobManager"/>
    /// </summary>
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

        /// <inheritdoc/>
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
                .UsingJobData("trigger-name", cronSessionTrigger.Name)
                .UsingJobData("handler-key", cronSessionTrigger.Handler)
                .UsingJobData("handler-arguments", cronSessionTrigger.Parameters.ToCommaSeperateString())
                .UsingJobData("agent-group", cronSessionTrigger.Group)
                .ForJob(job).WithCronSchedule(cronSessionTrigger.CronExpression).StartNow().Build();
            logger.LogInformation("Created a new trigger job for job : {0}, trigger : {1}", template.Name, cronSessionTrigger.Name);
            var scheduledTime = await scheduler.ScheduleJob(trigger, CancellationToken.None);
            if(!cronSessionTrigger.IsEnabled)
            {
                await scheduler.PauseTrigger(trigger.Key);
            }
            return cronSessionTrigger.IsEnabled ? scheduledTime : DateTimeOffset.MaxValue;
        }

        /// <inheritdoc/>
        public async Task UpdateCronJobAsync(SessionTemplate template, CronSessionTrigger cronSessionTrigger)
        {
            await DeleteTriggerAsync(template, cronSessionTrigger);
            await AddCronJobAsync(template, cronSessionTrigger);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteTriggerAsync(SessionTemplate template, SessionTrigger sessionTrigger)
        {
            var scheduler = await this.schedulerFactory.GetScheduler();           
            bool wasRemoved = await scheduler.UnscheduleJob(new TriggerKey(sessionTrigger.Name, template.Name));
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

        /// <inheritdoc/>
        public async Task RunTriggerAsync(SessionTemplate template, SessionTrigger sessionTrigger)
        {
            var scheduler = await this.schedulerFactory.GetScheduler();
            JobKey jobKey = new JobKey(template.Name, template.Name);
            var job = await scheduler.GetJobDetail(jobKey);                  
            var trigger = TriggerBuilder.Create().WithIdentity($"{sessionTrigger.Name}-{DateTime.Now.ToString("hh-mm-ss")}", template.Name)
              .UsingJobData("trigger-name", sessionTrigger.Name)
              .UsingJobData("handler-key", sessionTrigger.Handler)
              .UsingJobData("handler-arguments", sessionTrigger.Parameters.ToCommaSeperateString())
              .UsingJobData("agent-group", sessionTrigger.Group)
              .ForJob(job).StartNow().Build(); 
            await scheduler.ScheduleJob(trigger, CancellationToken.None);
            logger.LogInformation("Adhoc one-time trigger with identity : {0} was created and scheduled", trigger.Key);
        }

        /// <inheritdoc/>
        public async Task<DateTimeOffset?> GetNextFireTimeUtcAsync(string jobName, string triggerName)
        {
            var scheduler = await this.schedulerFactory.GetScheduler();
            var trigger =  await scheduler.GetTrigger(new TriggerKey(triggerName, jobName));           
            return trigger.GetNextFireTimeUtc();
        }

        /// <inheritdoc/>      
        public async Task PauseTriggerAsync(string jobName, string triggerName)
        {
            var scheduler = await this.schedulerFactory.GetScheduler();                     
            await scheduler.PauseTrigger(new TriggerKey(triggerName, jobName));
            logger.LogInformation("Trigger {0} was paused for job {1}", jobName, triggerName);
        }

        /// <inheritdoc/>
        public async Task ResumeTriggerAsync(string jobName, string triggerName)
        {
            var scheduler = await this.schedulerFactory.GetScheduler();         
            await scheduler.ResumeTrigger(new TriggerKey(triggerName, jobName));
            logger.LogInformation("Trigger {0} was resumed for job {1}", jobName, triggerName);
        }

        /// <inheritdoc/>
        public async Task PauseJobAsync(string jobName)
        {
            var scheduler = await this.schedulerFactory.GetScheduler();
            await scheduler.PauseJob(new JobKey(jobName));
            logger.LogInformation("Job {0} was paused", jobName);
        }

        /// <inheritdoc/>
        public async Task ResumeJobAsync(string jobName)
        {
            var scheduler = await this.schedulerFactory.GetScheduler();         
            await scheduler.ResumeJob(new JobKey(jobName));
            logger.LogInformation("Job {0} was resumed", jobName);
        }
    }
}
