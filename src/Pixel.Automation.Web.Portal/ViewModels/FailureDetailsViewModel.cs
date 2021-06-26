using Pixel.Persistence.Core.Models;
using System;

namespace Pixel.Automation.Web.Portal.ViewModels
{
    public class FailureDetailsViewModel
    {
        private FailureDetails failureDetails;
       
        public string SessionId { get; set; }

        public string TestId { get; set; }
        
        public string Exception => failureDetails.Exception;

        public string ShortMessage
        {
            get
            {
                if (Message.Length <= 15)
                {
                    return Message;
                }
                return $"{Message.Substring(0, 13)}...";
            }
        }

        public string Message => failureDetails.Message;

        public string StackTrace => failureDetails.StackTrace;

        public DateTime FailedOn => failureDetails.FailedOn.ToLocalTime();

        private string failureReason;
        public string FailureReason
        {
            get => failureReason;
            set => failureReason = value;
        }

        public bool IsDetailedViewVisible { get; set; }
        public FailureDetailsViewModel(FailureDetails failureDetails)
        {
            this.failureDetails = failureDetails;
            this.failureReason = failureDetails.FailureReason;
        }

        public FailureDetailsViewModel(string sessionId, string testId, FailureDetails failureDetails) : this(failureDetails)
        {
            this.SessionId = sessionId;
            this.TestId = testId;         
        }      
    }
}
