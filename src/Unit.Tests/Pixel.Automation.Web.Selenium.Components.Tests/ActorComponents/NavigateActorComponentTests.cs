using NSubstitute;
using NUnit.Framework;
using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Selenium.Components.Tests.ActorComponents
{
    class NavigateActorComponentTests
    {
        /// <summary>
        /// Validate that Click actor component can perform click on a web element
        /// </summary>
        [Test]
        public async Task ValidateThatNavigateActorComponentCanNavigateBrowserToAGivenUri()
        {
            var entityManager = Substitute.For<IEntityManager>();
         
            var argumentProcessor = Substitute.For<IArgumentProcessor>();
            argumentProcessor.GetValueAsync<string>(Arg.Any<Argument>()).Returns("https://www.bing.com");
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            var webDriver = Substitute.For<IWebDriver>();
            WebApplication webAppliction = new WebApplication()
            {
                WebDriver = webDriver
            };
            entityManager.GetOwnerApplication<WebApplication>(Arg.Any<IComponent>()).Returns(webAppliction);
                       

            var navigateActor = new GotoActorComponent()
            {
                EntityManager = entityManager                
            };
            await navigateActor.ActAsync();

            await argumentProcessor.Received(1).GetValueAsync<string>(Arg.Any<Argument>());
            webDriver.Received(1).Navigate();    
        }
    }
}
