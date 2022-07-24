extern alias uiaComWrapper;
using Caliburn.Micro;
using Gma.System.MouseKeyHook;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Java.Access.Bridge.Components;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using uiaComWrapper::System.Windows.Automation;
using WindowsAccessBridgeInterop;


namespace Pixel.Automation.JAB.Scrapper
{
    public class JABControlScrapperComponent : PropertyChangedBase, IControlScrapper, IHandle<RepositoryApplicationOpenedEventArgs>
    {
        private readonly ILogger logger = Log.ForContext<JABControlScrapperComponent>();

        private readonly IEventAggregator eventAggregator;

        private IKeyboardMouseEvents m_GlobalHook;

        private readonly IScreenCapture screenCapture;

        public string DisplayName { get; } = "JAB Scrapper";

        string targetApplicationId = string.Empty;

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

        public JABControlScrapperComponent(IEventAggregator eventAggregator, IHighlightRectangleFactory highlightRectangleFactory, IScreenCapture screenCapture)
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

        AccessBridge accessBridge;

        /// <summary>
        /// Placeholder for parent control when scraping a child control relative to it
        /// </summary>
        AccessibleContextNode containerNode;

        ConcurrentQueue<ScrapedControl> capturedControls;

        System.Windows.Forms.Timer captureTimer = new System.Windows.Forms.Timer();



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

        /// <summary>
        /// Start highligting controls on hover and watch for control + click to capture control details
        /// </summary>
        public async Task StartCapture()
        {
            accessBridge = new AccessBridge();
            accessBridge.Initialize();
            
            capturedControls = new ConcurrentQueue<ScrapedControl>();

            controlHighlight = highlightRectangleFactory.CreateHighlightRectangle();
            containerHighlight = highlightRectangleFactory.CreateHighlightRectangle();
          
            captureTimer.Interval = 2000;
            captureTimer.Tick -= CaptureTimer_Tick;
            captureTimer.Tick += CaptureTimer_Tick;
            captureTimer.Start();

            if (m_GlobalHook == null)
            {
                m_GlobalHook = Hook.GlobalEvents();
            }
            m_GlobalHook.MouseDownExt -= GlobalHookMouseDownExt;
            m_GlobalHook.MouseDownExt += GlobalHookMouseDownExt;           
            logger.Information("Java Scrapper has been activated.");
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
                    {
                        return;
                    }

                    if (!accessBridge.Functions.IsJavaWindow(new IntPtr(trackedElement.Current.NativeWindowHandle)))
                    {
                        return;
                    }

                    using (var accessibleWindow = accessBridge.CreateAccessibleWindow(new IntPtr(trackedElement.Current.NativeWindowHandle)))
                    {
                        var controlPath = accessibleWindow.GetNodePathAt(new System.Drawing.Point(Convert.ToInt32(point.X), Convert.ToInt32(point.Y)));
                        AccessibleContextNode leafNode = controlPath.Last() as AccessibleContextNode;
                        var boundingBox = leafNode.GetScreenRectangle();
                        if(boundingBox.HasValue)
                        {
                            focusedRect = new System.Windows.Rect() { X = boundingBox.Value.X, Y = boundingBox.Value.Y, Width = boundingBox.Value.Width, Height = boundingBox.Value.Height };
                            ShowControlHighlightRectangle(focusedRect, "Orange");
                        }                       
                    }
       
                }
                catch (Exception ex)
                {
                    controlHighlight.BorderColor = "Red";
                    logger.Error(ex, ex.Message);
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
            
            captureTimer.Tick -= CaptureTimer_Tick;
            controlHighlight.Visible = false;

            Thread captureControlThread = new Thread(() =>
            {
                try
                {
                    System.Windows.Point point = new System.Windows.Point(e.X, e.Y);                 

                    AutomationElement trackedElement = AutomationElement.FromPoint(point);
                    if (trackedElement == null)
                    {
                        return;
                    }

                    if (!accessBridge.Functions.IsJavaWindow(new IntPtr(trackedElement.Current.NativeWindowHandle)))
                    {
                        return;
                    }
                    
                    using (var accessibleWindow = accessBridge.CreateAccessibleWindow(new IntPtr(trackedElement.Current.NativeWindowHandle)))
                    {                        
                        var nodesInPath = accessibleWindow.GetNodePathAt(new Point(Convert.ToInt32(point.X), Convert.ToInt32(point.Y)));                       
                        using (AccessibleContextNode leafNode = nodesInPath.Last() as AccessibleContextNode)
                        {
                            var boundingBox = leafNode.GetScreenRectangle();
                            focusedRect = new System.Windows.Rect() { X = boundingBox.Value.X, Y = boundingBox.Value.Y, Width = boundingBox.Value.Width, Height = boundingBox.Value.Height };

                            var controlScreenShot = screenCapture.CaptureArea(new BoundingBox(boundingBox.Value.X, boundingBox.Value.Y, boundingBox.Value.Width, boundingBox.Value.Height));                          
                            ShowControlHighlightRectangle(focusedRect, "Orange");

                            logger.Information("Processing control @ coordiante : {@Point}.", point);

                            bool isRelativeScraping = false;
                            List<AccessibleNode> controlPath = new List<AccessibleNode>();
                            foreach (var node in nodesInPath)
                            {
                                controlPath.Add(node);
                                //if any node in path matches contaier node, discard all the nodes until container node as we want to capture this control relative to container node
                                if (containerNode != null && containerNode.Equals(node))
                                {
                                    controlPath.Clear();
                                    isRelativeScraping = true;
                                }
                            }

                            //user clicked an element not relative to containerNode. Clear out the container and remove highlight
                            if (!isRelativeScraping && containerNode != null)
                            {
                                containerNode = null;
                                containerHighlight.Visible = false;
                            }

                            var startingNode = (isRelativeScraping ? controlPath.ElementAt(0) : accessibleWindow);
                            JavaControlIdentity rootNodeIdentity = CreateControlComponent(startingNode as AccessibleContextNode, 1);
                            rootNodeIdentity.SearchScope = Core.Enums.SearchScope.Children;
                            rootNodeIdentity.LookupType = (isRelativeScraping ? Core.Enums.LookupType.Relative : Core.Enums.LookupType.Default);
                            JavaControlIdentity currentNodeIdentity = rootNodeIdentity;
                            int pathLength = controlPath.Count;
                            AccessibleContextNode lastCapturedAncestorNode = accessibleWindow;

                            for (int i = 1; i < pathLength - 1; i++)
                            {
                                var currentNode = controlPath.ElementAt(i) as AccessibleContextNode;
                                var nextNode = controlPath.ElementAt(i + 1) as AccessibleContextNode;

                                if (ShouldCaptureNode(currentNode, nextNode))
                                {
                                    var nextNodeIdentity = CreateControlComponent(controlPath.ElementAt(i) as AccessibleContextNode, i + 1);
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
                                var leafNodeIdentity = CreateControlComponent(leafNode, pathLength);
                                leafNodeIdentity.Index = GetIndexInParent(leafNode, lastCapturedAncestorNode);
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
                                    //everything taken care of already
                                    break;

                                case MouseButtons.Right:
                                    containerNode = leafNode;
                                    ShowContainerHighlightRectangle(focusedRect, "Purple");
                                    break;
                            }

                            ScrapedControl scrapedControl = new ScrapedControl() { ControlData = rootNodeIdentity, ControlImage = controlScreenShot };
                            capturedControls.Enqueue(scrapedControl);

                            controlHighlight.BorderColor = "Green";
                        }                       
                    }
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


        /// <summary>
        /// Multiple controls can have same identifier properties. So, we try to match descendant controls of last captured ancestor to 
        /// current control's identifer and find the index at which current control can be located within this ancestor node.
        /// </summary>
        /// <param name="targetNode"></param>
        /// <returns></returns>
        private int GetIndexInParent(AccessibleContextNode targetNode, AccessibleContextNode lastCapturedAncestorNode)
        {
            return targetNode.FindIndexOfControl(lastCapturedAncestorNode);           
        }

        private JavaControlIdentity CreateControlComponent(AccessibleContextNode trackedElement, int depth)
        {
            var nodeInfo = trackedElement.GetInfo();
            var boundingBox = trackedElement.GetScreenRectangle().GetValueOrDefault();
            JavaControlIdentity controlDetails = new JavaControlIdentity()
            {               
                ControlName = nodeInfo.name,
                Description = nodeInfo.description,
                Role = nodeInfo.role,
                Depth = depth,            
                Name = depth.ToString()
            };
            return controlDetails;
        }

        /// <summary>
        /// Simple rules to determine if we should capture control details pointing to this node during scraping
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="currentNodeInfo"></param>
        /// <returns></returns>
        private bool ShouldCaptureNode(AccessibleContextNode currentNode, AccessibleContextNode nextNode)
        {
            var currentNodeInfo = currentNode.GetInfo();
            //this is the root element in the hierarchy
            if (currentNodeInfo.indexInParent == -1)
                return true;

            //this is the desired leaf element and must be captured
            //Note : Sometimes although this is not the leaf element Access bridge provides details upto this element only directly using GetNodePathAt(x,y) api
            if (currentNodeInfo.childrenCount == 0 || nextNode==null)
                return true;        


            AccessibleContextNode parentNode = currentNode.GetParent() as AccessibleContextNode;
            AccessibleContextInfo parentInfo = parentNode.GetInfo();

            //if this node is single child , no need to capture this. During search we have exactly one path to follow
            if (parentInfo.childrenCount == 1)
                return false;

            //if the parent node has more than 3 children, we should capture node detail
            if (parentInfo.childrenCount > 3)
                return true;

            //if there are less than 4 sibling , however for any of the sibling node if number of descendant nodes is greater then 50, we should capture node detail
            if(parentNode.GetChildren().Any(a => a.GetDescendantsCount()>50))
            {
                return true;
            }

            //if the next node doesn't have a name and description, always capture the current node
            var nextNodeInfo = nextNode?.GetInfo();
            if (string.IsNullOrEmpty(nextNodeInfo.name) && string.IsNullOrEmpty(nextNodeInfo.description))
                return true;
            
            //let's not capture this
            return false;
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


        ///<inheritdoc>
        public async Task StopCapture()
        {
            accessBridge.Dispose();
            accessBridge = null;
            m_GlobalHook.MouseDownExt -= GlobalHookMouseDownExt;
            m_GlobalHook.Dispose();
            m_GlobalHook = null;
            captureTimer.Stop();
            await eventAggregator.PublishOnUIThreadAsync(capturedControls.ToList<ScrapedControl>());
            controlHighlight.Dispose();
            containerHighlight.Dispose();
            capturedControls = null;
            logger.Information("Java Scrapper has been stopped.");
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
