using Dawn;
using Microsoft.Extensions.Logging;
using Quartz;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Jobs
{
    public class SendTriggerNotificationJob : IJob
    {
        private readonly ILogger logger;       
        private readonly IRedisConnectionFactory connectionFactory;
       
        public SendTriggerNotificationJob(ILogger<SendTriggerNotificationJob> logger, IRedisConnectionFactory connectionFactory)
        {
            this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;
            this.connectionFactory = Guard.Argument(connectionFactory, nameof(connectionFactory)).NotNull().Value;           
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var dataMap = context.MergedJobDataMap;
            string template = dataMap.Get("template-name").ToString();
            string handler = dataMap.Get("handler-key").ToString();
            logger.LogInformation("Trigger activated for template : {0}, handler : {1}", template, handler);
            var connection = this.connectionFactory.GetOrCreateConnectionMultiplexer();
            if(connection  != null)
            {
                var pubsub = connection.GetSubscriber();
                var redisChannel = new RedisChannel(handler, RedisChannel.PatternMode.Literal);
                await pubsub.PublishAsync(redisChannel, template, CommandFlags.FireAndForget);
                logger.LogInformation($"Send notification for executing {template} to channel {handler}");
            }           
        }
    }
}
