using NSubstitute;
using NUnit.Framework;
using OpenQA.Selenium;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using System.Threading.Tasks;

namespace Pixel.Automation.Web.Selenium.Components.Tests
{
    class WebUIControlTests
    {
        private WebUIControl webUIControl;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var controlIdentity = Substitute.For<IControlIdentity>();
            controlIdentity.PivotPoint.Returns(Core.Enums.Pivots.Center);
            controlIdentity.XOffSet.Returns(10);
            controlIdentity.YOffSet.Returns(0);
            controlIdentity.Next = null;

            var coordinateProvider = Substitute.For<ICoordinateProvider>();
            coordinateProvider.GetBoundingBox(Arg.Any<IWebElement>()).Returns(new BoundingBox(0, 0, 100, 100));

            var webElement = Substitute.For<IWebElement>();

            webUIControl = new WebUIControl(controlIdentity, webElement,  coordinateProvider);
        }

        [Test]
        public async Task ValidateThatWebUIControlCanProvideBoundingBoxOfUnderlyingControl()
        {
            var boundingBox = await webUIControl.GetBoundingBoxAsync();

            Assert.That(boundingBox, Is.EqualTo(new BoundingBox(0, 0, 100, 100)));
        }

        [Test]
        public async Task ValidateThatWebUIControlCanProvideClickablePointOnUnderlyingControl()
        {
            var (x, y) = await webUIControl.GetClickablePointAsync();

            //since pivot mode is center and x offset is 10 
            Assert.That(x, Is.EqualTo(60));
            Assert.That(y, Is.EqualTo(50));
        }
    }
}
