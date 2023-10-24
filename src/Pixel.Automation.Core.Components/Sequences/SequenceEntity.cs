using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Serilog;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Sequences;

[DataContract]
[Serializable]
public class SequenceEntity : Entity , IApplicationContext
{
    private readonly ILogger logger = Log.ForContext<SequenceEntity>();

    string targetAppId = string.Empty;
    [DataMember(Order = 500)]
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

    [DataMember(Order = 200)]
    [Display(Name = "Requires Focus", Order = 20, GroupName = "Application Details")]
    [Description("If set to true, target application window will be brought in to focus before processing child components")]
    public bool RequiresFocus { get; set; } = false;


    public SequenceEntity() : base("Sequence", "Sequence")
    {

    }

    public override async Task BeforeProcessAsync()
    {
        if (!string.IsNullOrEmpty(this.targetAppId) && RequiresFocus)
        {      
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
}
