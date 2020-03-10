using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Pixel.Automation.Core.Interfaces
{
    public interface IApplicationWindowManager
    {
        /// <summary>
        /// Find all top level windows that match the title.
        /// </summary>
        /// <param name="titleToMatch">Title of the window to match</param>
        /// <param name="matchType">Match criteria to use e.g. title starts with or contains specified text</param>
        /// <param name="visibleOnly">Whether to look for only visible windows</param>
        /// <returns></returns>
        IEnumerable<ApplicationWindow> FindAllDesktopWindows(string titleToMatch, MatchType matchType, bool visibleOnly);

        /// <summary>
        /// Find all child windows of a given top level window that match the title
        /// </summary>
        /// <param name="parentWindow">Parent window whose child window needs to be looked</param>
        /// <param name="titleToMatch">Title of the window to match</param>
        /// <param name="matchType">Match criteria to use e.g. title starts with or contains specified text</param>
        /// <param name="visibleOnly">Whether to look for only visible windows</param>
        /// <returns></returns>
        IEnumerable<ApplicationWindow> FindAllChildWindows(ApplicationWindow parentWindow, string titleToMatch, MatchType matchType, bool visibleOnly);

        /// <summary>
        /// Get the foreground window
        /// </summary>
        /// <returns></returns>
        ApplicationWindow GetForeGroundWindow();

        /// <summary>
        /// Set the position of window on desktop
        /// </summary>
        /// <param name="applicationWindow"></param>
        /// <param name="topLeftPosition"></param>
        void SetWindowPosition(ApplicationWindow applicationWindow, ScreenCoordinate topLeftPosition);

        /// <summary>
        /// Set the size of the window 
        /// </summary>
        /// <param name="applicationWindow"></param>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        void SetWindowSize(ApplicationWindow applicationWindow, int newWidth, int newHeight);

        /// <summary>
        /// Set the current state of window to specified <see cref="WindowState"/>
        /// </summary>
        /// <param name="applicationWindow">Target application window whose state needs to be modified</param>
        /// <param name="windowState">Desired window state e.g. Minimize/Maximize/Restore</param>
        /// <param name="shouldActivate">If true , activate the window as well</param>
        void SetWindowState(ApplicationWindow applicationWindow, WindowState windowState, bool shouldActivate);

        /// <summary>
        /// Set window as foreground window
        /// </summary>
        /// <param name="applicationWindow"></param>
        void SetForeGroundWindow(ApplicationWindow applicationWindow);

        /// <summary>
        /// Set focus to window
        /// </summary>
        /// <param name="applicationWindow"></param>
        void FocusWindow(ApplicationWindow applicationWindow);

        /// <summary>
        /// Get the bounding box of windows
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        Rectangle GetWindowSize(IntPtr hWnd);

        /// <summary>
        /// Get the title of window
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        string GetWindowTitle(IntPtr hWnd);

        /// <summary>
        /// Create ApplicationWindow from a handle to window
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        ApplicationWindow FromHwnd(IntPtr hWnd);
    }
}
