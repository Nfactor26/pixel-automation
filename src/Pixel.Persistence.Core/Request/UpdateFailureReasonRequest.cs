using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Request
{
    [DataContract]
    public class UpdateFailureReasonRequest
    {
        [DataMember]
        public string SessionId { get; set; }

        [DataMember]
        public string TestId { get; set; }

        [DataMember]
        public string FailureReason { get; set; }

        public UpdateFailureReasonRequest()
        {

        }

        public UpdateFailureReasonRequest(string sessionId, string testId, string failureReason)
        {
            this.SessionId = sessionId;
            this.TestId = testId;
            this.FailureReason = failureReason;
        }
    }
}
