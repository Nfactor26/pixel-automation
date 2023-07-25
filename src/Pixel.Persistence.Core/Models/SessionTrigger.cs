using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Pixel.Persistence.Core.Models
{
    /// <summary>
    /// Defines a trigger that can be used to initiate execution of the template 
    /// </summary>
    [DataContract] 
    [JsonDerivedType(typeof(CronSessionTrigger), typeDiscriminator: nameof(CronSessionTrigger))]
    [BsonKnownTypes(typeof(CronSessionTrigger))]
    public abstract class SessionTrigger : ICloneable, IEquatable<SessionTrigger>
    {
        /// <summary>
        /// Display name for the trigger
        /// </summary>
        [Required]
        [DataMember(Order = 10)]
        public string Name { get; set; }

        /// <summary>
        /// Identifies the handler that will be used to execute the template when trigger occurs
        /// </summary>
        [Required]
        [DataMember(Order = 20)]
        public string Handler { get; set; }

        /// <summary>
        /// Group from which an agent must be selected to process the request
        /// </summary>
        [Required]
        [DataMember(Order = 30)]
        public string Group { get; set; }               
    
        /// <summary>
        /// Indicates if trigger is enabled
        /// </summary>
        [Required]
        [DataMember(Order = 40)]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Parameters for the trigger.
        /// These parameterrs will override any parameter defined on template handler
        /// </summary>  
        [DataMember(Order = 50, IsRequired = false)]
        public Dictionary<string, string> Parameters { get; set; } = new();


        public abstract object Clone();

        public abstract bool Equals(SessionTrigger other);
        
    }

    /// <summary>
    /// Trigger based on a cron expression to execute template on a scheduled basis
    /// </summary>
    [DataContract]
    [JsonDerivedType(typeof(CronSessionTrigger), typeDiscriminator: nameof(CronSessionTrigger))]
    public class CronSessionTrigger : SessionTrigger, IEquatable<CronSessionTrigger>
    {  
        /// <summary>
        /// Cron expression
        /// </summary>
        [Required]
        [DataMember]
        public string CronExpression { get; set; }

        public override object Clone()
        {
            return new CronSessionTrigger()
            {
                Name = this.Name,
                Handler = this.Handler,
                Group = this.Group,
                IsEnabled = this.IsEnabled,
                Parameters = new Dictionary<string, string>(this.Parameters),
                CronExpression = this.CronExpression
            };
        }

        public override bool Equals(SessionTrigger other)
        {
            return other is CronSessionTrigger cst && cst.Name.Equals(this.Name) &&
                cst.Handler.Equals(this.Handler) && cst.CronExpression.Equals(this.CronExpression);
        }

        public bool Equals(CronSessionTrigger other)
        {
            return this.Equals(other);
        }
    }
}
