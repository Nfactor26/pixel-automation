using System;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models
{
    [DataContract]
    public class FailureDetails
    {
        /// <summary>
        /// Type of exception due to which test failed
        /// </summary>
        [DataMember]
        public string Exception { get; set; }

        /// <summary>
        /// Error message from the exception
        /// </summary>
        [DataMember]
        public string Message { get; set; }

        /// <summary>
        /// Stack trace of the exception
        /// </summary>
        [DataMember]
        public string StackTrace { get; set; }

        /// <summary>
        /// Date on which failure happened
        /// </summary>
        [DataMember]
        public DateTime FailedOn { get; set; }

        /// <summary>
        /// Reason for failure identified by a user manually
        /// </summary>
        [DataMember(IsRequired = false)]
        public string FailureReason { get; set; }

        public FailureDetails()
        {

        }

        public FailureDetails(Exception exception)
        {
            this.Exception = exception.GetType().Name;
            this.Message = exception.Message;
            this.StackTrace = exception.StackTrace;
            this.FailedOn = DateTime.Now.ToUniversalTime();
        }


        public override bool Equals(object obj)
        {
            if(obj is FailureDetails other)
            {
                return other.Exception.Equals(this.Exception) && other.Message.Equals(this.Message);
            }
            return false;
        }
    }
}
