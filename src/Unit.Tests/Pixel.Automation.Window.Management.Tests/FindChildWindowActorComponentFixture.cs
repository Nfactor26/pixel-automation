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
using System.Threading.Tasks;

namespace Pixel.Automation.Window.Management.Tests
{
    public class FindChildWindowActorComponentFixture
    {

        [Test]
        public async Task ValidateThatFindChildWindowActorCanLocateSingleChildWindow()
        {
            string childWindowTitle = "Don't Save";
            var parentWindow = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Notepad", Rectangle.Empty, true);
            var childWindow = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Don't Save", Rectangle.Empty, true);

            var entityManager = Substitute.For<IEntityManager>();

            var windowManager = Substitute.For<IApplicationWindowManager>();
            windowManager.FindAllChildWindows(Arg.Is<ApplicationWindow>(parentWindow), Arg.Is<string>(childWindowTitle), Arg.Any<MatchType>(), Arg.Is<bool>(true)).Returns(
                    new List<ApplicationWindow>() { childWindow });

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValueAsync<string>(Arg.Any<InArgument<string>>()).Returns(childWindowTitle);
            argumentProcessor.GetValueAsync<ApplicationWindow>(Arg.Any<InArgument<ApplicationWindow>>()).Returns(parentWindow);

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            entityManager.GetServiceOfType<IApplicationWindowManager>().Returns(windowManager);


            var findChildWindowActor = new FindChildWindowActorComponent()
            {
                EntityManager = entityManager,
                LookupMode = LookupMode.FindSingle,
                MatchType = MatchType.Equals
            };
            Assert.AreEqual(LookupMode.FindSingle, findChildWindowActor.LookupMode);
            Assert.AreEqual(MatchType.Equals, findChildWindowActor.MatchType);
            Assert.AreEqual(FilterMode.Index, findChildWindowActor.FilterMode);

            await findChildWindowActor.ActAsync();

            argumentProcessor.Received(1).GetValueAsync<string>(Arg.Any<InArgument<string>>());
            argumentProcessor.Received(1).GetValueAsync<ApplicationWindow>(Arg.Any<InArgument<ApplicationWindow>>());
            windowManager.Received(1).FindAllChildWindows(parentWindow, childWindowTitle, MatchType.Equals, true);
            argumentProcessor.Received(1).SetValueAsync<ApplicationWindow>(Arg.Any<OutArgument<ApplicationWindow>>(), childWindow);

        }

        [Test]
        public async Task ValidateThatFindChildWindowActorCanLocateChildWindowByIndex()
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
            argumentProcessor.GetValueAsync<string>(Arg.Any<InArgument<string>>()).Returns(childWindowTitle);
            argumentProcessor.GetValueAsync<ApplicationWindow>(Arg.Any<InArgument<ApplicationWindow>>()).Returns(parentWindow);
            argumentProcessor.GetValueAsync<int>(Arg.Any<InArgument<int>>()).Returns(1); // we want child window at index 1 to be retrieved

            entityManager.GetArgumentProcessor().Returns(argumentProcessor);
            entityManager.GetServiceOfType<IApplicationWindowManager>().Returns(windowManager);


            var findChildWindowActor = new FindChildWindowActorComponent()
            {
                EntityManager = entityManager,
                LookupMode = LookupMode.FindAll,
                FilterMode = FilterMode.Index,
                MatchType = MatchType.Equals               
            };
            Assert.AreEqual(LookupMode.FindAll, findChildWindowActor.LookupMode);
            Assert.AreEqual(MatchType.Equals, findChildWindowActor.MatchType);
            Assert.AreEqual(FilterMode.Index, findChildWindowActor.FilterMode);           

            await findChildWindowActor.ActAsync();

            argumentProcessor.Received(1).GetValueAsync<string>(Arg.Any<InArgument<string>>());
            argumentProcessor.Received(1).GetValueAsync<ApplicationWindow>(Arg.Any<InArgument<ApplicationWindow>>());
            windowManager.Received(1).FindAllChildWindows(parentWindow, childWindowTitle, MatchType.Equals, true);
            argumentProcessor.Received(1).GetValueAsync<int>(Arg.Any<InArgument<int>>());
            argumentProcessor.Received(1).SetValueAsync<ApplicationWindow>(Arg.Any<OutArgument<ApplicationWindow>>(), childWindowTwo);


        }

        [Test]
        public async Task ValidateThatFindChildWindowActorCanLocateChildWindowByCustomFilter()
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
            argumentProcessor.GetValueAsync<string>(Arg.Any<InArgument<string>>()).Returns(childWindowTitle);
            argumentProcessor.GetValueAsync<ApplicationWindow>(Arg.Any<InArgument<ApplicationWindow>>()).Returns(parentWindow);

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
                MatchType = MatchType.Equals
            };
            Assert.AreEqual(LookupMode.FindAll, findChildWindowActor.LookupMode);
            Assert.AreEqual(MatchType.Equals, findChildWindowActor.MatchType);
            Assert.AreEqual(FilterMode.Custom, findChildWindowActor.FilterMode);
            Assert.NotNull(findChildWindowActor.Filter); //should be initialized when FilterMode getter is called.

            findChildWindowActor.Filter.ScriptFile = "FindWindow.csx";

            await findChildWindowActor.ActAsync();

            argumentProcessor.Received(1).GetValueAsync<string>(Arg.Any<InArgument<string>>());
            argumentProcessor.Received(1).GetValueAsync<ApplicationWindow>(Arg.Any<InArgument<ApplicationWindow>>());
            windowManager.Received(1).FindAllChildWindows(parentWindow, childWindowTitle, MatchType.Equals, true);
            await scriptEngine.Received(2).CreateDelegateAsync<Func<IComponent, ApplicationWindow, bool>>("FindWindow.csx"); // 1 for each window in collecton until match found
            argumentProcessor.Received(1).SetValueAsync<ApplicationWindow>(Arg.Any<OutArgument<ApplicationWindow>>(), childWindowTwo);
        }
    }
}