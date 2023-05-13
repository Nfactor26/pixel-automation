using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;

namespace Pixel.Automation.UIA.Components
{
    [DataContract]
    [Serializable]
    [DisplayName("Windows App")]
    [Description("WPF or Win32 based applications using UIA")]
    [ControlLocator(typeof(UIAControlLocatorComponent))]
    [ApplicationEntity(typeof(WinApplicationEntity))]
    [SupportedPlatforms("WINDOWS")]
    public class WinApplication : Application
    {      
        string executablePath;
        /// <summary>
        /// Path of the executable file
        /// </summary>
        [DataMember(IsRequired = true, Order = 10)]
        [Description("Path of the executable file")]
        public string ExecutablePath
        {
            get => executablePath;
            set => executablePath = value;            
        }

        string workingDirectory;
        /// <summary>
        /// Working directory of the application. Defaults to executable path if not specified.
        /// </summary>
        [DataMember(IsRequired = true, Order = 15)]
        [Description("Working directory of the application")]
        public string WorkingDirectory
        {
            get => workingDirectory ?? (Path.IsPathRooted(executablePath) ? Path.GetDirectoryName(executablePath) : "");
            set => workingDirectory = value;
        }

        ProcessWindowStyle windowStyle;
        /// <summary>
        /// Configure if the applcation in started in hidden/minimized/maximized/normal state
        /// </summary>
        [DataMember(IsRequired = true, Order = 20)]
        [Description("Configure if the applcation in started in hidden/minimized/maximized/normal state")]
        public ProcessWindowStyle WindowStyle
        {
            get => windowStyle;
            set => windowStyle = value;            
        }

        bool useShellExecute = false;
        /// <summary>
        /// Gets or sets a value indicating whether to use the operating system shell to start the process.
        /// https://msdn.microsoft.com/en-us/library/system.diagnostics.processstartinfo.useshellexecute(v=vs.110).aspx
        /// </summary>
        [DataMember(IsRequired = true, Order = 30)]
        [Description("Gets or sets a value indicating whether to use the operating system shell to start the process. see https://msdn.microsoft.com/en-us/library/system.diagnostics.processstartinfo.useshellexecute(v=vs.110).aspx")]
        public bool UseShellExecute
        {
            get => useShellExecute;
            set => useShellExecute = value;
        }

        string launchArguments;
        /// <summary>
        /// Arguments for starting the process
        /// </summary>
        [DataMember(IsRequired = false, Order = 40)]
        [Description("Arugments for starting the process if any")]
        public string LaunchArguments
        {
            get => launchArguments;
            set => launchArguments = value;          
        }
    }
}
