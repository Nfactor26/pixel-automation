using Dawn;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using StackExchange.Redis;
using System;

namespace Pixel.Persistence.Services.Api.Jobs
{
    public interface IRedisConnectionFactory
    {
        ConnectionMultiplexer GetOrCreateConnectionMultiplexer();
    }

    public class RedisConnectionFactory : IRedisConnectionFactory
    {
        private readonly string connectionStringKey = "RedisConnectionString";
        private readonly ILogger logger;
        private readonly IConfiguration configuration;
        private readonly IChangeToken changeToken;
        private ConnectionMultiplexer multiplexer;

        public RedisConnectionFactory(ILogger<RedisConnectionFactory> logger, IConfiguration configuration)
        {
            this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;
            this.configuration = Guard.Argument(configuration, nameof(configuration)).NotNull().Value;
            this.changeToken = this.configuration.GetReloadToken();
        }

        public ConnectionMultiplexer GetOrCreateConnectionMultiplexer()
        {
            if (multiplexer != null && !this.changeToken.HasChanged)
            {
                return multiplexer;
            }
            string redisConnectionString = this.configuration[connectionStringKey];
            try
            {
                if (this.multiplexer != null)
                {
                    this.multiplexer.ConnectionFailed -= OnConnectionFailed;
                    this.multiplexer.ConnectionRestored -= OnConnectionRestored;
                    this.multiplexer.Dispose();
                    logger.LogInformation("Existing redis connection was disposed");
                }
                if (!string.IsNullOrEmpty(redisConnectionString))
                {
                    this.multiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
                    this.multiplexer.ConnectionFailed += OnConnectionFailed;
                    this.multiplexer.ConnectionRestored += OnConnectionRestored;
                    logger.LogInformation("A new redis connection was established to : {0}", redisConnectionString);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occured while trying to connect to redis instance : {0}", redisConnectionString);
            }
            return this.multiplexer;
        }

        private void OnConnectionRestored(object sender, ConnectionFailedEventArgs e)
        {
            logger.LogInformation("Redis connection is restored now");
        }

        private void OnConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            logger.LogInformation("Redis connection is disconnected now");
        }
    }
}
