using Microsoft.AspNetCore.SignalR.Client;

namespace Pixel.Automation.Server.Agent;

/// <summary>
/// Retry Policy for connection to SignalR hub
/// </summary>
internal class ConnectionRetryPolicy : IRetryPolicy
{
    /// <summary>
    /// Try every 30 seconds for up to 10 minutes i.e. 20 attempts in all
    /// </summary>
    /// <param name="retryContext"></param>
    /// <returns></returns>
    public TimeSpan? NextRetryDelay(RetryContext retryContext)
    {
        return retryContext.ElapsedTime < TimeSpan.FromMinutes(10) ? TimeSpan.FromSeconds(30) : null; 
    }
}
