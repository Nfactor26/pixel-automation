﻿using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Exceptions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Pixel.Automation.UIA.Components
{
    public class WinApplicationEntity : ApplicationEntity
    {
        [Browsable(false)]
        public string Platform { get; } = "Windows";

        /// <summary>
        /// Optional argument that can be used to over-ride the location of executable set on application.
        /// </summary>
        [DataMember]
        [Display(Name = "Executable Override", GroupName = "Overrides", Order = 10, Description = "[Optional] Override the executable path set on application")]
        public Argument ExecutableOverride { get; set; } = new InArgument<string>() { Mode = ArgumentMode.DataBound, CanChangeType = false };

        /// <summary>
        /// Optional argument that can be used to over-ride the working directory of application
        /// </summary>
        [DataMember]
        [Display(Name = "Working Directory Override", GroupName = "Overrides", Order = 10, Description = "[Optional] Override the working directory of application.")]
        public Argument WorkingDirectoryOverride { get; set; } = new InArgument<string>() { Mode = ArgumentMode.DataBound, CanChangeType = false };


        [Browsable(false)]
        public override bool CanUseExisting => true;

        ///<inheritdoc/>
        public override void Launch()
        {
            var winApplicationDetalis = this.GetTargetApplicationDetails<WinApplication>();
            if (!(winApplicationDetalis.TargetApplication?.HasExited ?? true))
            {
                logger.Warning($"{winApplicationDetalis.ApplicationName} is already running.");
                return;
            }

            string executablePath = winApplicationDetalis.ExecutablePath;
            if (this.ExecutableOverride.IsConfigured())
            {
                executablePath = this.ArgumentProcessor.GetValue<string>(this.ExecutableOverride);
                logger.Information($"Executable Path was over-ridden to {executablePath} for application : {winApplicationDetalis.ApplicationName}");
            }

            string workingDirectory = winApplicationDetalis.WorkingDirectory;
            if (this.WorkingDirectoryOverride.IsConfigured())
            {
                workingDirectory = this.ArgumentProcessor.GetValue<string>(this.WorkingDirectoryOverride);
                logger.Information($"Working Directory was over-ridden to {workingDirectory} for application : {winApplicationDetalis.ApplicationName}");
            }


            if (!string.IsNullOrEmpty(executablePath))
            {
                ProcessStartInfo procInfo = new ProcessStartInfo(executablePath);
                procInfo.WindowStyle = winApplicationDetalis.WindowStyle;
                procInfo.UseShellExecute = winApplicationDetalis.UseShellExecute;
                procInfo.WorkingDirectory = workingDirectory;
                procInfo.Arguments = winApplicationDetalis.LaunchArguments;

                ApplicationProcess targetApp = ApplicationProcess.Launch(procInfo);
                winApplicationDetalis.TargetApplication = targetApp;
                logger.Information($"Launch completed for application : {winApplicationDetalis.ApplicationName} with processId : {targetApp.Process.Id}");
                logger.Information($"Working directory of application is : {procInfo.WorkingDirectory}");
                return;
            }

            logger.Error($"Launch failed for application : {winApplicationDetalis.ApplicationName}");
            throw new ConfigurationException($"Executable Path is not configured.");
        }

        public override void Close()
        {
            var winApplicationDetalis = this.GetTargetApplicationDetails<WinApplication>();
            if (!(winApplicationDetalis.TargetApplication?.HasExited ?? true))
            {
                winApplicationDetalis.TargetApplication.Close();
                logger.Information($"Application : {winApplicationDetalis.ApplicationName} was closed");
            }
        }

        ///<inheritdoc/>
        public override void UseExisting(ApplicationProcess application)
        {
            var winApplicationDetalis = this.GetTargetApplicationDetails<WinApplication>();
            winApplicationDetalis.TargetApplication = application;
        }
    }
}