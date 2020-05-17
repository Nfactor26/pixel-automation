using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Pixel.Automation.Window.Management.Tests
{
    public class FindDesktopWindowActorComponentTests
    {

        [Test]
        public void ValidateThatFindDesktopWindowActorCanLocateSingleWindow()
        {
            string windowTitle = "Notepad";          
            var window = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Notepad", Rectangle.Empty, true);

            var entityManager = Substitute.For<IEntityManager>();

            var windowManager = Substitute.For<IApplicationWindowManager>();
            windowManager.FindAllDesktopWindows(Arg.Is<string>(windowTitle), Arg.Any<MatchType>(), Arg.Is<bool>(true)).Returns(
                    new List<ApplicationWindow>() { window });

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<string>(Arg.Any<InArgument<string>>()).Returns(windowTitle);         

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            entityManager.GetServiceOfType<IApplicationWindowManager>().Returns(windowManager);


            var findDesktopWindowActor = new FindDesktopWindowsActorComponent()
            {
                EntityManager = entityManager,
                LookupMode = Core.Enums.LookupMode.FindSingle,
                MatchType = MatchType.Equals
            };

            findDesktopWindowActor.Act();

            argumentProcessor.Received(1).GetValue<string>(Arg.Any<InArgument<string>>());
            windowManager.Received(1).FindAllDesktopWindows(windowTitle, MatchType.Equals, true);
            argumentProcessor.Received(1).SetValue<ApplicationWindow>(Arg.Any<OutArgument<ApplicationWindow>>(), window);

        }

        /// <summary>
        /// Multiple Desktop windows can be located with configured search criteria. FindDesktopWindowActor allows retrieving desired window by Index.
        /// Validate that when multiple windows are located, correct window is returned at specified Index.
        /// </summary>
        [Test]
        public void ValidateThatFinDesktopWindowActorCanLocateWindowByIndex()
        {
            string windowTitle = "Notepad";       

            var windowOne = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Notepad", Rectangle.Empty, true);
            var windowTwo = new ApplicationWindow(int.MaxValue, IntPtr.Zero, "Notepad", Rectangle.Empty, true);

            var entityManager = Substitute.For<IEntityManager>();

            var windowManager = Substitute.For<IApplicationWindowManager>();
            windowManager.FindAllDesktopWindows(Arg.Is<string>(windowTitle), Arg.Any<MatchType>(), Arg.Is<bool>(true)).Returns(
                    new List<ApplicationWindow>() { windowOne, windowTwo });

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<string>(Arg.Any<InArgument<string>>()).Returns(windowTitle);         
            argumentProcessor.GetValue<int>(Arg.Any<InArgument<int>>()).Returns(1); // we want window at index 1 to be retrieved

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            entityManager.GetServiceOfType<IApplicationWindowManager>().Returns(windowManager);


            var findDesktopWindowActor = new FindDesktopWindowsActorComponent()
            {
                EntityManager = entityManager,
                LookupMode = LookupMode.FindAll,
                FilterMode = FilterMode.Index,
                MatchType = MatchType.Equals
            };

            findDesktopWindowActor.Act();

            argumentProcessor.Received(1).GetValue<string>(Arg.Any<InArgument<string>>());         
            windowManager.Received(1).FindAllDesktopWindows(windowTitle, MatchType.Equals, true);
            argumentProcessor.Received(1).GetValue<int>(Arg.Any<InArgument<int>>());
            argumentProcessor.Received(1).SetValue<ApplicationWindow>(Arg.Any<OutArgument<ApplicationWindow>>(), windowTwo);


        }

        [Test]
        public void ValidateThatFindChildWindowActorCanLocateChildWindowByCustomFilter()
        {
            string windowTitle = "Notepad";

            var windowOne = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Notepad", Rectangle.Empty, true);
            var windowTwo = new ApplicationWindow(int.MaxValue, IntPtr.Zero, "Notepad", Rectangle.Empty, true);
            var windowThree = new ApplicationWindow(int.MaxValue, IntPtr.Zero, "Notepad", Rectangle.Empty, true);

            var entityManager = Substitute.For<IEntityManager>();

            var windowManager = Substitute.For<IApplicationWindowManager>();
            windowManager.FindAllDesktopWindows(Arg.Is<string>(windowTitle), Arg.Any<MatchType>(), Arg.Is<bool>(true)).Returns(
                    new List<ApplicationWindow>() { windowOne, windowTwo, windowThree });

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<string>(Arg.Any<InArgument<string>>()).Returns(windowTitle);
           
            var scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.CreateDelegateAsync<Func<IComponent, ApplicationWindow, bool>>(Arg.Any<string>())
                .Returns((c, a) =>
                {
                    if (a.Equals(windowTwo))
                    {
                        return true;
                    }
                    return false;
                });

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            entityManager.GetScriptEngine().Returns(scriptEngine);
            entityManager.GetServiceOfType<IApplicationWindowManager>().Returns(windowManager);


            var findDesktopWindowActor = new FindDesktopWindowsActorComponent()
            {
                EntityManager = entityManager,
                LookupMode = LookupMode.FindAll,
                FilterMode = FilterMode.Custom,
                MatchType = MatchType.Equals,
                Filter = new InArgument<string>() { ScriptFile = "FindWindow.csx" }
            };

            findDesktopWindowActor.Act();

            argumentProcessor.Received(1).GetValue<string>(Arg.Any<InArgument<string>>());
            windowManager.Received(1).FindAllDesktopWindows(windowTitle, MatchType.Equals, true);
            scriptEngine.Received(2).CreateDelegateAsync<Func<IComponent, ApplicationWindow, bool>>("FindWindow.csx"); // 1 for each window in collecton until match found
            argumentProcessor.Received(1).SetValue<ApplicationWindow>(Arg.Any<OutArgument<ApplicationWindow>>(), windowTwo);


        }
    }
}
