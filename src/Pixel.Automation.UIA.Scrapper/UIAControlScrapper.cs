extern alias uiaComWrapper;
using Caliburn.Micro;
using Gma.System.MouseKeyHook;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.UIA.Components;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using uiaComWrapper::System.Windows.Automation;

namespace Pixel.Automation.UIA.Scrapper
{
    public class UIAControlScrapperComponent : PropertyChangedBase, IControlScrapper, IHandle<RepositoryApplicationOpenedEventArgs>
    {
        private readonly ILogger logger = Log.ForContext<UIAControlLocatorComponent>();
        private readonly IEventAggregator eventAggregator;
        private IKeyboardMouseEvents m_GlobalHook;
        private readonly IScreenCapture screenCapture;

        public string DisplayName { get; } = "UIA Scrapper";

        string targetApplicationId = string.Empty;

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        bool isCapturing;
        public bool IsCapturing
        {
            get => isCapturing;
            set
            {
                isCapturing = value;
                NotifyOfPropertyChange(() => IsCapturing);
            }
        }

        public UIAControlScrapperComponent(IEventAggregator eventAggregator, IHighlightRectangleFactory highlightRectangleFactory, IScreenCapture screenCapture)
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

        public async Task StartCapture()
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
            await Task.CompletedTask;

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

                    ShowControlHighlightRectangle(focusedRect, "Orange");

                }

                catch (Exception ex)
                {
                    controlHighlight.BorderColor = "Red";
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
                  
                    var controlScreenShot = screenCapture.CaptureArea(new BoundingBox((int)focusedRect.X, (int)focusedRect.Y, (int)focusedRect.Width, (int)focusedRect.Height));
                    ShowControlHighlightRectangle(focusedRect, "Orange");

                    Log.Information("Processing AutomationElement @ coordiante : {@Point}.", point);

                    List<AutomationElement> controlPath = new List<AutomationElement>();
                    AutomationElement current = trackedElement;
                    while (current != AutomationElement.RootElement && !(containerNode?.GetRuntimeId().SequenceEqual(current.GetRuntimeId()) ?? false))
                    {
                        LogElementDetails("Parent", current);
                        controlPath.Add(current);
                        current = TreeWalker.RawViewWalker.GetParent(current);
                    }
                    controlPath.Reverse();
                    logger.Information($"Identified {controlPath.Count} elements in control path");

                    //user clicked element which is not relative to container node. Clear the container node and remove container highlight
                    if (containerNode != null && current == AutomationElement.RootElement)
                    {
                        containerNode = null;
                        containerHighlight.Visible = false;
                        logger.Information("Removing container node since user clicked an element which is not relative to container node");
                    }


                    current = controlPath.ElementAt(0);
                    WinControlIdentity rootNodeIdentity = CreateControlComponent(current, 1);
                    rootNodeIdentity.SearchScope = Core.Enums.SearchScope.Children;
                    WinControlIdentity currentNodeIdentity = rootNodeIdentity;
                    AutomationElement lastCapturedAncestorNode = current;
                    int pathLength = controlPath.Count;

                    for (int i = 1; i < pathLength - 1; i++)
                    {
                        var currentNode = controlPath.ElementAt(i) as AutomationElement;
                        var nextNode = controlPath.ElementAt(i + 1) as AutomationElement;

                        if (ShouldCaptureNode(currentNode, nextNode))
                        {
                            var nextNodeIdentity = CreateControlComponent(controlPath.ElementAt(i), i + 1);
                            nextNodeIdentity.Index = GetIndexInParent(currentNode, lastCapturedAncestorNode);
                            lastCapturedAncestorNode = currentNode;
                            if (nextNodeIdentity.Depth - currentNodeIdentity.Depth > 1)
                            {
                                nextNodeIdentity.SearchScope = Core.Enums.SearchScope.Descendants;
                            }
                            else
                            {
                                nextNodeIdentity.SearchScope = Core.Enums.SearchScope.Children;
                            }
                            currentNodeIdentity.Next = nextNodeIdentity;
                            currentNodeIdentity = nextNodeIdentity;
                        }

                    }

                    if (pathLength > 1)
                    {
                        var leafNodeIdentity = CreateControlComponent(trackedElement, pathLength);
                        leafNodeIdentity.Index = GetIndexInParent(trackedElement, lastCapturedAncestorNode);
                        if (leafNodeIdentity.Depth - currentNodeIdentity.Depth > 1)
                        {
                            leafNodeIdentity.SearchScope = Core.Enums.SearchScope.Descendants;
                        }
                        else
                        {
                            leafNodeIdentity.SearchScope = Core.Enums.SearchScope.Children;
                        }
                        currentNodeIdentity.Next = leafNodeIdentity;
                        currentNodeIdentity = leafNodeIdentity;
                    }

                    switch (e.Button)
                    {
                        case MouseButtons.Left:                          
                            break;

                        case MouseButtons.Right:
                            containerNode = trackedElement;                         
                            ShowContainerHighlightRectangle(containerNode.Current.BoundingRectangle, "Purple");
                            break;
                    }


                    ScrapedControl scrapedControl = new ScrapedControl() { ControlData = rootNodeIdentity, ControlImage = controlScreenShot };
                    capturedControls.Enqueue(scrapedControl);

                    controlHighlight.BorderColor = "Green";

                    logger.Information("Captured control : {$capturedControl}", current);

                }
                catch (Exception ex)
                {
                    controlHighlight.BorderColor = "Red";
                    logger.Error(ex, ex.Message);
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

        private void LogElementDetails(string purpose , AutomationElement automationElement)
        {
            logger.Debug($"{purpose} -> AutomationId : {automationElement.Current.AutomationId}|Name : {automationElement.Current.Name} | ControlType : {automationElement.Current.ControlType}|ProcessId : {automationElement.Current.ProcessId}");
        }
   
        private bool ShouldCaptureNode(AutomationElement currentNode, AutomationElement nextNode)
        {
            //this is the desired leaf element and must be captured
            //Note : Sometimes although this is not the leaf element Access bridge provides details upto this element only directly using GetNodePathAt(x,y) api
            if (nextNode == null || currentNode.FindAll(TreeScope.Children, Condition.TrueCondition).Count == 0)
            {
                return true;
            }

            //if currentNode is single child , no need to capture this. During search we have exactly one path to follow
            AutomationElement parentNode = TreeWalker.RawViewWalker.GetParent(currentNode);
            var siblingsOfCurrentNode = parentNode.FindAll(TreeScope.Children, Condition.TrueCondition).ToList();
            if (siblingsOfCurrentNode.Count == 1)
            {
                return false;
            }

            //if the parent node has more than 3 children, we should capture node detail
            if (siblingsOfCurrentNode.Count > 3)
            {
                return true;
            }

            //if there are less than 4 sibling , however for any of the sibling node if number of descendant nodes is greater then 50, we should capture node detail
            if (siblingsOfCurrentNode.Any(a => a.GetDescendantsCount() > 50))
            {
                return true;
            }

            //if next node doesn't have unique identification properties, capture the current node
            if(string.IsNullOrEmpty(nextNode.Current.Name) && string.IsNullOrEmpty(nextNode.Current.AutomationId) && string.IsNullOrEmpty(nextNode.Current.HelpText))
            {
                return true;
            }

            return true;
        }

        /// <summary>
        /// Create a WinControlIdentityComponent from AutomationElement
        /// </summary>
        /// <param name="trackedElement"></param>
        /// <returns></returns>
        private WinControlIdentity CreateControlComponent(AutomationElement trackedElement, int depth)
        {
            WinControlIdentity capturedControl = new WinControlIdentity();
            capturedControl.Name = depth.ToString();
            capturedControl.Depth = depth;
            capturedControl.ProcessId = trackedElement.Current.ProcessId;                       
            capturedControl.NameProperty = trackedElement.Current.Name;
            capturedControl.AutomationId = trackedElement.Current.AutomationId;
            capturedControl.ClassName = trackedElement.Current.ClassName;
            capturedControl.WinControlType = ControlType.LookupById(trackedElement.Current.ControlType.Id).ToWinControlType();                  
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
        /// <param name="lastCapturedAncestorNode"></param>
        private int GetIndexInParent(AutomationElement currentNode, AutomationElement lastCapturedAncestorNode)
        {           
            Condition searchCondition = ConditionFactory.FromControlType(currentNode.Current.ControlType);
            searchCondition = searchCondition.AndPropertyCondition(new PropertyCondition(AutomationElement.IsContentElementProperty, currentNode.Current.IsContentElement));
            searchCondition = searchCondition.AndPropertyCondition(new PropertyCondition(AutomationElement.IsControlElementProperty, currentNode.Current.IsControlElement));


            if (!string.IsNullOrEmpty(currentNode.Current.Name))
            {
                searchCondition = searchCondition.AndName(currentNode.Current.Name);
            }

            if (!string.IsNullOrEmpty(currentNode.Current.AutomationId))
            {
                searchCondition = searchCondition.AndAutomationId(currentNode.Current.AutomationId);
            }

            if (!string.IsNullOrEmpty(currentNode.Current.ClassName))
            {
                searchCondition = searchCondition.AndClassName(currentNode.Current.ClassName);
            }

            if (!string.IsNullOrEmpty(currentNode.Current.AccessKey))
            {
                searchCondition = searchCondition.AndAccessKey(currentNode.Current.AccessKey);
            }

            if (!string.IsNullOrEmpty(currentNode.Current.HelpText))
            {
                searchCondition = searchCondition.AndHelpText(currentNode.Current.HelpText);
            }

            if (!string.IsNullOrEmpty(currentNode.Current.AcceleratorKey))
            {
                searchCondition = searchCondition.AndAccessKey(currentNode.Current.AcceleratorKey);
            }

            var foundElements = lastCapturedAncestorNode.FindAllDescendants(searchCondition);
          
            int index = 0;
            foreach (AutomationElement elem in foundElements)
            {
                index++;
                if (currentNode.GetRuntimeId().SequenceEqual(elem.GetRuntimeId()))
                {
                    return index;                        
                }
            }
            logger.Warning($"Failed to uniquely identify control.");
            return 1;
        }

        private void ShowControlHighlightRectangle(System.Windows.Rect boundingBox, string borderColor)
        {
            // Hide old rectangle.
            controlHighlight.Visible = false;

            // Show new rectangle.
            controlHighlight.Location = new BoundingBox((int)boundingBox.Left, (int)boundingBox.Top,
            (int)boundingBox.Width, (int)boundingBox.Height);

            controlHighlight.BorderColor = borderColor;
            controlHighlight.Visible = true;

        }

        private void ShowContainerHighlightRectangle(System.Windows.Rect boundingBox, string borderColor)
        {
            // Hide old rectangle.
            containerHighlight.Visible = false;

            // Show new rectangle.
            containerHighlight.Location = new BoundingBox((int)boundingBox.Left, (int)boundingBox.Top,
            (int)boundingBox.Width, (int)boundingBox.Height);

            containerHighlight.BorderColor = borderColor;
            containerHighlight.Visible = true;

        }

        public async Task StopCapture()
        {
            m_GlobalHook.MouseDownExt -= GlobalHookMouseDownExt;
            m_GlobalHook.Dispose();
            m_GlobalHook = null;
            captureTimer.Stop();
            await eventAggregator.PublishOnUIThreadAsync(capturedControls.ToList<ScrapedControl>());
            controlHighlight.Dispose();
            containerHighlight.Dispose();
            capturedControls = null;

            logger.Information("Win Scrapper has been stopped.");

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
