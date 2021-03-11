using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Pixel.Automation.UIA.Components.ActorComponents
{

    [DataContract]
    [Serializable]
    [ToolBoxItem("Launch", "UIA", "Application", iconSource: null, description: "Launch target application", tags: new string[] { "Launch", "UIA" })]

    public class LaunchApplicationActorComponent : ActorComponent
    {
        private readonly ILogger logger = Log.ForContext<LaunchApplicationActorComponent>();


        [RequiredComponent]
        [Browsable(false)]
        public WinApplication ApplicationDetails
        {
            get
            {
                return this.EntityManager.GetOwnerApplication<WinApplication>(this);
            }
        }

        public LaunchApplicationActorComponent() : base("Launch", "WinApplicationLauncher")
        {       
           
        }


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
      
    }
}
