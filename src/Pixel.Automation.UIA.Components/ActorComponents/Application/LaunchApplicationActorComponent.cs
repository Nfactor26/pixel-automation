using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Pixel.Automation.UIA.Components.ActorComponents
{
    /// <summary>
    /// Use <see cref="LaunchApplicationActorComponent"/> to launch an executable file.
    /// </summary>
    [DataContract]
    [Serializable]
    [ToolBoxItem("Launch", "Application", "UIA", iconSource: null, description: "Launch target application", tags: new string[] { "Launch", "UIA" })]
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
        /// Optional argument that can be used to over-ride the location of executable set on application.
        /// </summary>
        [DataMember]
        [Display(Name = "Executable Override", GroupName = "Configuration", Order = 10, Description = "[Optional] Override the executable path set on application")]
        public Argument ExecutableOverride { get; set; } = new InArgument<string>() { Mode = ArgumentMode.DataBound, CanChangeType = false };

        /// <summary>
        /// Optional argument that can be used to over-ride the working directory of application
        /// </summary>
        [DataMember]
        [Display(Name = "Working Directory Override", GroupName = "Configuration", Order = 10, Description = "[Optional] Override the working directory of application.")]
        public Argument WorkingDirectoryOverride { get; set; } = new InArgument<string>() { Mode = ArgumentMode.DataBound, CanChangeType = false };


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
            if(this.ExecutableOverride.IsConfigured())
            {
                executablePath = this.ArgumentProcessor.GetValue<string>(this.ExecutableOverride);
                logger.Information("Executable location was over-ridden to {executablePath}", executablePath);
            }

            string workingDirectory = appDetails.WorkingDirectory;
            if(this.WorkingDirectoryOverride.IsConfigured())
            {
                workingDirectory = this.ArgumentProcessor.GetValue<string>(this.WorkingDirectoryOverride);
                logger.Information("Working directory was over-ridden to {workingDirectory}", workingDirectory);
            }

            if (!string.IsNullOrEmpty(executablePath))
            {
                ProcessStartInfo procInfo = new ProcessStartInfo(executablePath);
                procInfo.WindowStyle = appDetails.WindowStyle;
                procInfo.UseShellExecute = appDetails.UseShellExecute;
                procInfo.WorkingDirectory = workingDirectory;
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
