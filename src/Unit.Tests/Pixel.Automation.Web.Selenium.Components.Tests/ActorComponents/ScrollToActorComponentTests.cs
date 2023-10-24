using NSubstitute;
using NUnit.Framework;
using OpenQA.Selenium;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Selenium.Components.Tests.ActorComponents
{
    class ScrollToActorComponentTests
    {
        /// <summary>
        /// Validate that scroll to actor component can scroll to a target control on web page
        /// </summary>
        [Test]
        public async Task ValidateThatScrollToActorCanScrollTargetControlInView()
        {
            var entityManager = Substitute.For<IEntityManager>();
        
            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValueAsync<int>(Arg.Any<Argument>()).Returns(10);
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            IWebElement targetControl = Substitute.For<IWebElement>();
            targetControl.Location.Returns(new System.Drawing.Point(100, 800));
            UIControl uiControl = Substitute.For<UIControl>();
            uiControl.GetApiControl<IWebElement>().Returns(targetControl);

            var controlEntity = Substitute.For<Entity, IControlEntity>();
            (controlEntity as IControlEntity).GetControl().Returns(uiControl);

            var webDriver = Substitute.For<IWebDriver, IJavaScriptExecutor>();
            (webDriver as IJavaScriptExecutor).ExecuteScript(Arg.Any<string>()).Returns(true);
            WebApplication webAppliction = new WebApplication()
            {
                WebDriver = webDriver
            };
            entityManager.GetOwnerApplication<WebApplication>(Arg.Any<IComponent>()).Returns(webAppliction);

            var scrollToActor = new ScrollToActorComponent()
            {
                EntityManager = entityManager,
                Parent = controlEntity
            };
            await scrollToActor.ActAsync();

            await argumentProcessor.Received(2).GetValueAsync<int>(Arg.Any<Argument>());
            (webDriver as IJavaScriptExecutor).Received(1).ExecuteScript(Arg.Is<string>("window.scroll(110, 810);"));
        }
    }
}

