﻿using Dawn;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Pixel.Persistence.Services.Api.Hubs;
using Quartz;
using System;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Jobs
{
    /// <summary>
    /// A quartz job to start execution of test cases whenever triggers are activated for templates
    /// </summary>
    public class ProcessTriggerJob : IJob
    {
        private readonly ILogger logger;
        private readonly IHubContext<AgentsHub, IAgentClient> agentsHub;
        private readonly IAgentManager agentManager;
        
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>     
        /// <param name="agentManager"></param>
        public ProcessTriggerJob(ILogger<ProcessTriggerJob> logger, IAgentManager agentManager)
        {
            this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;          
            this.agentManager = Guard.Argument(agentManager, nameof(agentManager)).NotNull().Value; 
        }

        /// </inheritdoc>        
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var dataMap = context.MergedJobDataMap;
                string template = dataMap.Get("template-name").ToString();
                string handler = dataMap.Get("handler-key").ToString();
                logger.LogInformation("Trigger activated for template : {0}, handler : {1}", template, handler);
                await agentManager.ExecuteTemplateAsync(template, handler, "default");
            }
            catch (Exception ex)
            {
                //TODO : Check JobExecutionException for advanced options on rescheuding etc
                logger.LogError(ex, "There was an error while trying to execute the job");
            }    
        }
    }
}
