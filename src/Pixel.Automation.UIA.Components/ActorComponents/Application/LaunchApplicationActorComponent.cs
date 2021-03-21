using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
    /// <summary>
    /// Use <see cref="LaunchApplicationActorComponent"/> to launch an executable file.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Launch", "UIA", "Application", iconSource: null, description: "Launch target application", tags: new string[] { "Launch", "UIA" })]
    public class LaunchApplicationActorComponent : ActorComponent
    {
        private readonly ILogger logger = Log.ForContext<LaunchApplicationActorComponent>();

        /// <summary>
        /// Owner application details
        /// </summary>
        [RequiredComponent]
        [Browsable(false)]
        public WinApplication ApplicationDetails
        {
            get
            {
                return this.EntityManager.GetOwnerApplication<WinApplication>(this);
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public LaunchApplicationActorComponent() : base("Launch", "WinApplicationLauncher")
        {       
           
        }

        /// <summary>
        /// Launch the executable for owner application
        /// </summary>
        public override void Act()
        {
            var appDetails = ApplicationDetails;
            string executablePath = appDetails.ExecutablePath;
            if(!string.IsNullOrEmpty(executablePath))
            {
                ProcessStartInfo procInfo = new ProcessStartInfo(executablePath);
                procInfo.WindowStyle = appDetails.WindowStyle;
                procInfo.UseShellExecute = appDetails.UseShellExecute;
                procInfo.WorkingDirectory = appDetails.WorkingDirectory;
                procInfo.Arguments = appDetails.LaunchArguments;

                Application targetApp = Application.Launch(procInfo);
                appDetails.TargetApplication = targetApp;
                logger.Information($"Launch completed for application : {executablePath} with processId : {targetApp.Process.Id}");
                logger.Information($"Working directory of application is : {procInfo.WorkingDirectory}");
                logger.Information("Process details are : {appDetails}", appDetails);
                return;
            }
            logger.Error($"Launch failed for application : {appDetails.ExecutablePath}");
            throw new ArgumentException($"Please verify configured application path : {executablePath}");
        }

        public override string ToString()
        {
            return "Launch Application Actor";
        }
    }
}
