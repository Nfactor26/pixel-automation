using NSubstitute;
using NUnit.Framework;
using OpenQA.Selenium;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Selenium.Components.Tests;

[TestFixture]
public class WebControlEntityFixture
{
    [TestCase(true, 0)]
    [TestCase(false, 1)]
    public async Task ValidateThatControlIsRetrievedFromCacheIfCachingIsEnabled(bool enableCaching, int expectedCallsForSecondAccess)
    {
        var control = new WebUIControl(Substitute.For<IControlIdentity>(), Substitute.For<IWebElement>(), Substitute.For<ICoordinateProvider>());   
        var controlLoader = Substitute.For<IControlLoader>();
        controlLoader.GetControl(Arg.Any<string>(), Arg.Any<string>()).Returns(new ControlDescription() { ControlDetails = Substitute.For<IControlIdentity>() });
        var controlLocator = Substitute.For<IControlLocator>();
        controlLocator.FindControlAsync(Arg.Any<IControlIdentity>(), Arg.Any<UIControl>()).Returns(control);
        var entityManager = Substitute.For<IEntityManager>();
        entityManager.GetServiceOfType<IControlLoader>().Returns(controlLoader);
        entityManager.GetControlLocator(Arg.Any<IControlIdentity>()).Returns(controlLocator);
        var webControlEntity = new WebControlEntity()
        {
            EntityManager = entityManager,
            LookupMode = Core.Enums.LookupMode.FindSingle,
            CacheControl = enableCaching,
            SearchRoot = new InArgument<UIControl>()
        };      

        //On first request to GetControl, control should be cached if caching is enabled
        var foundControl = await webControlEntity.GetControl();
        await webControlEntity.OnCompletionAsync();
        Assert.That(foundControl, Is.EqualTo(control));       
        await controlLocator.Received(1).FindControlAsync(Arg.Any<IControlIdentity>(), Arg.Any<UIControl>());

        controlLocator.ClearReceivedCalls();

        //On second request, control should be returned from cache if caching is enabled
        var cachedControl = await webControlEntity.GetControl();
        Assert.That(cachedControl, Is.EqualTo(control));
        await controlLocator.Received(expectedCallsForSecondAccess).FindControlAsync(Arg.Any<IControlIdentity>(), Arg.Any<UIControl>());
        await webControlEntity.OnCompletionAsync();
    }

}
