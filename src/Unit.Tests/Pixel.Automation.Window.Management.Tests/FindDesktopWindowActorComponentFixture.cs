﻿using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Pixel.Automation.Window.Management.Tests
{
    public class FindDesktopWindowActorComponentFixture
    {

        [Test]
        public async Task ValidateThatFindDesktopWindowActorCanLocateSingleWindow()
        {
            string windowTitle = "Notepad";          
            var window = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Notepad", Rectangle.Empty, true);

            var entityManager = Substitute.For<IEntityManager>();

            var windowManager = Substitute.For<IApplicationWindowManager>();
            windowManager.FindAllDesktopWindows(Arg.Is<string>(windowTitle), Arg.Any<MatchType>(), Arg.Is<bool>(true)).Returns(
                    new List<ApplicationWindow>() { window });

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValueAsync<string>(Arg.Any<InArgument<string>>()).Returns(windowTitle);         

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            entityManager.GetServiceOfType<IApplicationWindowManager>().Returns(windowManager);


            var findDesktopWindowActor = new FindDesktopWindowsActorComponent()
            {
                EntityManager = entityManager,
                LookupMode = LookupMode.FindSingle,
                MatchType = MatchType.Equals
            };
            Assert.AreEqual(LookupMode.FindSingle, findDesktopWindowActor.LookupMode);
            Assert.AreEqual(MatchType.Equals, findDesktopWindowActor.MatchType);
            Assert.AreEqual(FilterMode.Index, findDesktopWindowActor.FilterMode);

            await findDesktopWindowActor.ActAsync();

            await argumentProcessor.Received(1).GetValueAsync<string>(Arg.Any<InArgument<string>>());
            windowManager.Received(1).FindAllDesktopWindows(windowTitle, MatchType.Equals, true);
            await argumentProcessor.Received(1).SetValueAsync<ApplicationWindow>(Arg.Any<OutArgument<ApplicationWindow>>(), window);

        }

        /// <summary>
        /// Multiple Desktop windows can be located with configured search criteria. FindDesktopWindowActor allows retrieving desired window by Index.
        /// Validate that when multiple windows are located, correct window is returned at specified Index.
        /// </summary>
        [Test]
        public async Task ValidateThatFinDesktopWindowActorCanLocateWindowByIndex()
        {
            string windowTitle = "Notepad";       

            var windowOne = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Notepad", Rectangle.Empty, true);
            var windowTwo = new ApplicationWindow(int.MaxValue, IntPtr.Zero, "Notepad", Rectangle.Empty, true);

            var entityManager = Substitute.For<IEntityManager>();

            var windowManager = Substitute.For<IApplicationWindowManager>();
            windowManager.FindAllDesktopWindows(Arg.Is<string>(windowTitle), Arg.Any<MatchType>(), Arg.Is<bool>(true)).Returns(
                    new List<ApplicationWindow>() { windowOne, windowTwo });

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValueAsync<string>(Arg.Any<InArgument<string>>()).Returns(windowTitle);         
            argumentProcessor.GetValueAsync<int>(Arg.Any<InArgument<int>>()).Returns(1); // we want window at index 1 to be retrieved

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            entityManager.GetServiceOfType<IApplicationWindowManager>().Returns(windowManager);


            var findDesktopWindowActor = new FindDesktopWindowsActorComponent()
            {
                EntityManager = entityManager,
                LookupMode = LookupMode.FindAll,
                FilterMode = FilterMode.Index,
                MatchType = MatchType.Equals
            };
            Assert.AreEqual(LookupMode.FindAll, findDesktopWindowActor.LookupMode);
            Assert.AreEqual(MatchType.Equals, findDesktopWindowActor.MatchType);
            Assert.AreEqual(FilterMode.Index, findDesktopWindowActor.FilterMode);

            await findDesktopWindowActor.ActAsync();

            await argumentProcessor.Received(1).GetValueAsync<string>(Arg.Any<InArgument<string>>());         
            windowManager.Received(1).FindAllDesktopWindows(windowTitle, MatchType.Equals, true);
            await argumentProcessor.Received(1).GetValueAsync<int>(Arg.Any<InArgument<int>>());
            await argumentProcessor.Received(1).SetValueAsync<ApplicationWindow>(Arg.Any<OutArgument<ApplicationWindow>>(), windowTwo);


        }

        [Test]
        public async Task ValidateThatFindChildWindowActorCanLocateChildWindowByCustomFilter()
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
            argumentProcessor.GetValueAsync<string>(Arg.Any<InArgument<string>>()).Returns(windowTitle);
           
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
                MatchType = MatchType.Equals                
            };
            Assert.AreEqual(LookupMode.FindAll, findDesktopWindowActor.LookupMode);
            Assert.AreEqual(MatchType.Equals, findDesktopWindowActor.MatchType);
            Assert.AreEqual(FilterMode.Custom, findDesktopWindowActor.FilterMode);
            Assert.NotNull(findDesktopWindowActor.Filter); //Filter should be initialized on when FilterMode getter is called

            findDesktopWindowActor.Filter.ScriptFile = "FindWindow.csx";
            await findDesktopWindowActor.ActAsync();

            await argumentProcessor.Received(1).GetValueAsync<string>(Arg.Any<InArgument<string>>());
            windowManager.Received(1).FindAllDesktopWindows(windowTitle, MatchType.Equals, true);
            await scriptEngine.Received(2).CreateDelegateAsync<Func<IComponent, ApplicationWindow, bool>>("FindWindow.csx"); // 1 for each window in collecton until match found
            await argumentProcessor.Received(1).SetValueAsync<ApplicationWindow>(Arg.Any<OutArgument<ApplicationWindow>>(), windowTwo);


        }
    }
}
