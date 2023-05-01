using Dawn;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Linq;
using Pixel.Persistence.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Api.Hubs;

/// <summary>
/// Track available agents and dispatch requests to execute tests to them
/// </summary>
public interface IAgentManager
{
    /// <summary>
    /// Add details of agent to track whenever an agent is connected
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="agent"></param>
    void RegisterAgent(string connectionId, AgentDetails agent);

    /// <summary>
    /// Remove the agent details
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="agentName"></param>
    /// <param name="agentGroup"></param>
    void DeRegisterAgent(string connectionId, AgentDetails agent);

    /// <summary>
    /// When the agent is reconnected, we have a new connectionID.
    /// Update details on connection so that agent is associated with new connectionID.
    /// </summary>
    /// <param name="connectionId"></param>
    /// <param name="agentName"></param>
    void UpdateAgentConnection(string connectionId, AgentDetails agent);

    /// <summary>
    /// Mark agent as inactive by clearing the connnectionId when agent connection is lost.
    /// </summary>
    /// <param name="connectionId"></param>
    void MarkAgentInactive(string connectionId);

    /// <summary>
    /// Send a request to any available agent to start execution of a template
    /// </summary>
    /// <param name="template"></param>
    /// <param name="handler"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    Task ExecuteTemplateAsync(string template, string handler, string group);
}

/// <summary>
/// Implementation of <see cref="IAgentManager"/>
/// </summary>
public class AgentManager : IAgentManager
{
    private readonly List<AgentConnection> agents = new();   
    private readonly ILogger logger;
    private readonly IHubContext<AgentsHub, IAgentClient> agentsHub;
    private readonly object locker = new();

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="agentsHub"></param>
    public AgentManager(ILogger<AgentManager> logger, IHubContext<AgentsHub, IAgentClient> agentsHub)
    {
        this.logger = Guard.Argument(logger, nameof(logger)).NotNull().Value;
        this.agentsHub = Guard.Argument(agentsHub, nameof(agentsHub)).NotNull().Value;
    }

    /// </inheritdoc>  
    public void RegisterAgent(string connectionId, AgentDetails agent)
    {
        Guard.Argument(connectionId, nameof(connectionId)).NotNull();
        Guard.Argument(agent, nameof(agent)).NotNull();

        lock(locker)
        {
            if (!this.agents.Any(a => a.Agent.Equals(agent)))
            {
                var connectedAgent = new AgentConnection()
                {
                    ConnectionId = connectionId,
                    Agent = agent
                };
                this.agents.Add(connectedAgent);
                logger.LogInformation("Agent {0} in group {1} was added", agent.Name, agent.Group);
                return;
            }
            logger.LogWarning("Agent {0} in group {1} already exists", agent.Name, agent.Group);
        }       
    }

    /// </inheritdoc>  
    public void DeRegisterAgent(string connectionId, AgentDetails agent)
    {
        Guard.Argument(connectionId, nameof(connectionId)).NotNull();
        Guard.Argument(agent, nameof(agent)).NotNull();
       
        lock(locker)
        {
            var agentToRemove = this.agents.FirstOrDefault(a =>  a.Agent.Equals(agent));
            if (agentToRemove != null)
            {
                this.agents.Remove(agentToRemove);
                logger.LogInformation("Agent {0} was removed from group {1}", agentToRemove.Agent.Name, agentToRemove.Agent.Group);
                return;
            }
            logger.LogWarning("Agent with name : {0} was not found.", agent.Name);
        }       
    }

    /// </inheritdoc>  
    public void UpdateAgentConnection(string connectionId, AgentDetails agent)
    {
        Guard.Argument(connectionId, nameof(connectionId)).NotNull();
        Guard.Argument(agent, nameof(agent)).NotNull();
        lock(locker)
        {
            var agentToUpdate = this.agents.FirstOrDefault(a => a.Agent.Equals(agent));
            if(agentToUpdate == null) //can happen if the server goes down and comes back up before agent connection retry timeout
            {
                RegisterAgent(connectionId, agent);
                logger.LogInformation("Agent {0} was not found and hence registered.", agent.Name);
            }
            else
            {
                agentToUpdate.ConnectionId = connectionId;
                logger.LogInformation("Agent {0} is associated with new connection ID : {1} now.", agentToUpdate.Agent.Name);
                return;
            }           
        }       
    }

    /// </inheritdoc>  
    public void MarkAgentInactive(string connectionId)
    {
        Guard.Argument(connectionId, nameof(connectionId)).NotNull();     
        lock(locker)
        {
            var agentToUpdate = this.agents.FirstOrDefault(a => a.ConnectionId.Equals(connectionId));           
            if (agentToUpdate != null)
            {
                agentToUpdate.ConnectionId = string.Empty;
                logger.LogInformation("Agent {0} is marked inactive now due to connection loss.", agentToUpdate.Agent.Name);
                return;
            }
            logger.LogWarning("No connection with ID :{0} was found.", connectionId);
        }     
    }

    /// <inheritdoc>   
    public async Task ExecuteTemplateAsync(string template, string handler, string group = "default")
    {
        var agentsInGroup = this.agents.Where(a => a.Agent.Group.Equals(group));
        if (!agentsInGroup.Any())
        {
            throw new ArgumentException($"There are no agents in group : {group}");
        }
        var usableAgents = agentsInGroup.Where(a => a.IsActive && a.Agent.RegisteredHadlers.Contains(handler));
        if (!usableAgents.Any())
        {
            throw new Exception($"There are no available agents to process request in group : {group}");
        }

        foreach (var agent in usableAgents)
        {
            bool canAgentExecuteRequest = await this.agentsHub.Clients.Client(agent.ConnectionId).CanExecuteNew();
            if (canAgentExecuteRequest)
            {                
                logger.LogInformation("Agent {0} was picked to execute template {1}", agent.Agent.Name, template);
                await agentsHub.Clients.Client(agent.ConnectionId).ExecuteTemplate(template, handler);
                logger.LogInformation("Agent {0} started execution of template {1}", agent.Agent.Name, template);
                break;
            }
        }
    }   
}

