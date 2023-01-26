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
    class ScriptExecutorActorComponentTests
    {
        /// <summary>
        /// Validate that configured javascript can be executed by Script executor actor      
        /// </summary>
        [Test]
        public async Task ValidateThatScriptExecutorActorCanExecuteGivenJavaScript()
        {
            var entityManager = Substitute.For<IEntityManager>();
          
            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValueAsync<string>(Arg.Any<Argument>()).Returns("some java script to check if element is visible");
            argumentProcessor.GetValueAsync<object[]>(Arg.Any<Argument>()).Returns(new object[] { 1 });
            argumentProcessor.When(x => x.SetValueAsync(Arg.Any<Argument>(), Arg.Any<string>()))
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
                Arguments = new InArgument<object[]>() { Mode = ArgumentMode.Default, DefaultValue = new object[] { 1 } },
                Result = new OutArgument<string>() { Mode = ArgumentMode.DataBound, PropertyPath = "ScriptResult" }
            };
            await executeScriptActor.ActAsync();
            
            await argumentProcessor.Received(1).SetValueAsync<string>(Arg.Any<Argument>(), Arg.Is<string>("Element is visible"));
        }
       
    }
}

