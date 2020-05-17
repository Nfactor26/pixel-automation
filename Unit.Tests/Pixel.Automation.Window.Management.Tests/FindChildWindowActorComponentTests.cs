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
    public class FindChildWindowActorComponentTests
    {

        [Test]
        public void ValidateThatFindChildWindowActorCanLocateSingleChildWindow()
        {
            string childWindowTitle = "Don't Save";
            var parentWindow = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Notepad", Rectangle.Empty, true);
            var childWindow = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Don't Save", Rectangle.Empty, true);

            var entityManager = Substitute.For<IEntityManager>();

            var windowManager = Substitute.For<IApplicationWindowManager>();
            windowManager.FindAllChildWindows(Arg.Is<ApplicationWindow>(parentWindow), Arg.Is<string>(childWindowTitle), Arg.Any<MatchType>(), Arg.Is<bool>(true)).Returns(
                    new List<ApplicationWindow>() { childWindow });

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<string>(Arg.Any<InArgument<string>>()).Returns(childWindowTitle);
            argumentProcessor.GetValue<ApplicationWindow>(Arg.Any<InArgument<ApplicationWindow>>()).Returns(parentWindow);

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            entityManager.GetServiceOfType<IApplicationWindowManager>().Returns(windowManager);


            var findChildWindowActor = new FindChildWindowActorComponent()
            {
                EntityManager = entityManager,
                LookupMode = Core.Enums.LookupMode.FindSingle,
                MatchType = MatchType.Equals
            };

            findChildWindowActor.Act();

            argumentProcessor.Received(1).GetValue<string>(Arg.Any<InArgument<string>>());
            argumentProcessor.Received(1).GetValue<ApplicationWindow>(Arg.Any<InArgument<ApplicationWindow>>());
            windowManager.Received(1).FindAllChildWindows(parentWindow, childWindowTitle, MatchType.Equals, true);
            argumentProcessor.Received(1).SetValue<ApplicationWindow>(Arg.Any<OutArgument<ApplicationWindow>>(), childWindow);

        }

        [Test]
        public void ValidateThatFindChildWindowActorCanLocateChildWindowByIndex()
        {
            string childWindowTitle = "Don't Save";
            var parentWindow = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Notepad", Rectangle.Empty, true);
          
            var childWindowOne = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Don't Save", Rectangle.Empty, true);
            var childWindowTwo = new ApplicationWindow(int.MaxValue, IntPtr.Zero, "Don't Save", Rectangle.Empty, true);

            var entityManager = Substitute.For<IEntityManager>();

            var windowManager = Substitute.For<IApplicationWindowManager>();
            windowManager.FindAllChildWindows(Arg.Is<ApplicationWindow>(parentWindow), Arg.Is<string>(childWindowTitle), Arg.Any<MatchType>(), Arg.Is<bool>(true)).Returns(
                    new List<ApplicationWindow>() { childWindowOne, childWindowTwo });

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<string>(Arg.Any<InArgument<string>>()).Returns(childWindowTitle);
            argumentProcessor.GetValue<ApplicationWindow>(Arg.Any<InArgument<ApplicationWindow>>()).Returns(parentWindow);
            argumentProcessor.GetValue<int>(Arg.Any<InArgument<int>>()).Returns(1); // we want child window at index 1 to be retrieved

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            entityManager.GetServiceOfType<IApplicationWindowManager>().Returns(windowManager);


            var findChildWindowActor = new FindChildWindowActorComponent()
            {
                EntityManager = entityManager,
                LookupMode = LookupMode.FindAll,
                FilterMode = FilterMode.Index,
                MatchType = MatchType.Equals               
            };

            findChildWindowActor.Act();

            argumentProcessor.Received(1).GetValue<string>(Arg.Any<InArgument<string>>());
            argumentProcessor.Received(1).GetValue<ApplicationWindow>(Arg.Any<InArgument<ApplicationWindow>>());
            windowManager.Received(1).FindAllChildWindows(parentWindow, childWindowTitle, MatchType.Equals, true);
            argumentProcessor.Received(1).GetValue<int>(Arg.Any<InArgument<int>>());
            argumentProcessor.Received(1).SetValue<ApplicationWindow>(Arg.Any<OutArgument<ApplicationWindow>>(), childWindowTwo);


        }

        [Test]
        public void ValidateThatFindChildWindowActorCanLocateChildWindowByCustomFilter()
        {
            string childWindowTitle = "Don't Save";
            var parentWindow = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Notepad", Rectangle.Empty, true);

            var childWindowOne = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Don't Save", Rectangle.Empty, true);
            var childWindowTwo = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Don't Save", Rectangle.Empty, true);
            var childWindowThree = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Don't Save", Rectangle.Empty, true);

            var entityManager = Substitute.For<IEntityManager>();

            var windowManager = Substitute.For<IApplicationWindowManager>();
            windowManager.FindAllChildWindows(Arg.Is<ApplicationWindow>(parentWindow), Arg.Is<string>(childWindowTitle), Arg.Any<MatchType>(), Arg.Is<bool>(true)).Returns(
                    new List<ApplicationWindow>() { childWindowOne, childWindowTwo, childWindowThree });

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<string>(Arg.Any<InArgument<string>>()).Returns(childWindowTitle);
            argumentProcessor.GetValue<ApplicationWindow>(Arg.Any<InArgument<ApplicationWindow>>()).Returns(parentWindow);

            var scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.CreateDelegateAsync<Func<IComponent, ApplicationWindow, bool>>(Arg.Any<string>())
                .Returns((c, a) =>
                {
                    if(a.Equals(childWindowTwo))
                    {
                        return true;
                    }
                    return false;
                });

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            entityManager.GetScriptEngine().Returns(scriptEngine);
            entityManager.GetServiceOfType<IApplicationWindowManager>().Returns(windowManager);


            var findChildWindowActor = new FindChildWindowActorComponent()
            {
                EntityManager = entityManager,
                LookupMode = LookupMode.FindAll,
                FilterMode = FilterMode.Custom,
                MatchType = MatchType.Equals,
                Filter = new InArgument<string>() { ScriptFile = "FindWindow.csx"}
            };

            findChildWindowActor.Act();

            argumentProcessor.Received(1).GetValue<string>(Arg.Any<InArgument<string>>());
            argumentProcessor.Received(1).GetValue<ApplicationWindow>(Arg.Any<InArgument<ApplicationWindow>>());
            windowManager.Received(1).FindAllChildWindows(parentWindow, childWindowTitle, MatchType.Equals, true);
            scriptEngine.Received(2).CreateDelegateAsync<Func<IComponent, ApplicationWindow, bool>>("FindWindow.csx"); // 1 for each window in collecton until match found
            argumentProcessor.Received(1).SetValue<ApplicationWindow>(Arg.Any<OutArgument<ApplicationWindow>>(), childWindowTwo);
        }
    }
}