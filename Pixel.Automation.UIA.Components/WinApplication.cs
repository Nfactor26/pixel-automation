using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.UIA.Components
{
    [DataContract]
    [Serializable]
    [DisplayName("Windows App")]
    [Description("WPF or Win32 based applications using UIA")]
    [ControlLocator(typeof(UIAControlLocatorComponent))]
    public class WinApplication : Entity, IApplication, IDisposable
    {

        string executablePath;
        /// <summary>
        /// Path of the executable file
        /// </summary>
        [DataMember(IsRequired = true, Order = 10)]
        [Description("Path of the executable file")]
        public string ExecutablePath
        {
            get
            {
                return executablePath;
            }

            set
            {
                executablePath = value;
            }
        }


        ProcessWindowStyle windowStyle;
        /// <summary>
        /// Configure if the applcation in started in hidden/minimized/maximized/normal state
        /// </summary>
        [DataMember(IsRequired = true, Order = 20)]
        [Description("Configure if the applcation in started in hidden/minimized/maximized/normal state")]
        public ProcessWindowStyle WindowStyle
        {
            get
            {
                return windowStyle;
            }
            set
            {
                windowStyle = value;
            }
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
            get
            {
                return useShellExecute;
            }
            set
            {
                useShellExecute = value;
            }
        }


        string launchArguments;
        /// <summary>
        /// Arguments for starting the process
        /// </summary>
        [DataMember(IsRequired = false, Order = 40)]
        [Description("Arugments for starting the process if any")]
        public string LaunchArguments
        {
            get
            {
                return launchArguments;
            }
            set
            {
                launchArguments = value;
            }
        }

        [NonSerialized]
        Application targetApplication;
        [Browsable(false)]
        public Application TargetApplication
        {
            get
            {
                return targetApplication;
            }

            set
            {
                targetApplication = value;
            }
        }
    
        #region IApplication

        [DataMember(IsRequired = true, Order = 10)]
        [Browsable(false)]
        public string ApplicationId { get; set; } = Guid.NewGuid().ToString();

        string applicationName;
        [DataMember(IsRequired = true, Order = 20)]
        public string ApplicationName
        {
            get
            {
                return applicationName;
            }
            set
            {
                applicationName = value;
                OnPropertyChanged();
            }
        }      

        [Browsable(false)]
        public int ProcessId
        {
            get
            {
                if (this.targetApplication != null && !this.targetApplication.HasExited)
                {
                    return this.targetApplication.Process.Id;
                }
                return 0;
            }
        }

        [Browsable(false)]
        public IntPtr Hwnd
        {
            get
            {
                if (this.targetApplication != null && !this.targetApplication.HasExited)
                {
                    return this.targetApplication.Process.MainWindowHandle;
                }
                return IntPtr.Zero;
            }
        }

        #endregion IApplication

        public WinApplication() : base("Executable Details", "WinApplicationDetails")
        {

        }

        public override bool ValidateComponent()
        {
            base.ValidateComponent();
            if (Path.GetExtension(this.executablePath) != ".exe")
            {
                isValid = false;
            }
            OnPropertyChanged("IsValid");
            return isValid;
        }

        public override void ResolveDependencies()
        {
            if (this.Parent.GetComponentsOfType<UIAControlLocatorComponent>().Count() == 0)
                this.Parent.AddComponent(new UIAControlLocatorComponent());
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if(isDisposing)
            {
                if (this.targetApplication != null)
                {
                    if (!this.targetApplication.HasExited)
                    {
                        this.targetApplication.Close();
                    }
                }
            }            
        }

        //public override bool Equals(object obj)
        //{
        //    if(obj is WinApplication other)
        //    {
        //        if(this.ExecutablePath.Equals(other.executablePath) && this.launchArguments.Equals(other.launchArguments) && this.useShellExecute.Equals(
        //            other.useShellExecute) && this.windowStyle.Equals(other.windowStyle))
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
    }
}
