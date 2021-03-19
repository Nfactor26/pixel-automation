using NSubstitute;
using NUnit.Framework;
using OpenQA.Selenium;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Web.Selenium.Components.ActorComponents;

namespace Pixel.Automation.Web.Selenium.Components.Tests.ActorComponents
{
    class ScriptExecutorActorComponentTests
    {
        /// <summary>
        /// Validate that configured javascript can be executed by Script executor actor      
        /// </summary>
        [Test]
        public void ValidateThatScriptExecutorActorCanExecuteGivenJavaScript()
        {
            var entityManager = Substitute.For<IEntityManager>();
          
            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValue<string>(Arg.Any<Argument>()).Returns("some java script to check if element is visible");
            argumentProcessor.GetValue<object[]>(Arg.Any<Argument>()).Returns(new object[] { 1 });
            argumentProcessor.When(x => x.SetValue(Arg.Any<Argument>(), Arg.Any<string>()))
                .Do(p =>
                {

                });
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            var webDriver = Substitute.For<IWebDriver, IJavaScriptExecutor>();
            (webDriver as IJavaScriptExecutor).ExecuteScript(Arg.Any<string>(), Arg.Any<IWebElement>(), Arg.Any<object>())
                .Returns("Element is visible");
            WebApplication webAppliction = new WebApplication()
            {
                WebDriver = webDriver
            };
            entityManager.GetOwnerApplication<WebApplication>(Arg.Any<IComponent>()).Returns(webAppliction);

            IWebElement targetControl = Substitute.For<IWebElement>();
            targetControl.GetAttribute(Arg.Any<string>()).Returns("Enter your search term");
            UIControl uiControl = Substitute.For<UIControl>();
            uiControl.GetApiControl<IWebElement>().Returns(targetControl);

            var controlEntity = Substitute.For<Entity, IControlEntity>();
            (controlEntity as IControlEntity).GetControl().Returns(uiControl);

            var executeScriptActor = new ScriptExecutorActorComponent()
            {
                EntityManager = entityManager,
                Parent = controlEntity,
                Arguments = new InArgument<object[]>() { Mode = ArgumentMode.Default, DefaultValue = new object[] { 1 } }
            };
            executeScriptActor.Act();
            
            argumentProcessor.Received(1).SetValue<string>(Arg.Any<Argument>(), Arg.Is<string>("Element is visible"));
        }
       
    }
}

