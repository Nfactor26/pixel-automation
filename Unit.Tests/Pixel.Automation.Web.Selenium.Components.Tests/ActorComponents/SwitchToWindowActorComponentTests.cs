using NSubstitute;
using NUnit.Framework;
using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Web.Selenium.Components.ActorComponents;
using System;

namespace Pixel.Automation.Web.Selenium.Components.Tests.ActorComponents
{
    class SwitchToWindowActorComponentTests
    {
        /// <summary>
        /// Validate that Switch to window actor component can switch to target window / tab.
        /// </summary>
        [Test]
        public void ValidateThatSwitchToWindowActorCanActiviateSpecifiedWindowOrTab()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<int>(Arg.Any<Argument>()).Returns(2);
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
                EntityManager = entityManager
            };
            switchToActor.Act();

            argumentProcessor.Received(1).GetValue<int>(Arg.Any<Argument>());
            webDriver.Received(1).SwitchTo();
        }

        /// <summary>
        /// Validate that Switch to window actor component throws exception if there are lesser number of window / tabs open then configured window / tab number to be activated
        /// </summary>
        [Test]
        public void ValidateThatSwitchToWindowActorThrowsExceptionIfFewerWindowsAreAvailableThenConfiguredIndex()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<int>(Arg.Any<Argument>()).Returns(2);
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
                EntityManager = entityManager
            };
            Assert.Throws<IndexOutOfRangeException>(() => { navigateActor.Act(); });           
        }
    }
}

