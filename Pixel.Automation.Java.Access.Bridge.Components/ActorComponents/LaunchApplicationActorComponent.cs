﻿using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;

namespace Pixel.Automation.Java.Access.Bridge.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Launch", "Java", "Application", iconSource: null, description: "Launch target application", tags: new string[] { "Launch", "Java" })]

    public class LaunchApplicationActorComponent : ActorComponent
    {
        private readonly ILogger logger = Log.ForContext<LaunchApplicationActorComponent>();

        [RequiredComponent]
        [Browsable(false)]
        public JavaApplication ApplicationDetails
        {
            get
            {
                return this.EntityManager.GetOwnerApplication<JavaApplication>(this);
            }
        }

        public LaunchApplicationActorComponent() : base("Launch", "JavaApplicationLauncher")
        {

        }


        public override void Act()
        {
            var appDetails = ApplicationDetails;
            string executablePath = appDetails.ExecutablePath;
            if (!string.IsNullOrEmpty(executablePath) && File.Exists(executablePath))
            {
                ProcessStartInfo procInfo = new ProcessStartInfo("java");
                procInfo.WindowStyle = appDetails.WindowStyle;
                procInfo.UseShellExecute = appDetails.UseShellExecute;
                procInfo.WorkingDirectory = Path.GetDirectoryName(appDetails.WorkingDirectory);
                procInfo.Arguments = $"-jar {executablePath} {appDetails.LaunchArguments}";

                Application targetApp = Application.Launch(procInfo);
                appDetails.TargetApplication = targetApp;
                logger.Information("Launch completed for application : {$executablePath}", executablePath);
                logger.Information("Process details are : {appDetails}", appDetails);
                return;
            }          
            throw new ArgumentException($"Application path : {executablePath} empty or doesn't exist.");
        }

    }
}