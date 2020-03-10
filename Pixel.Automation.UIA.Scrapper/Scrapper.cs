//using Caliburn.Micro;
//using Pixel.Automation.Core.Args;
//using Pixel.Automation.Core.Arguments;
//using Pixel.Automation.Core.Interfaces;
//using Pixel.Automation.Core.Models;
//using Pixel.Automation.UIA.Components;
//using Pixel.Automation.Utilities;
//using Serilog;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Drawing;
//using System.IO;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading;
//using System.Windows.Automation;
//using System.Windows.Forms;

//namespace Pixel.Automation.UIA.Scrapper
//{
//    public class ControlScrapperComponent : PropertyChangedBase, IControlScrapper, IHandle<RepositoryApplicationOpenedEventArgs>
//    {
//        private readonly IEventAggregator eventAggregator;

//        public string DisplayName { get; } = "Win Scrapper";

//        [DllImport("user32.dll")]
//        static extern IntPtr GetForegroundWindow();

//        [DllImport("user32.dll")]
//        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

//        private string GetActiveWindowTitle()
//        {
//            const int nChars = 256;
//            StringBuilder Buff = new StringBuilder(nChars);
//            IntPtr handle = GetForegroundWindow();

//            if (GetWindowText(handle, Buff, nChars) > 0)
//            {
//                return Buff.ToString();
//            }
//            return null;
//        }


//        bool isCapturing;
//        public bool IsCapturing
//        {
//            get => isCapturing;
//            set
//            {
//                isCapturing = value;
//                if (isCapturing)
//                {
//                    StartCapture();
//                }
//                else
//                {
//                    StopCapture();
//                }

//            }
//        }

//        string targetApplicationId = string.Empty;

//        public bool CanToggleScrapper
//        {
//            get
//            {
//                return !(string.IsNullOrEmpty(this.targetApplicationId));
//            }

//        }

//        public ControlScrapperComponent(IEventAggregator eventAggregator)
//        {
//            this.eventAggregator = eventAggregator;
//            this.eventAggregator.Subscribe(this);
//        }
       

//        HighlightRectangle highlight = new HighlightRectangle();
//        System.Windows.Rect focusedRect = new System.Windows.Rect();


//        ConcurrentQueue<ScrapedControl> capturedControls;


//        System.Windows.Forms.Timer captureTimer = new System.Windows.Forms.Timer();


//        int[] lastCapturedControlRunTimeId = new int[2] { 0, 0 };
//        int[] lastHoveredControlRunTimeId = new int[2] { 0, 0 };
//        public void StartCapture()
//        {
//            isCapturing = true;
//            capturedControls = new ConcurrentQueue<ScrapedControl>();
//            captureTimer.Interval = 2000;
//            captureTimer.Tick -= CaptureTimer_Tick;
//            captureTimer.Tick += CaptureTimer_Tick;
//            captureTimer.Start();

//            Log.Information("Win Scrapper has been activated.");


//        }



//        private void CaptureTimer_Tick(object sender, EventArgs e)
//        {
//            Thread captureControlThread = new Thread(() =>
//            {
//                try
//                {
//                    System.Windows.Point point = new System.Windows.Point(Cursor.Position.X, Cursor.Position.Y);
//                    AutomationElement trackedElement = AutomationElement.FromPoint(point);
//                    if (trackedElement == null)
//                        return;

//                    if (Control.ModifierKeys != Keys.Control)
//                    {
//                        if (lastHoveredControlRunTimeId.SequenceEqual(trackedElement.GetRuntimeId()))
//                        {
//                            return;
//                        }
//                        lastHoveredControlRunTimeId = trackedElement.GetRuntimeId();
//                    }
//                    focusedRect = trackedElement.Current.BoundingRectangle;
//                    // Hide old rectangle.
//                    highlight.Visible = false;

//                    // Show new rectangle.
//                    highlight.Location = new System.Drawing.Rectangle((int)focusedRect.Left, (int)focusedRect.Top,
//                        (int)focusedRect.Width, (int)focusedRect.Height);

//                    highlight.BorderColor = System.Drawing.Color.Orange;
//                    highlight.Visible = true;

//                    if (Control.ModifierKeys == Keys.Control)
//                    {
//                        Log.Information("Processing AutomationElement @ coordiante : {@Point}.", point);

//                        //captureTimer.Stop();
//                        highlight.BorderColor = System.Drawing.Color.Yellow;

//                        WinControlIdentity capturedControl = null;
//                        BuildComponentHierarchy(trackedElement, ref capturedControl);

//                        //Assign names in the form index_controltype
//                        WinControlIdentity current = capturedControl;
//                        int currentDepth = 0;
//                        while (current.Next != null)
//                        {
//                            current.Name = $"{currentDepth}";
//                            currentDepth++;
//                            current = current.Next as WinControlIdentity;
//                        }
//                        current.Name = $"{currentDepth}";
//                        //capturedControl.Name = Guid.NewGuid().ToString();

//                        ScreenCapture screenCapture = new ScreenCapture();
//                        Bitmap controlScreenShot = screenCapture.CaptureArea(new Rectangle((int)focusedRect.X, (int)focusedRect.Y, (int)focusedRect.Width, (int)focusedRect.Height));

//                        ScrapedControl scrapedControl = new ScrapedControl() { ControlData = capturedControl, ControlImage = controlScreenShot };

//                        capturedControls.Enqueue(scrapedControl);

//                        highlight.BorderColor = System.Drawing.Color.Green;
//                        //captureTimer.Start();                  

//                        //Log.Information("Captured details for  WinControlIdentityComponent : {@CapturedControl}", capturedControl);
//                    }
//                }
//                catch (Exception ex)
//                {
//                    highlight.BorderColor = System.Drawing.Color.Red;
//                    Log.Error(ex, ex.Message);
//                    //captureTimer.Start();
//                }
//            });
//            captureControlThread.IsBackground = true;
//            captureControlThread.Start();
//        }

//        private void BuildComponentHierarchy(AutomationElement currentNode, ref WinControlIdentity controlComponent)
//        {
//            if (currentNode == AutomationElement.RootElement)
//                return;

//            WinControlIdentity currentNodeDetails = CreateControlComponent(currentNode);
//            if (controlComponent != null)
//            {
//                currentNodeDetails.Next = controlComponent;
              
//            }
//            controlComponent = currentNodeDetails;
//            AutomationElement parent = TreeWalker.RawViewWalker.GetParent(currentNode);
//            CalculateRelativeIndex(controlComponent, parent);
//            BuildComponentHierarchy(parent, ref controlComponent);

//        }

//        /// <summary>
//        /// Create a WinControlIdentityComponent from AutomationElement
//        /// </summary>
//        /// <param name="trackedElement"></param>
//        /// <returns></returns>
//        private WinControlIdentity CreateControlComponent(AutomationElement trackedElement)
//        {
//            WinControlIdentity capturedControl = new WinControlIdentity();
//            capturedControl.ProcessId = trackedElement.Current.ProcessId;
//            string executable = Process.GetProcessById(capturedControl.ProcessId).MainModule.FileName;
//            FileInfo executableInfo = new FileInfo(executable);
//            capturedControl.OwnerApplication = executableInfo.Name.Split(new char[] { '.' })[0];
//            capturedControl.NameProperty = trackedElement.Current.Name;
//            capturedControl.AutomationId = trackedElement.Current.AutomationId;
//            capturedControl.ClassName = trackedElement.Current.ClassName;
//            capturedControl.ControlTypeId = trackedElement.Current.ControlType.Id;
//            capturedControl.BoundingRectangle = trackedElement.Current.BoundingRectangle;
//            capturedControl.AcceleratorKey = trackedElement.Current.AcceleratorKey;
//            capturedControl.AccessKey = trackedElement.Current.AccessKey;
//            //HelpText changes if mouseover is true . While capturing control, mouse is always over so it will yield wrong results as during playback mouse will not be initially hovering
//            //the AutomationElement.If required , this value can be set manually during configuration phase.
//            capturedControl.HelpText = string.Empty;  // trackedElement.Current.HelpText;   
//            capturedControl.IsContentElement = trackedElement.Current.IsContentElement;
//            capturedControl.IsControlElement = trackedElement.Current.IsControlElement;

//            var supportedPatterns = trackedElement.GetSupportedPatterns();
//            foreach (var pattern in supportedPatterns)
//            {
//                capturedControl.SupportedPatterns.Add(pattern.ProgrammaticName);
//            }

//            return capturedControl;
//        }

//        /// <summary>
//        /// Calculate the index of WinControlIdentity within parent AutomationElement
//        /// </summary>
//        /// <param name="winControl"></param>
//        /// <param name="parent"></param>
//        private void CalculateRelativeIndex(WinControlIdentity winControl, AutomationElement parent)
//        {

//            System.Windows.Automation.Condition searchCondition = ConditionFactory.FromControlType(ControlType.LookupById(winControl.ControlTypeId));
//            searchCondition = searchCondition.AndPropertyCondition(new PropertyCondition(AutomationElement.IsContentElementProperty, winControl.IsContentElement));
//            searchCondition = searchCondition.AndPropertyCondition(new PropertyCondition(AutomationElement.IsControlElementProperty, winControl.IsControlElement));

//            if (!string.IsNullOrEmpty(winControl.NameProperty))
//            {
//                searchCondition = searchCondition.AndName(winControl.NameProperty);
//            }
//            if (!string.IsNullOrEmpty(winControl.AutomationId))
//            {
//                searchCondition = searchCondition.AndAutomationId(winControl.AutomationId);
//            }
//            if (!string.IsNullOrEmpty(winControl.ClassName))
//            {
//                searchCondition = searchCondition.AndClassName(winControl.ClassName);
//            }
//            if (!string.IsNullOrEmpty(winControl.AccessKey))
//            {
//                searchCondition = searchCondition.AndAccessKey(winControl.AccessKey);
//            }
//            if (!string.IsNullOrEmpty(winControl.HelpText))
//            {
//                searchCondition = searchCondition.AndHelpText(winControl.HelpText);
//            }
//            if (!string.IsNullOrEmpty(winControl.AcceleratorKey))
//            {
//                searchCondition = searchCondition.AndAccessKey(winControl.AcceleratorKey);
//            }

//            var foundElements = parent.FindAll(TreeScope.Subtree, searchCondition);
//            bool found = false;
//            int index = 0;
//            foreach (AutomationElement elem in foundElements)
//            {
//                index++;
//                if (lastCapturedControlRunTimeId.SequenceEqual(elem.GetRuntimeId()))
//                {
//                    winControl.Index = new InArgument<int>() { CanChangeType = false, CanChangeMode = true, DefaultValue = index, Mode = ArgumentMode.Default };
//                    found = true;
//                    break;
//                }
//            }
//            if (!found)
//            {
//                winControl.Index = new InArgument<int>() { CanChangeType = false, CanChangeMode = true, DefaultValue = 1, Mode = ArgumentMode.Default };
//            }
//        }

//        public void StopCapture()
//        {
//            captureTimer.Stop();
//            captureTimer.Dispose();      
//            highlight.Visible = false;
//            eventAggregator.PublishOnUIThread(capturedControls.ToList<ScrapedControl>());
//            capturedControls = null;
//            isCapturing = false;
//            Log.Information("Win Scrapper has been stopped.");

//        }

//        public IEnumerable<Object> GetCapturedControls()
//        {
//            return capturedControls?.ToArray();
//        }

//        public void Handle(RepositoryApplicationOpenedEventArgs message)
//        {
//            targetApplicationId = message.ApplicationId;           
//            NotifyOfPropertyChange(() => CanToggleScrapper);
//        }
//    }
//}
