using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Serilog;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading;
using IComponent = Pixel.Automation.Core.Interfaces.IComponent;

namespace Pixel.Automation.Core.Components.Sequences
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Sequence", "Sequences", iconSource: null, description: "Represents a sequence of  steps within an application", tags: new string[] { "Automation Sequence" })]
    public class SequenceEntity : Entity , IApplicationContext , IDisposable
    {
        string targetAppId = string.Empty;
        [DataMember]
        [ReadOnly(true)]
        [Display(Name = "Application Id", Order = 10, GroupName = "Application Details")]
        public string TargetAppId
        {
            get
            {
                return targetAppId;
            }
            set
            {
                targetAppId = value;
                OnPropertyChanged(nameof(TargetAppId));
            }
        }

        [DataMember]
        [Display(Name = "Requires Focus", Order = 20, GroupName = "Application Details")]
        [Description("If set to true, target application window will be brought in to focus before processing child components")]
        public bool RequiresFocus { get; set; } = false;


        double acquireFocusTimeout = 3;
        [DataMember]
        [Display(Name = "Timeout", Order = 30, GroupName = "Application Details")]
        [Description("Maximum amount of time(seconds) to wait before focus can be acquired")]
        /// <summary>
        /// Maximum amount of time(seconds ) to wait before focus can be acquired.
        /// </summary>
        public double AcquireFocusTimeout
        {
            get => acquireFocusTimeout;
            set
            {
                if (value > 0)
                {
                    acquireFocusTimeout = value;
                }
            }
        }

        [NonSerialized]
        Mutex mutex = default;
    
        private readonly string mutexName = "Local\\Pixel.AppFocus";
        private bool wasMutexAcquired = false;

        public SequenceEntity() : base("Sequence","Sequence")
        {

        }


        public override void BeforeProcess()
        {
            if (!string.IsNullOrEmpty(this.targetAppId) && RequiresFocus)
            {
                if(mutex==null)
                {
                    //Log.Information($"Mutex acuried by thread with id : {Thread.CurrentThread.ManagedThreadId}");                  
                    mutex = new Mutex(false, mutexName);
                }

                Log.Information($"Waiting to acquire focus : {this}");
                if (mutex.WaitOne(TimeSpan.FromSeconds(acquireFocusTimeout)))
                {
                    Log.Information($"Focus acquired by {this}");
                    wasMutexAcquired = true;
                    IApplication targetApp = this.EntityManager.GetApplicationDetails(this);
                    IntPtr hWnd = targetApp.Hwnd;
                    if (hWnd != IntPtr.Zero)
                    {
                        IApplicationWindowManager windowManager = this.EntityManager.GetServiceOfType<IApplicationWindowManager>();
                        ApplicationWindow appWindow = windowManager.FromHwnd(hWnd);
                        windowManager.SetForeGroundWindow(appWindow);
                    }
                    else
                    {
                        throw new Exception($"Handle of the application is 0. Failed to focus application window.");
                    }
                }
                else
                {
                    wasMutexAcquired = false;
                    throw new TimeoutException($"Failed to acquire focus  within configured timeout of  {this.acquireFocusTimeout} ms.");
                }
            }
        }

        public override void OnCompletion()
        {
            if(mutex!=null)
            {
                //Log.Information($"Mutex will be released by thread with id : {Thread.CurrentThread.ManagedThreadId}");
                mutex.ReleaseMutex();
                //mutex.Dispose();
                //mutex = null;
                Log.Information($"Focus released by {this}");
            }
          
            base.OnCompletion();
        }


        public override void OnFault(IComponent faultingComponent)
        {
            if (mutex != null)
            {
                //Log.Information($"Mutex will be released by thread with id : {Thread.CurrentThread.ManagedThreadId}");
                if (wasMutexAcquired)
                {
                    mutex.ReleaseMutex();

                }
                //mutex.Dispose();
                //mutex = null;
                Log.Information($"Focus mutex released by {this}");
            }

            base.OnFault(faultingComponent);
        }

        public void SetAppContext(string targetAppId)
        {
            this.targetAppId = targetAppId;
            OnPropertyChanged(nameof(TargetAppId));
        }

        public string GetAppContext()
        {
            return this.targetAppId;
        }

        public void Dispose()
        {
            Dispose(true);         
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                mutex = null;
            }
        }
    }
}
