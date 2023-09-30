using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.Core.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Java.Access.Bridge.Components
{
    public class JavaApplicationEntity : ApplicationEntity
    {
        [Browsable(false)]
        public string Platform { get; } = "Java";

        /// <summary>
        /// Optional argument that can be used to over-ride the location of jar file set on application.
        /// </summary>
        [DataMember]
        [Display(Name = "Executable Override", GroupName = "Overrides", Order = 10, Description = "[Optional] Override the jar file path set on application")]
        public Argument ExecutableOverride { get; set; } = new InArgument<string>() { Mode = ArgumentMode.DataBound, CanChangeType = false };

        /// <summary>
        /// Optional argument that can be used to over-ride the working directory of application
        /// </summary>
        [DataMember]
        [Display(Name = "Working Directory Override", GroupName = "Overrides", Order = 10, Description = "[Optional] Override the working directory of application.")]
        public Argument WorkingDirectoryOverride { get; set; } = new InArgument<string>() { Mode = ArgumentMode.DataBound, CanChangeType = false };
            

        ///<inheritdoc/>
        public override async Task LaunchAsync()
        {
            var javaApplicationDetails = this.GetTargetApplicationDetails<JavaApplication>();
            if (!(javaApplicationDetails.TargetApplication?.HasExited ?? true))
            {
                logger.Warning($"{javaApplicationDetails.ApplicationName} is already running.");
                await Task.CompletedTask;
                return;
            }

            string executablePath = javaApplicationDetails.ExecutablePath;
            if (this.ExecutableOverride.IsConfigured())
            {
                executablePath = await this.ArgumentProcessor.GetValueAsync<string>(this.ExecutableOverride);
                logger.Information($"Executable Path was over-ridden to {executablePath} for application : {javaApplicationDetails.ApplicationName}");
            }

            string workingDirectory = javaApplicationDetails.WorkingDirectory;
            if (this.WorkingDirectoryOverride.IsConfigured())
            {
                workingDirectory = await this.ArgumentProcessor.GetValueAsync<string>(this.WorkingDirectoryOverride);
                logger.Information($"Working Directory was over-ridden to {workingDirectory} for application : {javaApplicationDetails.ApplicationName}");
            }


            if (!string.IsNullOrEmpty(executablePath))
            {
                ProcessStartInfo procInfo = new ProcessStartInfo("java");
                procInfo.WindowStyle = javaApplicationDetails.WindowStyle;
                procInfo.UseShellExecute = javaApplicationDetails.UseShellExecute;
                procInfo.WorkingDirectory = workingDirectory;
                procInfo.Arguments = $"-jar {executablePath} {javaApplicationDetails.LaunchArguments}";

                ApplicationProcess targetApp = ApplicationProcess.Launch(procInfo);
                javaApplicationDetails.TargetApplication = targetApp;
                logger.Information($"Launch completed for application : {javaApplicationDetails.ApplicationName} with processId : {targetApp.Process.Id}");
                logger.Information($"Working directory of application is : {procInfo.WorkingDirectory}");
                return;
            }

            logger.Error($"Launch failed for application : {javaApplicationDetails.ApplicationName}");
            throw new ConfigurationException($"Executable Path is not configured.");
        }

        public override async Task CloseAsync()
        {
            var javaApplicationDetails = this.GetTargetApplicationDetails<JavaApplication>();
            if (!(javaApplicationDetails.TargetApplication?.HasExited ?? true))
            {
                javaApplicationDetails.TargetApplication.Close();
                logger.Information($"Application : {javaApplicationDetails.ApplicationName} was closed");
                await Task.CompletedTask;
            }
        }

        ///<inheritdoc/>
        public override async Task CaptureScreenShotAsync(string filePath)
        {
            if (this.AllowCaptureScreenshot)
            {
                var screenCapture = this.EntityManager.GetServiceOfType<IScreenCapture>();
                var windowManager = this.EntityManager.GetServiceOfType<IApplicationWindowManager>();
                var appRectangle = windowManager.GetWindowSize(this.applicationDetails.Hwnd);
                var screenShotBytes = screenCapture.CaptureArea(appRectangle);
                using (var memoryStream = new MemoryStream(screenShotBytes))
                {
                    using (var bitmap = new Bitmap(memoryStream))
                    {
                        using (FileStream fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
                        {
                            bitmap.Save(fs, ImageFormat.Jpeg);
                        }
                    }
                }
                await Task.CompletedTask;
            }           
        }

    }
}
