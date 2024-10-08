using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Automation.Window.Management.Components.Tests
{
    public class FindChildWindowActorComponentFixture
    {

        [Test]
        public async Task ValidateThatFindChildWindowActorCanLocateSingleChildWindow()
        {
            string childWindowTitle = "Don't Save";
            var parentWindow = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Notepad", BoundingBox.Empty, true);
            var childWindow = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Don't Save", BoundingBox.Empty, true);

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
            Assert.That(findChildWindowActor.LookupMode, Is.EqualTo(LookupMode.FindSingle));
            Assert.That(findChildWindowActor.MatchType, Is.EqualTo(MatchType.Equals));
            Assert.That(findChildWindowActor.FilterMode, Is.EqualTo(FilterMode.Index));

            await findChildWindowActor.ActAsync();

            await argumentProcessor.Received(1).GetValueAsync<string>(Arg.Any<InArgument<string>>());
            await argumentProcessor.Received(1).GetValueAsync<ApplicationWindow>(Arg.Any<InArgument<ApplicationWindow>>());
            windowManager.Received(1).FindAllChildWindows(parentWindow, childWindowTitle, MatchType.Equals, true);
            await argumentProcessor.Received(1).SetValueAsync<ApplicationWindow>(Arg.Any<OutArgument<ApplicationWindow>>(), childWindow);

        }

        [Test]
        public async Task ValidateThatFindChildWindowActorCanLocateChildWindowByIndex()
        {
            string childWindowTitle = "Don't Save";
            var parentWindow = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Notepad", BoundingBox.Empty, true);
          
            var childWindowOne = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Don't Save", BoundingBox.Empty, true);
            var childWindowTwo = new ApplicationWindow(int.MaxValue, IntPtr.Zero, "Don't Save", BoundingBox.Empty, true);

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
            Assert.That(findChildWindowActor.LookupMode, Is.EqualTo(LookupMode.FindAll));
            Assert.That(findChildWindowActor.MatchType, Is.EqualTo(MatchType.Equals));
            Assert.That(findChildWindowActor.FilterMode, Is.EqualTo(FilterMode.Index));           

            await findChildWindowActor.ActAsync();

            await argumentProcessor.Received(1).GetValueAsync<string>(Arg.Any<InArgument<string>>());
            await argumentProcessor.Received(1).GetValueAsync<ApplicationWindow>(Arg.Any<InArgument<ApplicationWindow>>());
            windowManager.Received(1).FindAllChildWindows(parentWindow, childWindowTitle, MatchType.Equals, true);
            await argumentProcessor.Received(1).GetValueAsync<int>(Arg.Any<InArgument<int>>());
            await argumentProcessor.Received(1).SetValueAsync<ApplicationWindow>(Arg.Any<OutArgument<ApplicationWindow>>(), childWindowTwo);


        }

        [Test]
        public async Task ValidateThatFindChildWindowActorCanLocateChildWindowByCustomFilter()
        {
            string childWindowTitle = "Don't Save";
            var parentWindow = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Notepad", BoundingBox.Empty, true);

            var childWindowOne = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Don't Save", BoundingBox.Empty, true);
            var childWindowTwo = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Don't Save", BoundingBox.Empty, true);
            var childWindowThree = new ApplicationWindow(int.MinValue, IntPtr.Zero, "Don't Save", BoundingBox.Empty, true);

            var entityManager = Substitute.For<IEntityManager>();

            var windowManager = Substitute.For<IApplicationWindowManager>();
            windowManager.FindAllChildWindows(Arg.Is<ApplicationWindow>(parentWindow), Arg.Is<string>(childWindowTitle), Arg.Any<MatchType>(), Arg.Is<bool>(true)).Returns(
                    new List<ApplicationWindow>() { childWindowOne, childWindowTwo, childWindowThree });

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValueAsync<string>(Arg.Any<InArgument<string>>()).Returns(childWindowTitle);
            argumentProcessor.GetValueAsync<ApplicationWindow>(Arg.Any<InArgument<ApplicationWindow>>()).Returns(parentWindow);

            var scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.CreateDelegateAsync<Func<IComponent, ApplicationWindow, Task<bool>>>(Arg.Any<string>())
                .Returns((c, a) =>
                {
                    if(a.Equals(childWindowTwo))
                    {
                        return Task.FromResult(true);
                    }
                    return Task.FromResult(false);
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
            Assert.That(findChildWindowActor.LookupMode, Is.EqualTo(LookupMode.FindAll));
            Assert.That(findChildWindowActor.MatchType, Is.EqualTo(MatchType.Equals));
            Assert.That(findChildWindowActor.FilterMode, Is.EqualTo(FilterMode.Custom));
            Assert.That(findChildWindowActor.Filter is not null); //should be initialized when FilterMode getter is called.

            findChildWindowActor.Filter.ScriptFile = "FindWindow.csx";

            await findChildWindowActor.ActAsync();

            await argumentProcessor.Received(1).GetValueAsync<string>(Arg.Any<InArgument<string>>());
            await argumentProcessor.Received(1).GetValueAsync<ApplicationWindow>(Arg.Any<InArgument<ApplicationWindow>>());
            windowManager.Received(1).FindAllChildWindows(parentWindow, childWindowTitle, MatchType.Equals, true);
            await scriptEngine.Received(2).CreateDelegateAsync<Func<IComponent, ApplicationWindow, Task<bool>>>("FindWindow.csx"); // 1 for each window in collecton until match found
            await argumentProcessor.Received(1).SetValueAsync<ApplicationWindow>(Arg.Any<OutArgument<ApplicationWindow>>(), childWindowTwo);
        }
    }
}