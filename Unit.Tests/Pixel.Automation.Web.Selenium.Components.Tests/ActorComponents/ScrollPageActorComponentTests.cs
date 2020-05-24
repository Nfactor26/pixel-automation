using NSubstitute;
using NUnit.Framework;
using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.Web.Selenium.Components.Tests.ActorComponents
{
    class ScrollPageActorComponentTests
    {
        /// <summary>
        /// Validate that scroll page actor component can scroll window by configured horizontal and vertical scroll amount
        /// </summary>
        [Test]
        public void ValidateThatScrollPageActorCanScrollBrowserWindow()
        {
            var entityManager = Substitute.For<IEntityManager>();
            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<int>(Arg.Any<Argument>()).Returns(100);
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            var webDriver = Substitute.For<IWebDriver, IJavaScriptExecutor>();
            (webDriver as IJavaScriptExecutor).ExecuteScript(Arg.Any<string>()).Returns(true); 
            WebApplication webAppliction = new WebApplication()
            {
                WebDriver = webDriver
            };
            entityManager.GetOwnerApplication<WebApplication>(Arg.Any<IComponent>()).Returns(webAppliction);

            var scrollPageActorComponent = new ScrollPageActorComponent()
            {
                EntityManager = entityManager                
            };
            scrollPageActorComponent.Act();

            argumentProcessor.Received(2).GetValue<int>(Arg.Any<Argument>());
            (webDriver as IJavaScriptExecutor).Received(1).ExecuteScript(Arg.Is<string>("window.scroll(100, 100);"));
        }
    }
}
