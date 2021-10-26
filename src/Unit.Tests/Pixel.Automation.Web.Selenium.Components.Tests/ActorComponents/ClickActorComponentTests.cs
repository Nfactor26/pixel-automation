using NSubstitute;
using NUnit.Framework;
using OpenQA.Selenium;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;


namespace Pixel.Automation.Web.Selenium.Components.Tests.ActorComponents
{
    class ClickActorComponentTests
    {
        /// <summary>
        /// Validate that Click actor component can perform click on a web element
        /// </summary>
        [Test]
        public void ValidateThatClickActorCanClickTargetcontrol()
        {
            var entityManager = Substitute.For<IEntityManager>();

            IWebElement targetControl = Substitute.For<IWebElement>();
            UIControl uiControl = Substitute.For<UIControl>();
            uiControl.GetApiControl<IWebElement>().Returns(targetControl);
           
            var controlEntity = Substitute.For<Entity, IControlEntity>();
            (controlEntity as IControlEntity).GetControl().Returns(uiControl);        

            var clickActor = new ClickActorComponent()
            {
                EntityManager = entityManager,
                Parent = controlEntity,
                ForceClick = false
            };
            clickActor.Act();           
            targetControl.Received(1).Click();
        }

        /// <summary>
        /// Validate that Click actor component can force click on a web element.
        /// </summary>
        //[Test]
        //public void ValidateThatClickActorCanForceClickTargetcontrol()
        //{
        //    var entityManager = Substitute.For<IEntityManager>();

        //    IWebDriver webDriver = Substitute.For<IWebDriver, IActionExecutor>();
        //    WebApplication webAppliction = new WebApplication()
        //    {
        //        WebDriver = webDriver
        //    };
        //    entityManager.GetOwnerApplication<WebApplication>(Arg.Any<IComponent>()).Returns(webAppliction);

        //    IWebElement targetControl = Substitute.For<IWebElement, ILocatable>();
        //    (targetControl as ILocatable).LocationOnScreenOnceScrolledIntoView.Returns(new System.Drawing.Point(100, 200));
        //    UIControl uiControl = Substitute.For<UIControl>();
        //    uiControl.GetApiControl<IWebElement>().Returns(targetControl);

        //    var controlEntity = Substitute.For<Entity, IControlEntity>();
        //    (controlEntity as IControlEntity).GetControl().Returns(uiControl);

        //    var clickActor = new ClickActorComponent()
        //    {
        //        EntityManager = entityManager,
        //        Parent = controlEntity,
        //        ForceClick = true
        //    };
        //    clickActor.Act();
        //    targetControl.DidNotReceive().Click();
        //}

    }
}
