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
            argumentProcessor.GetValueAsync<Uri>(Arg.Any<Argument>()).Returns(new Uri("https://www.bing.com"));
            entityManager.GetArgumentProcessor().Returns(argumentProcessor);

            var webDriver = Substitute.For<IWebDriver>();
            WebApplication webAppliction = new WebApplication()
            {
                WebDriver = webDriver
            };
            entityManager.GetOwnerApplication<WebApplication>(Arg.Any<IComponent>()).Returns(webAppliction);
                       

            var navigateActor = new NavigateActorComponent()
            {
                EntityManager = entityManager                
            };
            await navigateActor.ActAsync();

            await argumentProcessor.Received(1).GetValueAsync<Uri>(Arg.Any<Argument>());
            webDriver.Received(1).Navigate();    
        }
    }
}
