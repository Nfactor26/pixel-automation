using NSubstitute;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions.Internal;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;


namespace Pixel.Automation.Web.Selenium.Components.Tests.ActorComponents
{
    //class DoubleClickActorComponentTests
    //{
    //    [Test]
    //    public void ValidateThatDoubleClickActorCanDoubleClickTargetControl()
    //    {
    //        var entityManager = Substitute.For<IEntityManager>();

    //        IWebDriver webDriver = Substitute.For<IWebDriver, IActionExecutor>();
    //        WebApplication webAppliction = new WebApplication()
    //        {
    //            WebDriver = webDriver
    //        };
    //        entityManager.GetOwnerApplication<WebApplication>(Arg.Any<IComponent>()).Returns(webAppliction);

    //        IWebElement targetControl = Substitute.For<IWebElement, ILocatable>();
    //        (targetControl as ILocatable).LocationOnScreenOnceScrolledIntoView.Returns(new System.Drawing.Point(100, 200));
    //        var coordinates = Substitute.For<ICoordinates>();
    //        (targetControl as ILocatable).Coordinates.Returns(coordinates);

    //        UIControl uiControl = Substitute.For<UIControl>();
    //        uiControl.GetApiControl<IWebElement>().Returns(targetControl);

    //        var controlEntity = Substitute.For<Entity, IControlEntity>();
    //        (controlEntity as IControlEntity).GetControl().Returns(uiControl);

    //        var doubleClickActor = new DoubleClickActorComponent()
    //        {
    //            EntityManager = entityManager,
    //            Parent = controlEntity
    //        };
    //        doubleClickActor.Act();

    //        _ = (targetControl as ILocatable).Received(1).LocationOnScreenOnceScrolledIntoView;
    //    }
    //}
}
