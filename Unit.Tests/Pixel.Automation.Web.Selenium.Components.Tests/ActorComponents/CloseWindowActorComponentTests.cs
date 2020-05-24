using NSubstitute;
using NUnit.Framework;
using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Web.Selenium.Components.ActorComponents;
using System;

namespace Pixel.Automation.Web.Selenium.Components.Tests.ActorComponents
{
    class CloseWindowActorComponentTests
    {
        /// <summary>
        /// Validate that Close Window Actor component can close window / tab at a configured index
        /// </summary>
        [Test]
        public void ValidateThatCloseWindowActorCanCloseSpecifiedWindowOrTab()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<int>(Arg.Any<Argument>()).Returns(2);
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            var webDriver = Substitute.For<IWebDriver>();
            webDriver.WindowHandles.Returns(new System.Collections.ObjectModel.ReadOnlyCollection<string>(new[] { "1", "2" }));
            WebApplication webAppliction = new WebApplication()
            {
                WebDriver = webDriver
            };
            entityManager.GetOwnerApplication<WebApplication>(Arg.Any<IComponent>()).Returns(webAppliction);


            var closeWindowActor = new CloseWindowActorComponent()
            {
                EntityManager = entityManager
            };
            closeWindowActor.Act();

            argumentProcessor.Received(1).GetValue<int>(Arg.Any<Argument>());
            webDriver.Received(1).SwitchTo();
            webDriver.Received(1).Close();
        }

        /// <summary>
        /// Validate that Close window actor component throws exception if there are less number of window / tabs open then configured window / tab number to be closed
        /// </summary>
        [Test]
        public void ValidateThatCloseWindowActorThrowsExceptionIfFewerWindowsAreAvailableThenConfiguredIndex()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<int>(Arg.Any<Argument>()).Returns(2);
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            var webDriver = Substitute.For<IWebDriver>();
            webDriver.WindowHandles.Returns(new System.Collections.ObjectModel.ReadOnlyCollection<string>(new[] { "1" }));
            WebApplication webAppliction = new WebApplication()
            {
                WebDriver = webDriver
            };
            entityManager.GetOwnerApplication<WebApplication>(Arg.Any<IComponent>()).Returns(webAppliction);


            var closeWindowActor = new CloseWindowActorComponent()
            {
                EntityManager = entityManager
            };           
            Assert.Throws<IndexOutOfRangeException>(() => { closeWindowActor.Act(); });
        }
    }
}

