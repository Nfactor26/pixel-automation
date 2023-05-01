using System;
using System.Collections.Generic;

namespace Pixel.Persistence.Core.Models
{
    public class AgentDetails
    {     
        /// <summary>
        /// Unique Name of the agent
        /// </summary>
        public string Name { get;  set; }

        /// <summary>
        /// Group to which agent belongs
        /// </summary>
        public string Group { get;  set; }   

        /// <summary>
        /// Name of the machine where agent is running
        /// </summary>
        public string MachineName { get; set; }
        
        /// <summary>
        /// Description of operating system where agent is running
        /// </summary>
        public string OSDescription { get; set; }

        /// <summary>
        /// Execution handlers supported by agent
        /// </summary>
        public List<string> RegisteredHadlers { get; set; }

        /// </inheritdoc>      
        public override bool Equals(object obj)
        {
            if(obj is AgentDetails agent)
            {
                return this.Name.Equals(agent.Name);
            }
            return false;
        }

        /// </inheritdoc>    
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Name);
        }
    }

    public class AgentConnection
    {
        /// <summary>
        /// ConnectionId of the agent with hub
        /// </summary>
        public string ConnectionId { get; set;}
                 
        /// <summary>
        /// Details of the Agent
        /// </summary>
        public AgentDetails Agent { get; set; }       

        /// <summary>
        /// Indicates if an agent is connected
        /// </summary>
        public bool IsActive => !string.IsNullOrEmpty(ConnectionId);

    }
}
