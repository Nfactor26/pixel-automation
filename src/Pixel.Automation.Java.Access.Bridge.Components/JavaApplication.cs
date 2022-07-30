using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;

namespace Pixel.Automation.Java.Access.Bridge.Components
{
    [DataContract]
    [Serializable]
    [DisplayName("Java App")]
    [Description("Java based application that support automation using java access bridge")]
    [ControlLocator(typeof(JavaControlLocatorComponent))]
    [ApplicationEntity(typeof(JavaApplicationEntity))]
    public class JavaApplication : Application
    {             
        /// <summary>
        /// Path of the executable file
        /// </summary>
        [DataMember(IsRequired = true, Order = 10)]
        [Description("Path of the executable file")]
        public string ExecutablePath { get; set; }
      

        string workingDirectory;
        /// <summary>
        /// Working directory of the application. Defaults to executable path if not specified.
        /// </summary>
        [DataMember(IsRequired = true, Order = 15)]
        [Description("Working directory of the application")]
        public string WorkingDirectory
        {
            get => workingDirectory ?? (Path.IsPathRooted(ExecutablePath) ? Path.GetDirectoryName(ExecutablePath) : "");
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
