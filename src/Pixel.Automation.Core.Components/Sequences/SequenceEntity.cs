using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Serilog;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using IComponent = Pixel.Automation.Core.Interfaces.IComponent;

namespace Pixel.Automation.Core.Components.Sequences
{
    [DataContract]
    [Serializable]
    public class SequenceEntity : Entity , IApplicationContext , IDisposable
    {
        private readonly ILogger logger = Log.ForContext<SequenceEntity>();

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
        [Description("Timeout(seconds) for acquiring lock on mutex in order to set window as foregroud window")]
        /// <summary>
        /// Timeout(seconds) for acquiring lock on mutex in order to set window as foregroud window
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

        public SequenceEntity() : base("Sequence", "Sequence")
        {

        }


        public override async Task BeforeProcessAsync()
        {
            if (!string.IsNullOrEmpty(this.targetAppId) && RequiresFocus)
            {
                if(mutex==null)
                {                           
                    mutex = new Mutex(false, mutexName);
                }

                logger.Debug($"Waiting to acquire lock on Mutex : {this}");
                if (mutex.WaitOne(TimeSpan.FromSeconds(acquireFocusTimeout)))
                {
                    logger.Debug($"Mutex lock acquired by {this}");
                    wasMutexAcquired = true;
                    IApplication targetApp = this.EntityManager.GetOwnerApplication(this);
                    IntPtr hWnd = targetApp.Hwnd;
                    if (hWnd != IntPtr.Zero)
                    {
                        IApplicationWindowManager windowManager = this.EntityManager.GetServiceOfType<IApplicationWindowManager>();
                        ApplicationWindow appWindow = windowManager.FromHwnd(hWnd);
                        windowManager.SetForeGroundWindow(appWindow);
                        logger.Information($"Window with hWnd : {hWnd} is set to foreground window");
                        return;
                    }
                   
                    throw new InvalidOperationException($"hWnd of the application window is 0. Can't set foreground window");                    
                }
                else
                {
                    wasMutexAcquired = false;
                    throw new TimeoutException($"Failed to acquire mutex lock  within configured timeout of  {this.acquireFocusTimeout} ms.");
                }
            }
            await Task.CompletedTask;
        }

        public override async Task OnCompletionAsync()
        {
            if(mutex!=null && wasMutexAcquired)
            {               
                mutex.ReleaseMutex();
                wasMutexAcquired = false;
                logger.Information($"Mutex lock released by {this}");
            }
            await Task.CompletedTask;
        }


        public override async Task OnFaultAsync(IComponent faultingComponent)
        {
            if (mutex != null && wasMutexAcquired)
            {
                mutex.ReleaseMutex();
                wasMutexAcquired = false;
                logger.Information($"Mutex lock released by {this}");
            }
            await Task.CompletedTask;
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
            if (isDisposing && mutex != null)
            {
                mutex.Dispose();
                mutex = null;              
            }
            wasMutexAcquired = false;
        }
    }
}
