using Caliburn.Micro;
using Dawn;
using Notifications.Wpf.Core;
using OpenQA.Selenium.Appium;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace Pixel.Automation.Appium.Scrapper;

/// <summary>
/// Base class for appium control inspector screen
/// </summary>
public abstract class InspectorViewModel : Screen, IDisposable
{
    protected readonly ILogger logger = Log.ForContext<InspectorViewModel>();

    private readonly double yOffSetCorrection = 0.013d;
    protected IHighlightRectangle highlightRectangle;
    protected INotificationManager notificationManager;
    protected System.Windows.Controls.Image imageControl;
    protected int mobileScreenWidth;
    protected int mobileScreenHeight;
         
    protected AppiumDriver appiumDriver;
    protected AppiumOptions appiumOptions;

    protected string remoteUrl = "http://localhost:4723";
    /// <summary>
    /// Address of the appium server to connect to
    /// </summary>
    public string RemoteUrl
    {
        get => this.remoteUrl;
        set
        {
            this.remoteUrl = value;
            NotifyOfPropertyChange();
        }
    }

    /// <summary>
    /// Capabilities for appium session
    /// </summary>
    public BindableCollection<OptionRow> DesiredCapabilities { get; protected set; } = new();

    protected ImageSource mobileScreen;
    /// <summary>
    /// Image source for the mobile screen snapshot
    /// </summary>
    public ImageSource MobileScreen
    {
        get => this.mobileScreen;
        set
        {
            this.mobileScreen = value;
            NotifyOfPropertyChange();
        }
    }

    /// <summary>
    /// Controls parsed from the current page source
    /// </summary>
    public BindableCollection<ControlNode> Controls { get; protected set; } = new();

    
    private ControlNode selectedControl;
    /// <summary>
    /// Selected control on the screen
    /// </summary>
    public ControlNode SelectedControl
    {
        get => this.selectedControl;
        set
        {
            if (this.selectedControl != value)
            {
                this.selectedControl = value;
                NotifyOfPropertyChange();
            }
        }
    }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="highlightRectangle"></param>
    /// <param name="notificationManager"></param>
    public InspectorViewModel(IHighlightRectangle highlightRectangle, INotificationManager notificationManager)
    {
        this.highlightRectangle = Guard.Argument(highlightRectangle, nameof(highlightRectangle)).NotNull().Value;
        this.notificationManager = Guard.Argument(notificationManager, nameof(notificationManager)).NotNull().Value;
    }

    /// <summary>
    /// Highlight control at a given point in windows desktop coordinates
    /// </summary>
    /// <param name="cursorPosition"></param>
    public void HighlightControl(Point cursorPosition)
    {
        if (this.Controls.Any())
        {
            this.highlightRectangle.BorderColor = "Orange";           
            var rootNode = this.Controls.First();
            Queue<ControlNode> controlQueue = new Queue<ControlNode>();
            controlQueue.Enqueue(rootNode);
            while (controlQueue.Any())
            {
                var current = controlQueue.Dequeue();
                foreach (var controlNode in current.Children)
                {
                    var boundingBox = GetControlBoundingBox(controlNode);
                    if (boundingBox.Contains(cursorPosition.X, cursorPosition.Y))
                    {
                        controlQueue.Enqueue(controlNode);
                        break;
                    }
                }
                if (!controlQueue.Any() && current != rootNode)
                {
                    SetSelectedItem(current);
                }
            }
        }       
    }

    /// <summary>
    /// Set selected item and highlight it
    /// </summary>
    /// <param name="controlNode"></param>
    public void SetSelectedItem(ControlNode controlNode)
    {
        if (this.SelectedControl != controlNode)
        {
            this.SelectedControl = controlNode;
            if (controlNode != null)
            {
                highlightRectangle.Visible = false;
                highlightRectangle.Location = GetControlBoundingBox(controlNode);
                highlightRectangle.Visible = true;
            }
        }       
    }

    /// <summary>
    /// Get the bounding box of control in windows desktop coordinates
    /// </summary>
    /// <param name="controlNode"></param>
    /// <returns></returns>
    protected BoundingBox GetControlBoundingBox(ControlNode controlNode)
    {
        double widthRatio = imageControl.ActualWidth / (double)this.mobileScreenWidth;
        double heightRatio = imageControl.ActualHeight / (double)this.mobileScreenHeight - yOffSetCorrection;
        var offSet = imageControl.PointToScreen(new Point(0d, 0d));
        if (controlNode.ScaledBoundingBox != null)
        {
            controlNode.ScaledBoundingBox.X = (int)(controlNode.ActualBoundingBox.X * widthRatio + offSet.X);
            controlNode.ScaledBoundingBox.Y = (int)(controlNode.ActualBoundingBox.Y * heightRatio + offSet.Y);
        }   
        else
        {
            controlNode.ScaledBoundingBox = new BoundingBox((int)(controlNode.ActualBoundingBox.X * widthRatio + offSet.X), (int)(controlNode.ActualBoundingBox.Y * heightRatio + offSet.Y), (int)(controlNode.ActualBoundingBox.Width * widthRatio), (int)(controlNode.ActualBoundingBox.Height * heightRatio));
        }
        return controlNode.ScaledBoundingBox;
    }

    /// <summary>
    /// Add a new capability
    /// </summary>
    public void AddCapability()
    {
        this.DesiredCapabilities.Add(new OptionRow() { Type = "text" });
    }

    /// <summary>
    /// Remove an existing capability
    /// </summary>
    /// <param name="optionRow"></param>
    public void RemoveCapability(OptionRow optionRow)
    {
        this.DesiredCapabilities.Remove(optionRow);
    }

    protected bool isConnected;
    /// <summary>
    /// Indicates if the appium session is  connected
    /// </summary>
    public bool IsConnected
    {
        get => this.isConnected;
        set
        {
            this.isConnected = value;
            NotifyOfPropertyChange();
        }
    }

    /// <summary>
    /// Indicates if it is possible to start an appium session
    /// </summary>
    public bool CanConnect
    {
        get => !this.isConnected;
    }

    /// <summary>
    /// Start a new appium session
    /// </summary>
    /// <returns></returns>
    public async Task Connect()
    {
        try
        {
            this.appiumDriver = CreateDriver();         
            await RefreshScreen();
            this.IsConnected = true;
            NotifyOfPropertyChange(nameof(CanConnect));
            NotifyOfPropertyChange(nameof(CanRefreshScreen));
            NotifyOfPropertyChange(nameof(IsConnected));
        }
        catch (Exception ex)
        {
            logger.Error(ex, "There was an error while trying to start appium session");
            await this.notificationManager.ShowAsync(new NotificationContent()
            {
                Title = "Error",
                Message = ex.Message,
                Type = NotificationType.Error
            });
        }
    }

    /// <summary>
    /// Create the AppiumDriver to start a session
    /// </summary>
    /// <returns></returns>
    protected abstract AppiumDriver CreateDriver();

    /// <summary>
    /// Create MobileControl from xml description of control
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="reader"></param>
    /// <returns></returns>
    protected abstract MobileControl CreateMobileControl(XmlReader reader);

    /// <summary>
    /// Indicates if screen can be refreshed
    /// </summary>
    public bool CanRefreshScreen
    {
        get => this.appiumDriver != null && this.isConnected;
    }

    /// <summary>
    /// Refresh screen
    /// </summary>
    /// <returns></returns>
    public async Task RefreshScreen()
    {
        try
        {
            CaptureScreen();
            var root = ProcessControls();
            this.Controls.Clear();
            if (root != null)
            {
                this.Controls.Add(root);
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "There was an error while trying to refresh screen");
            await this.notificationManager.ShowAsync(new NotificationContent()
            {
                Title = "Error",
                Message = ex.Message,
                Type = NotificationType.Error
            });
        }
    }

    /// <summary>
    /// Take a screen shot of the mobile display to show on inspector screen
    /// </summary>
    protected virtual void CaptureScreen()
    {
        var screenSnapshot = this.appiumDriver.GetScreenshot();
        var ms = new MemoryStream(screenSnapshot.AsByteArray);
        var source = new BitmapImage();
        source.BeginInit();
        source.CacheOption = BitmapCacheOption.None;
        source.StreamSource = ms;
        source.EndInit();
        this.MobileScreen = source;      
    }

    /// <summary>
    /// Process the page source from appium session in to tree of ControlNode
    /// </summary>
    /// <returns></returns>
    protected virtual ControlNode ProcessControls()
    {
        var pageSource = this.appiumDriver.PageSource;
        if (!string.IsNullOrEmpty(pageSource))
        {
            Stack<ControlNode> controlNodes = new Stack<ControlNode>();
            ControlNode currentRoot = default;
            using (var reader = XmlReader.Create(new StringReader(pageSource)))
            {
                while (reader.Read())
                {
                    if (reader.Name.Equals("xml"))
                    {
                        continue;
                    }

                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            MobileControl node = CreateMobileControl(reader);
                            if (currentRoot != null && reader.IsEmptyElement)
                            {
                                var controlNode = new ControlNode(node);
                                currentRoot.Children.Add(controlNode);                              
                            }
                            else
                            {
                                var controlNode = new ControlNode(node);
                                if(currentRoot == null)
                                {
                                    controlNodes.Push(controlNode);
                                    currentRoot = controlNode;
                                    this.mobileScreenWidth = int.Parse(reader.GetAttribute("width"));
                                    this.mobileScreenHeight = int.Parse(reader.GetAttribute("height"));
                                    logger.Information("Width Ratio is : {0} ", imageControl.ActualWidth / (double)this.mobileScreenWidth);
                                    logger.Information("Height Ratio is : {0}", imageControl.ActualHeight / (double)this.mobileScreenHeight);
                                    logger.Information("Offset is : {0}", imageControl.PointToScreen(new Point(0d, 0d)));
                                }
                                else
                                {
                                    currentRoot.Children.Add(controlNode);
                                    controlNodes.Push(controlNode);
                                    currentRoot = controlNode;
                                }                              
                            }
                            break;
                        case XmlNodeType.EndElement:                           
                            controlNodes.Pop();
                            if(controlNodes.Count > 0)
                            {
                                currentRoot = controlNodes.Peek();
                            }
                            break;
                    }
                }
            }
            return currentRoot;
        }
        return default;
    }
 
    /// </inheritdoc>   
    protected override void OnViewLoaded(object view)
    {
        try
        {
            if (view is FrameworkElement fe)
            {
                this.imageControl = fe.FindName("MobileScreen") as System.Windows.Controls.Image;               
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, ex.Message);
        }
        base.OnViewLoaded(view);
    }

    /// </inheritdoc>   
    public void Dispose()
    {
        if (this.appiumDriver != null)
        {
            this.appiumDriver.Quit();
            this.appiumDriver.Dispose();
        }
        this.IsConnected = false;
    }
}
