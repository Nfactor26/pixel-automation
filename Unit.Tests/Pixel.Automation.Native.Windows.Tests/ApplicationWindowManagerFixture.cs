using NUnit.Framework;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Native.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Pixel.Automation.Nativew.Windows.Tests
{
    class ApplicationWindowManagerFixture
    {      
        //int processId;

        //[OneTimeSetUp]
        //public void OneTimeSetUp()
        //{
        //    var process = Process.Start(new ProcessStartInfo() { FileName = "calc" });
        //    Thread.Sleep(5000);
        //    int processId = process.Id;
        //}

        //[OneTimeTearDown]
        //public void OneTimeTearDown()
        //{
        //    var process = Process.GetProcessesByName("calculator").FirstOrDefault();
        //    process.Kill();
        //}


        //[TestCase("Calculator", MatchType.Equals, true, 1, "Calculator" )]
        //[TestCase("Calc", MatchType.StartsWith, true, 1, "Calculator")]
        //[TestCase("culator", MatchType.EndsWith, true, 1, "Calculator")]
        //[TestCase("cula", MatchType.Contains, true, 1, "Calculator")]
        //[TestCase("^C[a-z]*r$", MatchType.RegEx, true, 1, "Calculator")]

        //public void ValidateThatMainWindowCanBeLocated(string titleToMatch, MatchType matchType, bool visibleOnly, int expectedWindowCount, string mainWindowTitle)
        //{
        //    var applicationWindowManager = new ApplicationWindowManager();
        //    var foundWindows = applicationWindowManager.FindAllDesktopWindows(titleToMatch, matchType, visibleOnly);
        //    Assert.AreEqual(expectedWindowCount, foundWindows.Count());
        //    foreach(var window in foundWindows)
        //    {
        //        Assert.AreEqual(mainWindowTitle, window.WindowTitle);
        //    }
        //}

        //[TestCase("Calculator", MatchType.Equals, true, "Calculator", 1, "Calculator")]
        //[TestCase("Calc", MatchType.StartsWith, true, "Calculator", 1, "Calculator")]
        //[TestCase("culator", MatchType.EndsWith, true, "Calculator", 1, "Calculator")]
        //[TestCase("cula", MatchType.Contains, true, "Calculator", 1, "Calculator")]
        //[TestCase("^C[a-z]*r$", MatchType.RegEx, true, "Calculator", 1, "Calculator")]
        //public void ValidateThatChildWindowCanBeLocated(string titleToMatch, MatchType matchType, bool visibleOnly, string mainWindowTitle, int expectedChildWindowCount, string childWindowTitle)
        //{
        //    var applicationWindowManager = new ApplicationWindowManager();
        //    //Find the parent window first. Test case expected presence of only one parent window matching mainWindowTitle and should be visible
        //    var foundMainWindows = applicationWindowManager.FindAllDesktopWindows(mainWindowTitle, MatchType.Equals, true);
        //    Assert.AreEqual(1, foundMainWindows.Count());
        //    var parentWindow = foundMainWindows.FirstOrDefault();

        //    var foundChildWindows = applicationWindowManager.FindAllChildWindows(parentWindow, titleToMatch, matchType, visibleOnly);
        //    Assert.AreEqual(expectedChildWindowCount, foundChildWindows.Count());
        //    foreach (var window in foundChildWindows)
        //    {
        //        Assert.AreEqual(childWindowTitle, window.WindowTitle);
        //    }
        //}

        //[TestCase("Calculator", MatchType.Equals, true, "Calculator")]
        //public void ValidateThatWindowManagerCanSetForegroundWindow(string titleToMatch, MatchType matchType, bool visibleOnly, string mainWindowTitle)
        //{
        //    var applicationWindowManager = new ApplicationWindowManager();
        //    var foundWindows = applicationWindowManager.FindAllDesktopWindows(titleToMatch, matchType, visibleOnly);
        //    Assert.AreEqual(1, foundWindows.Count());
            
        //    var targetWindow = foundWindows.FirstOrDefault();
        //    Assert.AreEqual(mainWindowTitle, targetWindow.WindowTitle);

        //    //Minimize the window so that it is not the foreground window and assert the same
        //    applicationWindowManager.SetWindowState(targetWindow, WindowState.Minimize, false);
        //    Thread.Sleep(2000);
        //    var foreGroundWindow = applicationWindowManager.GetForeGroundWindow();
        //    Assert.AreNotEqual(mainWindowTitle, foreGroundWindow.WindowTitle);

        //    //set window as foreground window and assert the same
        //    applicationWindowManager.SetForeGroundWindow(targetWindow);
        //    Thread.Sleep(2000);
        //    foreGroundWindow = applicationWindowManager.GetForeGroundWindow();
        //    Assert.AreEqual(mainWindowTitle, foreGroundWindow.WindowTitle);
        //}

        //[TestCase("Calculator", MatchType.Equals, true, "Calculator")]
        //public void ValidateThatWindowManagerCanChangeWindowState(string titleToMatch, MatchType matchType, bool visibleOnly, string mainWindowTitle)
        //{
        //    var applicationWindowManager = new ApplicationWindowManager();
        //    var foundWindows = applicationWindowManager.FindAllDesktopWindows(titleToMatch, matchType, visibleOnly);
        //    Assert.AreEqual(1, foundWindows.Count());

        //    var targetWindow = foundWindows.FirstOrDefault();
        //    Assert.AreEqual(mainWindowTitle, targetWindow.WindowTitle);

        //    var currentWindowSize = applicationWindowManager.GetWindowSize(targetWindow.HWnd);

        //    //set window state to maximize
        //}
    }
}
