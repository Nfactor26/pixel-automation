using Dawn;
using System;

namespace Pixel.Automation.Core.Args
{
    public class ApplicationUpdatedEventArgs : EventArgs
    {
        public string ApplicationId { get; set; }

        public ApplicationUpdatedEventArgs(string applicationId)
        {
            Guard.Argument(applicationId).NotNull().NotEmpty();
            this.ApplicationId = applicationId;
        }
    }
}
