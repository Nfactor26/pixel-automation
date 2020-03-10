using Caliburn.Micro;
using Gma.System.MouseKeyHook;
using Pixel.Automation.Core.Args;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.UIA.Components;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;

namespace Pixel.Automation.UIA.Scrapper
{
    public class UIAControlScrapperComponent : PropertyChangedBase, IControlScrapper, IHandle<RepositoryApplicationOpenedEventArgs>
    {
        private readonly IEventAggregator eventAggregator;

        private IKeyboardMouseEvents m_GlobalHook;

        private readonly IScreenCapture screenCapture;

        public string DisplayName { get; } = "UIA Scrapper";

        string targetApplicationId = string.Empty;

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }


        bool isCapturing;
        public bool IsCapturing
        {
            get => isCapturing;
            set
            {
                isCapturing = value;
                if (isCapturing)
                {
                    StartCapture();
                }
                else
                {
                    StopCapture();
                }
            }
        }

        public UIAControlScrapperComponent(IEventAggregator eventAggregator,IHighlightRectangleFactory highlightRectangleFactory, IScreenCapture screenCapture)
        {
            this.eventAggregator = eventAggregator;
            this.eventAggregator.SubscribeOnUIThread(this);
            this.highlightRectangleFactory = highlightRectangleFactory;
            this.screenCapture = screenCapture;
        }

        public bool CanToggleScrapper
        {
            get
            {
                return !(string.IsNullOrEmpty(this.targetApplicationId));
            }

        }

        private readonly IHighlightRectangleFactory highlightRectangleFactory;

        IHighlightRectangle controlHighlight;

        IHighlightRectangle containerHighlight;

        System.Windows.Rect focusedRect = new System.Windows.Rect();

        AutomationElement containerNode = default;

        ConcurrentQueue<ScrapedControl> capturedControls;

        System.Windows.Forms.Timer captureTimer = new System.Windows.Forms.Timer();

        int[] lastCapturedControlRunTimeId = new int[2] { 0, 0 };
        int[] lastHoveredControlRunTimeId = new int[2] { 0, 0 };

        public void StartCapture()
        {
            capturedControls = new ConcurrentQueue<ScrapedControl>();

            controlHighlight = highlightRectangleFactory.CreateHighlightRectangle();
            containerHighlight = highlightRectangleFactory.CreateHighlightRectangle();

            captureTimer.Interval = 2000;
            captureTimer.Tick -= CaptureTimer_Tick;
            captureTimer.Tick += CaptureTimer_Tick;
            captureTimer.Start();

            if (m_GlobalHook == null)
                m_GlobalHook = Hook.GlobalEvents();
            m_GlobalHook.MouseDownExt += GlobalHookMouseDownExt;


            Log.Information("Win Scrapper has been activated.");

        }

        private void CaptureTimer_Tick(object sender, EventArgs e)
        {
            captureTimer.Tick -= CaptureTimer_Tick;
            Thread captureControlThread = new Thread(() =>
            {
                try
                {
                    System.Windows.Point point = new System.Windows.Point(Cursor.Position.X, Cursor.Position.Y);
                    AutomationElement trackedElement = AutomationElement.FromPoint(point);

                    if (trackedElement == null)
                        return;

                    //TODO : On windows 7, Runtime id can be empty. Find another way to check whether element is same. Maybe bounding box?
                    if (lastHoveredControlRunTimeId.SequenceEqual(trackedElement.GetRuntimeId()))
                    {
                        return;
                    }
                    lastHoveredControlRunTimeId = trackedElement.GetRuntimeId();

                    focusedRect = trackedElement.Current.BoundingRectangle;

                    ShowControlHighlightRectangle(focusedRect, Color.Orange);

                }

                catch (Exception ex)
                {
                    controlHighlight.BorderColor = Color.Red;
                    Log.Error(ex, ex.Message);
                }
                finally
                {
                    captureTimer.Tick += CaptureTimer_Tick;
                }
            });
            captureControlThread.IsBackground = true;
            captureControlThread.Start();
        }
      
        private void GlobalHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            if (Control.ModifierKeys != Keys.Control)
            {
                return;
            }
            e.Handled = true;

            controlHighlight.Visible = false;

            Thread captureControlThread = new Thread(() =>
            {
                try
                {
                    captureTimer.Tick -= CaptureTimer_Tick;

                    //Check if this control is same as last control that was scraped to avoid duplicates
                    System.Windows.Point point = new System.Windows.Point(e.X, e.Y);
                    AutomationElement trackedElement = AutomationElement.FromPoint(point);
                    if (trackedElement == null)
                        return;

                    if (lastCapturedControlRunTimeId.SequenceEqual(trackedElement.GetRuntimeId()))
                    {
                        return;
                    }
                    lastCapturedControlRunTimeId = trackedElement.GetRuntimeId();

                    //Take a snapshot of control
                    focusedRect = trackedElement.Current.BoundingRectangle;                
                    Bitmap controlScreenShot = screenCapture.CaptureArea(new Rectangle((int)focusedRect.X, (int)focusedRect.Y, (int)focusedRect.Width, (int)focusedRect.Height));

                    ShowControlHighlightRectangle(focusedRect, Color.Orange);

                    Log.Information("Processing AutomationElement @ coordiante : {@Point}.", point);

                    controlHighlight.BorderColor = Color.Yellow;

                    WinControlIdentity capturedControl = null;
                    BuildNodeHierarchy(trackedElement, ref capturedControl);
                    WinControlIdentity current = capturedControl;
                    switch (e.Button)
                    {
                        case MouseButtons.Left:
                            //everything taken care of already
                            break;

                        case MouseButtons.Right:
                            containerNode = trackedElement;
                            current.ControlType = Core.Enums.ControlType.Default;
                            controlHighlight.Visible = false;
                            containerHighlight.Visible = true;
                            containerHighlight.BorderColor = Color.Purple;
                            containerHighlight.Location = new Rectangle((int)focusedRect.Left, (int)focusedRect.Top,
                            (int)focusedRect.Width, (int)focusedRect.Height);

                            break;
                    }
                   
                    int currentDepth = 0;
                    while (current.Next != null)
                    {
                        current.Name = $"{currentDepth}";
                        currentDepth++;
                        current = current.Next as WinControlIdentity;                       
                    }
                    current.Name = $"{currentDepth}";
                 
                    ScrapedControl scrapedControl = new ScrapedControl() { ControlData = capturedControl, ControlImage = controlScreenShot };
                    capturedControls.Enqueue(scrapedControl);

                    controlHighlight.BorderColor = Color.Green;

                    Log.Information("Captured control : {$capturedControl} as {$controlType}", current, current.ControlType);

                }
                catch (Exception ex)
                {
                    controlHighlight.BorderColor = Color.Red;
                    Log.Error(ex, ex.Message);
                }
                finally
                {
                    Thread.Sleep(500);
                    captureTimer.Tick += CaptureTimer_Tick;
                }
            });
            captureControlThread.IsBackground = true;
            captureControlThread.Start();

        }

        private void BuildNodeHierarchy(AutomationElement currentNode, ref WinControlIdentity controlComponent)
        {
            if (currentNode == AutomationElement.RootElement)
            {
                //since we reached the root and we did not encounter containerNode, it would mean user is trying to scrap a control that is not a descendant 
                //of containerNode. Hence, clear it out.
                if (containerNode != null)
                {
                    containerHighlight.Visible = false;
                }
                controlComponent.ControlType = Core.Enums.ControlType.Default;
                controlComponent.SearchScope = Core.Enums.SearchScope.Children;
                return;
            }

            //if containerNode is not null and currentNode is same as containerNode , no need to look up any further in ancestors
            if (containerNode != null && currentNode.GetRuntimeId().SequenceEqual(containerNode.GetRuntimeId()))
            {
                controlComponent.ControlType = Core.Enums.ControlType.Relative;
                controlComponent.SearchScope = Core.Enums.SearchScope.Children;
                return;
            }

            lastCapturedControlRunTimeId = currentNode.GetRuntimeId();
            AutomationElement parent = TreeWalker.RawViewWalker.GetParent(currentNode);

            //checking for controlComponent!=null  ensures that target control is always captured..
            if (controlComponent != null)
            {
                //TODO : Revisit this when you have find scenario when some control has a different process id then one of its descendant control

                //if the parent control process id changes at any point of time from its child, set IsContainedInChildWindow property to true for all child.
                //At runtime, if IsContainedInChildWindow property is true, process id check is skipped while looking for control
                //if (parent.Current.ProcessId != currentNode.Current.ProcessId)
                //{
                //    var currentRoot = controlComponent;
                //    while (currentRoot.Next != null)
                //    {
                //        //currentRoot.IsContainedInChildWindow = true;
                //        currentRoot = currentRoot.Next as WinControlIdentity;
                //    }
                //}

                //Whether to capture control or not up in hierachy will be decided by ShouldCaptureNode
                //Also, if the last captured control has no name and automationid , capture the parent as well to calculate it's index in parent
                if (!ShouldCaptureNode(currentNode, parent) && (!string.IsNullOrEmpty(controlComponent.NameProperty) && !string.IsNullOrEmpty(controlComponent.AutomationId)))
                {
                    controlComponent.SearchScope = Core.Enums.SearchScope.Descendants;
                    BuildNodeHierarchy(parent, ref controlComponent);
                    return;
                }
                else
                {
                    controlComponent.SearchScope = Core.Enums.SearchScope.Children;
                    TryCalculateRelativeIndex(controlComponent, parent);
                }

            }

            //process current node details
            WinControlIdentity currentNodeDetails = CreateControlComponent(currentNode);
            if (controlComponent == null)
            {
                controlComponent = currentNodeDetails;
            }
            else
            {
                currentNodeDetails.Next = controlComponent;
                controlComponent = currentNodeDetails;
            }



            //On windows 7 , parent is obtained using TreeWalker , however, on trying to lookup control inside parent it is not located. Skip current in that case and try 
            //to lookup inside parent instead .

            //if (currentNodeDetails.SearchScope == Core.Enums.SearchScope.Children)
            //{
            //    if (TryCalculateRelativeIndex(currentNodeDetails, parent))
            //    {
            //        controlComponent = currentNodeDetails;
            //        BuildNodeHierarchy(parent, ref controlComponent);
            //        return;
            //    }
            //    Log.Warning("AutomationElement node was not located in its parent.Parent Element will be skipped from hierarchy.");
            //}


            //while (true)
            //{
            //    lastCapturedControlRunTimeId = parent.GetRuntimeId();
            //    if (TryCalculateRelativeIndex(controlComponent, parent))
            //    {
            //        break;
            //    }
            //    parent = TreeWalker.RawViewWalker.GetParent(parent);
            //    if (parent == AutomationElement.RootElement)
            //    {
            //        throw new NullReferenceException("AutomationElement could not be located in any of it's parent. Reached AutomationElement.RootElement during lookup");
            //    }

            //}
            BuildNodeHierarchy(parent, ref controlComponent);
        }

        private bool ShouldCaptureNode(AutomationElement currentNode, AutomationElement parentNode)
        {
            //Current node is application window.We should capture this
            if (parentNode == AutomationElement.RootElement)
                return true;

            var siblingsOfCurrentNode = parentNode.FindAll(TreeScope.Children, Condition.TrueCondition);  //this includes current node as well 

            //if currentNode is single child , no need to capture this. During search we have exactly one path to follow
            if (siblingsOfCurrentNode.Count == 1)
                return false;

            return true;
        }

        /// <summary>
        /// Create a WinControlIdentityComponent from AutomationElement
        /// </summary>
        /// <param name="trackedElement"></param>
        /// <returns></returns>
        private WinControlIdentity CreateControlComponent(AutomationElement trackedElement)
        {
            WinControlIdentity capturedControl = new WinControlIdentity();
            capturedControl.ProcessId = trackedElement.Current.ProcessId;
            string executable = Process.GetProcessById(capturedControl.ProcessId).MainModule.FileName;
            FileInfo executableInfo = new FileInfo(executable);
            capturedControl.OwnerApplication = executableInfo.Name.Split(new char[] { '.' })[0];
            capturedControl.NameProperty = trackedElement.Current.Name;
            capturedControl.AutomationId = trackedElement.Current.AutomationId;
            capturedControl.ClassName = trackedElement.Current.ClassName;
            capturedControl.ControlTypeId = trackedElement.Current.ControlType.Id;
            var boundingRectangle = trackedElement.Current.BoundingRectangle;
            capturedControl.BoundingBox = new Rectangle((int)boundingRectangle.X, (int)boundingRectangle.Y,
                            (int)boundingRectangle.Width, (int)boundingRectangle.Height);
            capturedControl.AcceleratorKey = trackedElement.Current.AcceleratorKey;
            capturedControl.AccessKey = trackedElement.Current.AccessKey;
            //HelpText changes if mouseover is true . While capturing control, mouse is always over so it will yield wrong results as during playback mouse will not be initially hovering
            //the AutomationElement.If required , this value can be set manually during configuration phase.
            capturedControl.HelpText = string.Empty;  // trackedElement.Current.HelpText;   
            capturedControl.IsContentElement = trackedElement.Current.IsContentElement;
            capturedControl.IsControlElement = trackedElement.Current.IsControlElement;

            var supportedPatterns = trackedElement.GetSupportedPatterns();
            foreach (var pattern in supportedPatterns)
            {
                capturedControl.SupportedPatterns.Add(pattern.ProgrammaticName);
            }

            return capturedControl;
        }

        /// <summary>
        /// Calculate the index of WinControlIdentity within parent AutomationElement
        /// </summary>
        /// <param name="winControl"></param>
        /// <param name="parent"></param>
        private bool TryCalculateRelativeIndex(WinControlIdentity winControl, AutomationElement parent)
        {
            //Note : On windows 7 , properties like name,automationid and classname can be null. Trying to build a search condition with a null value will result in exception           
            //Note : For Internet explorer, if name is null or empty , no match is found

            Condition searchCondition = ConditionFactory.FromControlType(ControlType.LookupById(winControl.ControlTypeId));
            searchCondition = searchCondition.AndPropertyCondition(new PropertyCondition(AutomationElement.IsContentElementProperty, winControl.IsContentElement));
            searchCondition = searchCondition.AndPropertyCondition(new PropertyCondition(AutomationElement.IsControlElementProperty, winControl.IsControlElement));


            if (!string.IsNullOrEmpty(winControl.NameProperty))
                searchCondition = searchCondition.AndName(winControl.NameProperty);

            if (winControl.AutomationId != null)
                searchCondition = searchCondition.AndAutomationId(winControl.AutomationId);

            if (winControl.ClassName != null)
                searchCondition = searchCondition.AndClassName(winControl.ClassName);

            if (!string.IsNullOrEmpty(winControl.AccessKey))
            {
                searchCondition = searchCondition.AndAccessKey(winControl.AccessKey);
            }
            if (!string.IsNullOrEmpty(winControl.HelpText))
            {
                searchCondition = searchCondition.AndHelpText(winControl.HelpText);
            }
            if (!string.IsNullOrEmpty(winControl.AcceleratorKey))
            {
                searchCondition = searchCondition.AndAccessKey(winControl.AcceleratorKey);
            }

            var foundElements = parent.FindAll(TreeScope.Children, searchCondition);
            bool found = false;
            int index = 0;
            foreach (AutomationElement elem in foundElements)
            {
                index++;
                if (lastCapturedControlRunTimeId.SequenceEqual(elem.GetRuntimeId()))
                {
                    winControl.Index = index;                 
                    found = true;
                    break;
                }
            }          
            return found;
        }

        private void ShowControlHighlightRectangle(System.Windows.Rect boundingBox, Color borderColor)
        {
            // Hide old rectangle.
            controlHighlight.Visible = false;

            // Show new rectangle.
            controlHighlight.Location = new System.Drawing.Rectangle((int)boundingBox.Left, (int)boundingBox.Top,
            (int)boundingBox.Width, (int)boundingBox.Height);

            controlHighlight.BorderColor = borderColor;
            controlHighlight.Visible = true;

        }

        public void StopCapture()
        {
            m_GlobalHook.MouseDownExt -= GlobalHookMouseDownExt;
            m_GlobalHook.Dispose();
            m_GlobalHook = null;
            captureTimer.Stop();
            eventAggregator.PublishOnUIThreadAsync(capturedControls.ToList<ScrapedControl>());
            controlHighlight.Dispose();
            containerHighlight.Dispose();
            capturedControls = null;

            Log.Information("Win Scrapper has been stopped.");

        }

        public IEnumerable<Object> GetCapturedControls()
        {
            return capturedControls.ToArray();
        }       

        public async Task HandleAsync(RepositoryApplicationOpenedEventArgs message, CancellationToken cancellationToken)
        {
            targetApplicationId = message.ApplicationId;
            NotifyOfPropertyChange(() => CanToggleScrapper);
            await Task.CompletedTask;
        }
    }
}
