using Pixel.Automation.Core.Interfaces;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core
{
    /// <summary>
    /// Base class for all the application types
    /// </summary>
    public abstract class Application : NotifyPropertyChanged, IApplication
    {     
        /// <summary>
        /// Unique identifier for the application
        /// </summary>
        [DataMember(IsRequired = true, Order = 10)]
        [Browsable(false)]
        public string ApplicationId { get; set; } = Guid.NewGuid().ToString();

        string applicationName;
        /// <summary>
        /// Name of the application
        /// </summary>
        [DataMember(IsRequired = true, Order = 20)]
        [Browsable(false)]
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

        /// <summary>
        /// Target application details
        /// </summary>
        [Browsable(false)]
        public ApplicationProcess TargetApplication { get; set; }


        /// <summary>
        /// ProcessId of the target application
        /// </summary>
        [Browsable(false)]
        public int ProcessId
        {
            get
            {
                if (!(this.TargetApplication?.HasExited ?? true))
                {
                    return this.TargetApplication.Process.Id;
                }
                return 0;
            }
        }

        /// <summary>
        /// Main Window Handle of the target application
        /// </summary>
        [Browsable(false)]
        public IntPtr Hwnd
        {
            get
            {
                if (!(this.TargetApplication?.HasExited ?? true))
                {
                    return this.TargetApplication.Process.MainWindowHandle;
                }
                return IntPtr.Zero;
            }
        }      

        public override string ToString()
        {
            return $"{this.GetType().Name} -> Application Name : {this.ApplicationName} | ProcessId : {this.ProcessId}";
        }
    }
}
