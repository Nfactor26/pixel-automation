using Dawn;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Vanara.PInvoke;

namespace Pixel.Automation.Native.Windows
{
    public class ApplicationWindowManager : IApplicationWindowManager
    {
        public IEnumerable<ApplicationWindow> FindAllDesktopWindows(string titleToMatch, MatchType matchType, bool visibleOnly)
        {
            Guard.Argument(titleToMatch).NotNull().NotEmpty();        

            var foundWindows = new List<ApplicationWindow>();
            var matchingWindows = new List<ApplicationWindow>();

            User32.EnumWindowsProc filter = delegate (HWND hWnd, IntPtr lParam)
            {
                if(visibleOnly)
                {
                    //User32.WINDOWINFO windowInfo = default;
                    //User32.GetWindowInfo(hWnd, ref windowInfo);
                    //if ((windowInfo.dwStyle & User32.WindowStyles.WS_VISIBLE) == User32.WindowStyles.WS_VISIBLE)
                    if(User32.IsWindowVisible(hWnd))
                    {
                        foundWindows.Add(FromHwnd(hWnd.DangerousGetHandle()));
                    }
                }
                else
                {
                    foundWindows.Add(FromHwnd(hWnd.DangerousGetHandle()));
                }
                return true;
            };

            if (User32.EnumDesktopWindows(IntPtr.Zero, filter, IntPtr.Zero))
            {
                foreach (var window in foundWindows)
                {
                    switch (matchType)
                    {
                        case MatchType.Equals:
                            if (window.WindowTitle.Equals(titleToMatch))
                                matchingWindows.Add(window);
                            break;
                        case MatchType.StartsWith:
                            if (window.WindowTitle.StartsWith(titleToMatch))
                                matchingWindows.Add(window);
                            break;
                        case MatchType.EndsWith:
                            if (window.WindowTitle.EndsWith(titleToMatch))
                                matchingWindows.Add(window);
                            break;
                        case MatchType.Contains:
                            if (window.WindowTitle.Contains(titleToMatch))
                                matchingWindows.Add(window);
                            break;
                        case MatchType.RegEx:
                            Regex regEx = new Regex(titleToMatch);
                            if (regEx.IsMatch(window.WindowTitle))
                                matchingWindows.Add(window);
                            break;
                        case MatchType.NotEqualTo:
                            if (!window.WindowTitle.Equals(titleToMatch))
                                matchingWindows.Add(window);
                            break;
                        default:
                            break;

                    }

                }
            }

            return matchingWindows;
        }

        public IEnumerable<ApplicationWindow> FindAllChildWindows(ApplicationWindow parentWindow, string titleToMatch, MatchType matchType, bool visibleOnly)
        {

            Guard.Argument(titleToMatch).NotNull().NotEmpty();
            Guard.Argument(parentWindow).NotNull();
            Guard.Argument(parentWindow).Require(a => a.HWnd != IntPtr.Zero);

            var foundWindows = new List<ApplicationWindow>();
            var matchingWindows = new List<ApplicationWindow>();

            List<IntPtr> childWindows = GetChildWindows(parentWindow.HWnd);
            foreach (var hWnd in childWindows)
            {
                if (visibleOnly)
                {
                    //User32.WINDOWINFO windowInfo = default;
                    //User32.GetWindowInfo(hWnd, ref windowInfo);
                    //if ((windowInfo.dwStyle & User32.WindowStyles.WS_VISIBLE) == User32.WindowStyles.WS_VISIBLE)
                    if (User32.IsWindowVisible(hWnd))
                    {
                        foundWindows.Add(FromHwnd(hWnd));
                    }
                }
                else
                {
                    foundWindows.Add(FromHwnd(hWnd));
                }             
            }

            foreach (var window in foundWindows)
            {
                switch (matchType)
                {
                    case MatchType.Equals:
                        if (window.WindowTitle.Equals(titleToMatch))
                            matchingWindows.Add(window);
                        break;
                    case MatchType.StartsWith:
                        if (window.WindowTitle.StartsWith(titleToMatch))
                            matchingWindows.Add(window);
                        break;
                    case MatchType.EndsWith:
                        if (window.WindowTitle.EndsWith(titleToMatch))
                            matchingWindows.Add(window);
                        break;
                    case MatchType.Contains:
                        if (window.WindowTitle.Contains(titleToMatch))
                            matchingWindows.Add(window);
                        break;
                    case MatchType.RegEx:
                        Regex regEx = new Regex(titleToMatch);
                        if (regEx.IsMatch(window.WindowTitle))
                            matchingWindows.Add(window);
                        break;
                    case MatchType.NotEqualTo:
                        if (!window.WindowTitle.Equals(titleToMatch))
                            matchingWindows.Add(window);
                        break;
                    default:
                        break;

                }
            }

            return matchingWindows;
        }

        public ApplicationWindow GetForeGroundWindow()
        {
            HWND hWnd = User32.GetForegroundWindow();
            if(!hWnd.IsNull)
            {
                return FromHwnd(hWnd.DangerousGetHandle());
            }
            return default;
        }

        public void SetWindowPosition(ApplicationWindow applicationWindow, ScreenCoordinate topLeftPosition)
        {         
            Guard.Argument(applicationWindow).NotNull();
            Guard.Argument(applicationWindow).Require(a => a.HWnd != IntPtr.Zero);
            Guard.Argument(topLeftPosition).NotNull();
            Guard.Argument(topLeftPosition).Require(s => s.XCoordinate >= 0 && s.YCoordinate >= 0); //TODO : Assert for max bound for XCoordinate and YCoordinate as well

            User32.SetWindowPosFlags windowPosFlags = User32.SetWindowPosFlags.SWP_ASYNCWINDOWPOS | User32.SetWindowPosFlags.SWP_NOSIZE | User32.SetWindowPosFlags.SWP_SHOWWINDOW;
            User32.SetWindowPos(applicationWindow.HWnd, IntPtr.Zero , topLeftPosition.XCoordinate, topLeftPosition.YCoordinate, 0, 0, windowPosFlags);
        }

        public void SetWindowSize(ApplicationWindow applicationWindow, int newWidth, int newHeight)
        {
            Guard.Argument(applicationWindow).NotNull();
            Guard.Argument(applicationWindow).Require(a => a.HWnd != IntPtr.Zero);
            Guard.Argument(newWidth).Require(w => w > 0);
            Guard.Argument(newHeight).Require(h => h > 0);

            User32.SetWindowPosFlags windowPosFlags = User32.SetWindowPosFlags.SWP_ASYNCWINDOWPOS | User32.SetWindowPosFlags.SWP_NOMOVE | User32.SetWindowPosFlags.SWP_SHOWWINDOW;
            User32.SetWindowPos(applicationWindow.HWnd, IntPtr.Zero, 0, 0, newWidth, newHeight, windowPosFlags);
        }

        public void SetWindowState(ApplicationWindow applicationWindow, WindowState windowState, bool shouldActivate)
        {
            Guard.Argument(applicationWindow).NotNull();
            Guard.Argument(applicationWindow).Require(a => a.HWnd != IntPtr.Zero);

            ShowWindowCommand showWindowCommand = default;
            switch(windowState)
            {
                case WindowState.Maximize:
                    showWindowCommand = shouldActivate ? ShowWindowCommand.SW_SHOWMAXIMIZED : ShowWindowCommand.SW_MAXIMIZE;
                    break;
                case WindowState.Minimize:
                    showWindowCommand = shouldActivate ? ShowWindowCommand.SW_SHOWMINIMIZED : ShowWindowCommand.SW_MINIMIZE;
                    break;
                case WindowState.Restore:
                    showWindowCommand = ShowWindowCommand.SW_RESTORE;
                    break;
            }
            User32.ShowWindow(applicationWindow.HWnd, showWindowCommand);
        }

        public void FocusWindow(ApplicationWindow applicationWindow)
        {
            Guard.Argument(applicationWindow).NotNull();
            Guard.Argument(applicationWindow).Require(a => a.HWnd != IntPtr.Zero);

            User32.SetFocus(applicationWindow.HWnd);
        }

        public void SetForeGroundWindow(ApplicationWindow applicationWindow)
        {
            Guard.Argument(applicationWindow).NotNull();
            Guard.Argument(applicationWindow).Require(a => a.HWnd != IntPtr.Zero);

            User32.WINDOWINFO windowInfo = default;
            User32.GetWindowInfo(applicationWindow.HWnd, ref windowInfo);
            if((windowInfo.dwStyle & User32.WindowStyles.WS_MINIMIZE) == User32.WindowStyles.WS_MINIMIZE)
            {
                SetWindowState(applicationWindow, WindowState.Restore, true);
            }

            User32.SetForegroundWindow(applicationWindow.HWnd);
        }

        public Rectangle GetWindowSize(IntPtr hWnd)
        {           
            Guard.Argument(hWnd).Require(h => h != IntPtr.Zero);

            User32.WINDOWINFO windowInfo = default;
            User32.GetWindowInfo(hWnd, ref windowInfo);
            if ((windowInfo.dwStyle & User32.WindowStyles.WS_MINIMIZE) == User32.WindowStyles.WS_MINIMIZE)
            {
                return Rectangle.Empty;
            }

            User32.GetWindowRect(hWnd, out RECT lpRect);
            return new Rectangle(lpRect.left, lpRect.top, lpRect.Width, lpRect.Height);
        }

        public string GetWindowTitle(IntPtr hWnd)
        {        
            Guard.Argument(hWnd).Require(h => h != IntPtr.Zero);

            StringBuilder sb = new StringBuilder(255);
            User32.GetWindowText(hWnd, sb, sb.Capacity + 1);
            return sb.ToString() ?? string.Empty;
        }

        public ApplicationWindow FromHwnd(IntPtr hWnd)
        {
            Guard.Argument(hWnd).Require(h => h != IntPtr.Zero);
      
            User32.GetWindowThreadProcessId(hWnd, out uint processId);
            string title = GetWindowTitle(hWnd);
            Rectangle windowSize = GetWindowSize(hWnd);
          
            User32.WINDOWINFO windowInfo = default;
            User32.GetWindowInfo(hWnd, ref windowInfo);
            bool isVisible = ((windowInfo.dwStyle & User32.WindowStyles.WS_VISIBLE) == User32.WindowStyles.WS_VISIBLE);

            return new ApplicationWindow((int)processId, hWnd, title, windowSize, isVisible);
        }

        /// <summary>
        /// Returns a list of child windows
        /// </summary>
        /// <param name="parent">Parent of the windows to return</param>
        /// <returns>List of child windows</returns>
        private List<IntPtr> GetChildWindows(IntPtr parent)
        {
            List<IntPtr> result = new List<IntPtr>();
            GCHandle listHandle = GCHandle.Alloc(result);
            try
            {
                User32.EnumWindowsProc childProc = new User32.EnumWindowsProc(EnumWindow);
                User32.EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return result;
        }

        /// <summary>
        /// Callback method to be used when enumerating windows.
        /// </summary>
        /// <param name="handle">Handle of the next window</param>
        /// <param name="pointer">Pointer to a GCHandle that holds a reference to the list to fill</param>
        /// <returns>True to continue the enumeration, false to bail</returns>
        private bool EnumWindow(HWND handle, IntPtr pointer)
        {
            GCHandle gch = GCHandle.FromIntPtr(pointer);
            List<HWND> list = gch.Target as List<HWND>;
            if (list == null)
            {
                throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
            }
            list.Add(handle);
            //  You can modify this to check to see if you want to cancel the operation, then return a null here
            return true;
        }

    }
}
