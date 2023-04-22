using MongoDB.Bson.Serialization.Attributes;
using System;
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
        /// Identifies the handler that will be used to execute the template when trigger occurs
        /// </summary>
        [Required]
        [DataMember]
        public string Handler { get; set; }

        /// <summary>
        /// Indicates if trigger is enabled
        /// </summary>
        [Required]
        [DataMember]
        public bool IsEnabled { get; set; } = true;

        public abstract object Clone();

        public abstract bool Equals(SessionTrigger other);
        
    }

    /// <summary>
    /// Trigger based on a cron expression to execute template on a scheduled basis
    /// </summary>
    [DataContract]
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
                Handler = this.Handler,
                IsEnabled = this.IsEnabled,
                CronExpression = this.CronExpression
            };
        }

        public override bool Equals(SessionTrigger other)
        {
            return other is CronSessionTrigger cst && cst.Handler.Equals(this.Handler) && cst.CronExpression.Equals(this.CronExpression);
        }

        public bool Equals(CronSessionTrigger other)
        {
            return this.Equals(other);
        }
    }
}
