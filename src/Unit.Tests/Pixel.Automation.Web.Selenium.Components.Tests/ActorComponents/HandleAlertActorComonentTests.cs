using NSubstitute;
using NUnit.Framework;
using OpenQA.Selenium;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Web.Selenium.Components.Alerts;

namespace Pixel.Automation.Web.Selenium.Components.Tests.ActorComponents
{
    class HandleAlertActorComonentTests
    {
        [TestCase(HandleAlertBehavior.Accept)]
        [TestCase(HandleAlertBehavior.Dismiss)]
        public void ValidateThatHandleAlertActorCanHandleAlerts(HandleAlertBehavior alertBehavior)
        {
            var entityManager = Substitute.For<IEntityManager>();
          
            var webDriver = Substitute.For<IWebDriver>();
            var targetLocator = Substitute.For<ITargetLocator>();
            var alert = Substitute.For<IAlert>();
            targetLocator.Alert().Returns(alert);
            webDriver.SwitchTo().Returns(targetLocator);
            WebApplication webAppliction = new WebApplication()
            {
                WebDriver = webDriver
            };
            entityManager.GetOwnerApplication<WebApplication>(Arg.Any<IComponent>()).Returns(webAppliction);

            var handleAlertActor = new HandleAlertActorComponent()
            {
                EntityManager = entityManager,
                Action = alertBehavior
            };

            handleAlertActor.Act();

            switch(alertBehavior)
            {
                case HandleAlertBehavior.Accept:
                    alert.Received(1).Accept();
                    alert.DidNotReceive().Dismiss();
                    break;
                case HandleAlertBehavior.Dismiss:
                    alert.Received(1).Dismiss();
                    alert.DidNotReceive().Accept();
                    break;
            }
        }
    }
}
