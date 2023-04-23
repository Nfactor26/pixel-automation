using Dawn;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Jobs
{
    public class SendTriggerNotificationJob : IJob
    {
        private readonly ILogger logger;
        private readonly string redisConnectionString;
        private readonly ConnectionMultiplexer connection;
       
        public SendTriggerNotificationJob(ILogger<SendTriggerNotificationJob> logger, IConfiguration configuration)
        {
            this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;
            Guard.Argument(configuration, nameof(configuration)).NotNull();
            this.redisConnectionString = configuration["RedisConnectionString"] ?? string.Empty;
            if(!string.IsNullOrEmpty(redisConnectionString) )
            {
                try
                {
                    this.connection = ConnectionMultiplexer.Connect(this.redisConnectionString);
                    logger.LogInformation("Redis connection established to : {0}", this.redisConnectionString);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occured while connecting to redis instance : {0}", this.redisConnectionString);
                }
                return;
            }
            logger.LogWarning("Redis connection could not be established to : {0}. No template executions message will be sent on trigger activations.", this.redisConnectionString);
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var dataMap = context.MergedJobDataMap;
            if(this.connection != null)
            {
                var pubsub = connection.GetSubscriber();
                var redisChannel = new RedisChannel(dataMap.Get("handler-key").ToString(), RedisChannel.PatternMode.Literal);
                await pubsub.PublishAsync(redisChannel, dataMap.Get("template-name").ToString(), CommandFlags.FireAndForget);
                logger.LogInformation($"Send notification for executing {dataMap.Get("template-name")} to channel {dataMap.Get("handler-key")}");
            }           
        }
    }
}
