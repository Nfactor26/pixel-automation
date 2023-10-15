using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;

namespace Pixel.Automation.Native.Linux;

public class ApplicationWindowManager : IApplicationWindowManager
{
    public IEnumerable<ApplicationWindow> FindAllChildWindows(ApplicationWindow parentWindow, string titleToMatch, Core.Enums.MatchType matchType, bool visibleOnly)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ApplicationWindow> FindAllDesktopWindows(string titleToMatch, Core.Enums.MatchType matchType, bool visibleOnly)
    {
        throw new NotImplementedException();
    }

    public void FocusWindow(ApplicationWindow applicationWindow)
    {
        throw new NotImplementedException();
    }

    public ApplicationWindow FromHwnd(IntPtr hWnd)
    {
        throw new NotImplementedException();
    }

    public ApplicationWindow FromProcessId(int processId)
    {
        throw new NotImplementedException();
    }

    public ApplicationWindow GetForeGroundWindow()
    {
        throw new NotImplementedException();
    }

    public BoundingBox GetWindowSize(IntPtr hWnd)
    {
        throw new NotImplementedException();
    }

    public string GetWindowTitle(IntPtr hWnd)
    {
        throw new NotImplementedException();
    }

    public bool IsForeGroundWindow(ApplicationWindow window)
    {
        throw new NotImplementedException();
    }

    public void SetForeGroundWindow(ApplicationWindow applicationWindow)
    {
       
    }

    public void SetWindowPosition(ApplicationWindow applicationWindow, ScreenCoordinate topLeftPosition)
    {
        throw new NotImplementedException();
    }

    public void SetWindowSize(ApplicationWindow applicationWindow, int newWidth, int newHeight)
    {
        throw new NotImplementedException();
    }

    public void SetWindowState(ApplicationWindow applicationWindow, WindowState windowState, bool shouldActivate)
    {
        throw new NotImplementedException();
    }
}
