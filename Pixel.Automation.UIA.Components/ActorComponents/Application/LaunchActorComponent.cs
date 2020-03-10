using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
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

    public class WinApplicationLauncherComponent : ActorComponent
    {
        [RequiredComponent]
        [Browsable(false)]
        public WinApplication ApplicationDetails
        {
            get
            {
                return this.EntityManager.GetApplicationDetails<WinApplication>(this);
            }
        }

        public WinApplicationLauncherComponent() : base("Launch", "WinApplicationLauncher")
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
                procInfo.Arguments = appDetails.LaunchArguments;

                Application targetApp = Application.Launch(procInfo);
                appDetails.TargetApplication = targetApp;
                Log.Information("Launch completed for application : {$executablePath}",appDetails.ExecutablePath);
                Log.Information("Process details are : {appDetails}", appDetails);
                return;
            }
            Log.Information("Launch failed for application : {$executablePath}", appDetails.ExecutablePath);
            throw new ArgumentException($"Please verify configured application path : {executablePath}");
        }
      
    }
}
