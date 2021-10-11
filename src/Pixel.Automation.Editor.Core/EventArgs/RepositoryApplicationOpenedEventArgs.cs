using System;

namespace Pixel.Automation.Editor.Core
{
    public class RepositoryApplicationOpenedEventArgs : EventArgs
    {
        public string ApplicationName { get; }

        public string ApplicationId { get; }

        public RepositoryApplicationOpenedEventArgs(string applicationName,string applicationId) : base()
        {
            this.ApplicationName = applicationName;
            this.ApplicationId = applicationId;
        }

    }
}
