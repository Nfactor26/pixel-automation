using NSubstitute;
using NUnit.Framework;
using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Web.Selenium.Components.ActorComponents;
using System;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Selenium.Components.Tests.ActorComponents
{
    class SwitchToWindowActorComponentTests
    {
        /// <summary>
        /// Validate that Switch to window actor component can switch to target window / tab.
        /// </summary>
        [Test]
        public async Task ValidateThatSwitchToWindowActorCanActiviateSpecifiedWindowOrTab()
        {
            int switchToWindow = 2;
            var entityManager = Substitute.For<IEntityManager>();

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValueAsync<int>(Arg.Any<Argument>()).Returns(switchToWindow);
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            var webDriver = Substitute.For<IWebDriver>();
            webDriver.WindowHandles.Returns(new System.Collections.ObjectModel.ReadOnlyCollection<string>(new[] { "1", "2" } ) );
            WebApplication webAppliction = new WebApplication()
            {
                WebDriver = webDriver
            };
            entityManager.GetOwnerApplication<WebApplication>(Arg.Any<IComponent>()).Returns(webAppliction);


            var switchToActor = new SwitchToWindowActorComponent()
            {
                EntityManager = entityManager,
                WindowNumber = new InArgument<int>() { DefaultValue = switchToWindow, Mode = ArgumentMode.Default }
            };
            await switchToActor.ActAsync();

            await argumentProcessor.Received(1).GetValueAsync<int>(Arg.Any<Argument>());
            webDriver.Received(2).SwitchTo();
        }

        /// <summary>
        /// Validate that Switch to window actor component throws exception if there are lesser number of window / tabs open then configured window / tab number to be activated
        /// </summary>
        [Test]
        public async Task ValidateThatSwitchToWindowActorThrowsExceptionIfFewerWindowsAreAvailableThenConfiguredIndex()
        {
            int switchToWindow = 2;
            var entityManager = Substitute.For<IEntityManager>();

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValueAsync<int>(Arg.Any<Argument>()).Returns(switchToWindow);
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            var webDriver = Substitute.For<IWebDriver>();
            webDriver.WindowHandles.Returns(new System.Collections.ObjectModel.ReadOnlyCollection<string>(new[] { "1"}));
            WebApplication webAppliction = new WebApplication()
            {
                WebDriver = webDriver
            };
            entityManager.GetOwnerApplication<WebApplication>(Arg.Any<IComponent>()).Returns(webAppliction);


            var navigateActor = new SwitchToWindowActorComponent()
            {
                EntityManager = entityManager,
                WindowNumber = new InArgument<int>() { DefaultValue = switchToWindow, Mode = ArgumentMode.Default }
            };
            Assert.ThrowsAsync<IndexOutOfRangeException>(async () => { await navigateActor.ActAsync(); });
            await Task.CompletedTask;
        }
    }
}

