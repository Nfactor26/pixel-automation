using Dawn;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Pixel.Persistence.Core.Models;
using System;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Hubs
{
    /// <summary>
    /// Typed client for agents
    /// </summary>
    public interface IAgentClient
    {
        /// <summary>
        /// Execute a given template on agent using a given handler
        /// </summary>
        /// <param name="templateName"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        Task ExecuteTemplate(string templateName, string handler, string arguments);

        /// <summary>
        /// Check if agent can execute a new request
        /// </summary>
        /// <returns></returns>
        Task<bool> CanExecuteNew();
    }

    /// <summary>
    /// SignalR endpoint for agents hub
    /// </summary>
    public class AgentsHub : Hub<IAgentClient>
    {
        private readonly ILogger logger;
        private readonly IAgentManager agentManager;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="agentManager"></param>
        public AgentsHub(ILogger<AgentsHub> logger, IAgentManager agentManager)
        {
            this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;
            this.agentManager = Guard.Argument(agentManager, nameof(agentManager)).NotNull().Value;
        }

        /// <summary>
        /// When the clients are started, they should register themselves by calling RegisterAgent
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public async Task RegisterAgent(AgentDetails agent)
        {
            Guard.Argument(agent, nameof(agent)).NotNull();
            this.agentManager.RegisterAgent(Context.ConnectionId, agent);
            await Groups.AddToGroupAsync(Context.ConnectionId, agent.Group);
        }     

        /// <summary>
        /// When the agents are shutting down, they should de-register themselves by calling DeRegisterAgent
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public async Task DeRegisterAgent(AgentDetails agent)
        {
            this.agentManager.DeRegisterAgent(Context.ConnectionId, agent);
            await Task.CompletedTask;
        }

        /// <summary>
        /// If an agent has lost connection and connection is restored later, they should call AgentReconnected
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public async Task AgentReconnected(AgentDetails agent)
        {
            this.agentManager.UpdateAgentConnection(Context.ConnectionId, agent);
            await Task.CompletedTask;
        }
        
        /// </inheritdoc> 
        public override Task OnConnectedAsync()
        {
            logger.LogInformation("A new agent connected with ConnectionId : {0}", Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        /// </inheritdoc> 
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            this.agentManager.MarkAgentInactive(Context.ConnectionId);
            logger.LogError(exception, "Agent with ConnectionId : {0} was disconnected", Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
