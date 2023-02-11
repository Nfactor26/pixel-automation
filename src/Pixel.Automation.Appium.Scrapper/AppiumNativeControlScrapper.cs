using Caliburn.Micro;
using Dawn;
using Gma.System.MouseKeyHook;
using Notifications.Wpf.Core;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Editor.Notifications;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pixel.Automation.Appium.Scrapper;

/// <summary>
/// Base class for appium based control scrapper for mobile devices 
/// </summary>
public abstract class AppiumNativeControlScrapper : PropertyChangedBase, IControlScrapper, IHandle<RepositoryApplicationOpenedEventArgs>
{
    protected readonly ILogger logger = Log.ForContext<AndroidNativeControlScrapper>();
    protected readonly IEventAggregator eventAggregator;
    protected readonly IWindowManager windowManager;
    protected readonly IHighlightRectangle highlightRectangle;
    protected string targetApplicationId = string.Empty;
    protected IKeyboardMouseEvents m_GlobalHook;
    protected readonly IScreenCapture screenCapture;
    protected readonly INotificationManager notificationManager; 
    protected InspectorViewModel inspector;
    protected System.Windows.Forms.Timer captureTimer = new System.Windows.Forms.Timer();
    protected ConcurrentQueue<ScrapedControl> capturedControls = new();
        
    /// </inheritdoc>
    public string DisplayName { get; protected set; }     

    bool isCapturing;
    /// </inheritdoc>
    public bool IsCapturing
    {
        get => isCapturing;
        set
        {
            isCapturing = value;
            NotifyOfPropertyChange(() => IsCapturing);
        }
    }

    /// </inheritdoc>
    public bool CanToggleScrapper
    {
        get
        {
            return !(string.IsNullOrEmpty(this.targetApplicationId));
        }
    }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="eventAggregator"></param>
    /// <param name="windowManager"></param>
    /// <param name="highlightRectangleFactory"></param>
    /// <param name="screenCapture"></param>
    /// <param name="notificationManager"></param>
    public AppiumNativeControlScrapper(IEventAggregator eventAggregator, IWindowManager windowManager,
        IHighlightRectangleFactory highlightRectangleFactory, IScreenCapture screenCapture, INotificationManager notificationManager)
    {      
        this.eventAggregator = Guard.Argument(eventAggregator, nameof(eventAggregator)).Value;
        this.windowManager = Guard.Argument(windowManager, nameof(windowManager)).Value;
        this.eventAggregator.SubscribeOnUIThread(this);
        this.highlightRectangle = Guard.Argument(highlightRectangleFactory, nameof(highlightRectangleFactory)).NotNull().Value.CreateHighlightRectangle();
        this.highlightRectangle.BorderColor = "Orange";
        this.screenCapture = Guard.Argument(screenCapture, nameof(screenCapture)).NotNull().Value;
        this.notificationManager = Guard.Argument(notificationManager, nameof(notificationManager)).Value;
    }
    
    ///</inheritdoc>
    public async Task ToggleCapture()
    {
        if (IsCapturing)
        {
            await StartCapture();
        }
        else
        {
            await StopCapture();
        }
    }

    ///</inheritdoc>
    public async Task StartCapture()
    {
        capturedControls = new();
        capturedControls.Clear();
        this.IsCapturing = true;

        if (m_GlobalHook == null)
            m_GlobalHook = Hook.GlobalEvents();
        m_GlobalHook.MouseDownExt += GlobalHookMouseDownExt;

        System.Windows.Application.Current.MainWindow.Hide();
        Thread.Sleep(1000);

        this.inspector = CreateInspectorViewModel();
        captureTimer.Interval = 2000;
        captureTimer.Tick -= CaptureTimer_Tick;
        captureTimer.Tick += CaptureTimer_Tick;
        captureTimer.Start();
        await windowManager.ShowDialogAsync(inspector);
        captureTimer.Tick -= CaptureTimer_Tick;
        captureTimer.Stop();       
        await StopCapture();
        System.Windows.Application.Current.MainWindow.Show();
        Thread.Sleep(200);            
        logger.Information("Android (Native) scrapper is started now.");
    }

    private void CaptureTimer_Tick(object sender, EventArgs e)
    {
        try
        {
            captureTimer.Tick -= CaptureTimer_Tick;
            System.Windows.Point point = new System.Windows.Point(Cursor.Position.X, Cursor.Position.Y);
            this.inspector.HighlightControl(point);
            captureTimer.Tick += CaptureTimer_Tick;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "An error occured while trying to highlight control");
        }      
    }

    private void GlobalHookMouseDownExt(object sender, MouseEventExtArgs e)
    {
        if (Control.ModifierKeys != Keys.Control)
        {
            return;
        }
        e.Handled = true;
        Thread captureControlThread = new Thread(() =>
        {
            try
            {
                logger.Information("Processing control @ coordiante : ({0},{1}).", e.X, e.Y);
                this.highlightRectangle.BorderColor = "Green";
                capturedControls.Enqueue(CaptureControlOnClick());
                Thread.Sleep(500);
                this.highlightRectangle.BorderColor = "Orange";
            }
            catch (Exception ex)
            {
                this.highlightRectangle.BorderColor = "Red";
                logger.Error(ex, "There was an error while trying to capture control details");
            }
        });
        captureControlThread.IsBackground = true;
        captureControlThread.Start();
    }

    /// <summary>
    /// Create the view model for the inspector
    /// </summary>
    /// <returns></returns>
    protected abstract InspectorViewModel CreateInspectorViewModel();

    /// <summary>
    /// Capture details of contrl when a control is clicked
    /// </summary>
    /// <returns></returns>
    protected abstract ScrapedControl CaptureControlOnClick();

    /// </inheritdoc>
    public async Task StopCapture()
    {
        this.m_GlobalHook.MouseDownExt -= GlobalHookMouseDownExt;
        this.m_GlobalHook.Dispose();
        this.m_GlobalHook = null;       
        await eventAggregator.PublishOnUIThreadAsync(capturedControls.ToList<ScrapedControl>());
        this.inspector.Dispose();
        this.inspector = null;       
        this.highlightRectangle.Dispose();
        this.IsCapturing = false;
        logger.Information("Android (Native) Scrapper has been stopped.");
    }

    /// </inheritdoc>
    public IEnumerable<Object> GetCapturedControls()
    {
        return capturedControls.ToArray();
    }

    /// </inheritdoc>
    public async Task HandleAsync(RepositoryApplicationOpenedEventArgs message, CancellationToken cancellationToken)
    {
        targetApplicationId = message.ApplicationId;
        NotifyOfPropertyChange(() => CanToggleScrapper);
        await Task.CompletedTask;
    }
}
