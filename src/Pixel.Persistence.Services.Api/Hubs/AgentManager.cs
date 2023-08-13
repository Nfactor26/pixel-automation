using Dawn;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Linq;
using Pixel.Persistence.Core.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    Task ExecuteTemplateAsync(string template, string handler, string group, string arguments);
}

/// <summary>
/// Implementation of <see cref="IAgentManager"/>
/// </summary>
public class AgentManager : IAgentManager
{
    private readonly ConcurrentDictionary<string, ConcurrentQueue<AgentConnection>> agents = new();   
    private readonly ILogger logger;
    private readonly IHubContext<AgentsHub, IAgentClient> agentsHub;
    private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
 
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


    public bool TryRemoveAgent(AgentDetails agentToRemove)
    {
        if (!agents.ContainsKey(agentToRemove.Group))
        {
            return false;
        }
        var agentsInGroup = agents[agentToRemove.Group];
        if (!agentsInGroup.Any(a => a.Agent.Equals(agentToRemove)))
        {
            return agents.TryUpdate(agentToRemove.Group,
                new ConcurrentQueue<AgentConnection>(agentsInGroup.Except(agentsInGroup.Where(a => a.Agent.Equals(agentToRemove)))),
                agentsInGroup);
        }
        return false;
    }

    /// </inheritdoc>  
    public void RegisterAgent(string connectionId, AgentDetails agent)
    {
        Guard.Argument(connectionId, nameof(connectionId)).NotNull();
        Guard.Argument(agent, nameof(agent)).NotNull();
        if (!agents.ContainsKey(agent.Group))
        {
            agents.TryAdd(agent.Group, new ConcurrentQueue<AgentConnection>());
        }
        var agentsInGroup = agents[agent.Group];
        if (!agentsInGroup.Any(a => a.Agent.Equals(agent)))
        {
            var connectedAgent = new AgentConnection()
            {
                ConnectionId = connectionId,
                Agent = agent
            };
            agentsInGroup.Enqueue(connectedAgent);
            logger.LogInformation("Agent {0} in group {1} was added", agent.Name, agent.Group);
            return;
        }       
        logger.LogWarning("Agent {0} in group {1} already exists", agent.Name, agent.Group);
    }

    /// </inheritdoc>  
    public void DeRegisterAgent(string connectionId, AgentDetails agent)
    {
        Guard.Argument(connectionId, nameof(connectionId)).NotNull();
        Guard.Argument(agent, nameof(agent)).NotNull();
        if (agents.ContainsKey(agent.Group))
        {
            var agentsInGroup = agents[agent.Group];
            if (agentsInGroup.Any(a => a.Agent.Equals(agent)))
            {
                agents.TryUpdate(agent.Group,
                    new ConcurrentQueue<AgentConnection>(agentsInGroup.Except(agentsInGroup.Where(a => a.Agent.Equals(agent)))),
                    agentsInGroup);
                logger.LogInformation("Agent {0} was removed from group {1}", agent.Name, agent.Group);
                return;
            }
        }
        logger.LogWarning("Agent with name : {0} was not found.", agent.Name);
    }

    /// </inheritdoc>  
    public void UpdateAgentConnection(string connectionId, AgentDetails agent)
    {
        Guard.Argument(connectionId, nameof(connectionId)).NotNull();
        Guard.Argument(agent, nameof(agent)).NotNull();
        if (!agents.ContainsKey(agent.Group))
        {
            agents.TryAdd(agent.Group, new ConcurrentQueue<AgentConnection>());
        }
        var agentsInGroup = agents[agent.Group];
        var agentToUpdate = agentsInGroup.FirstOrDefault(a => a.Agent.Equals(agent));
        if (agentToUpdate == null) //can happen if the server goes down and comes back up before agent connection retry timeout
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

    /// </inheritdoc>  
    public void MarkAgentInactive(string connectionId)
    {
        Guard.Argument(connectionId, nameof(connectionId)).NotNull();
        foreach(var key in agents.Keys)
        {
            foreach(var agent in agents[key])
            {
                if(agent.ConnectionId.Equals(connectionId))
                {
                    agent.ConnectionId = string.Empty;
                    logger.LogInformation("Agent {0} is marked inactive now due to connection loss.", agent.Agent.Name);
                    return;
                }
            }
        }
        logger.LogWarning("No connection with ID :{0} was found.", connectionId);
    }

    /// <inheritdoc>   
    public async Task ExecuteTemplateAsync(string template, string handler, string group = "default", string arguments = "")
    {
        try
        {
            await semaphore.WaitAsync();
            if (this.agents.ContainsKey(group))
            {
                var agentsInGroup = this.agents[group];
                if (!agentsInGroup.Any(a => a.IsActive && a.Agent.RegisteredHadlers.Contains(handler)))
                {                   
                    logger.LogWarning("No agents in group : {0} having {1} agents are available to proces request for template : {2} with handler : {3}", group, agentsInGroup.Count(), template, handler);                   
                }

                Stack<AgentConnection> availableAgents = new();
                while (agentsInGroup.TryDequeue(out AgentConnection agent))
                {
                    availableAgents.Push(agent);
                    bool canAgentExecuteRequest = await this.agentsHub.Clients.Client(agent.ConnectionId).CanExecuteNew();
                    if (canAgentExecuteRequest)
                    {
                        logger.LogInformation("Agent {0} was picked to execute template {1}", agent.Agent.Name, template);
                        await agentsHub.Clients.Client(agent.ConnectionId).ExecuteTemplate(template, handler, arguments);
                        logger.LogInformation("Agent {0} started execution of template {1}", agent.Agent.Name, template);
                        break;
                    }
                }
                while (availableAgents.TryPop(out AgentConnection agent))
                {
                    agentsInGroup.Enqueue(agent);
                }
                return;
            }
            logger.LogWarning("No agents are available in group : {0} to proces request for template : {1} with handler : {2}", group, template, handler);
        }
        finally
        {
            semaphore.Release();
        }
    }   
}

