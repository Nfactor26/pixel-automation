using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;

namespace Pixel.Automation.Core.Tests
{
    public class ControlClickablePointTests
    {

        [TestCase(Pivots.BottomLeft, 10, 110)]
        [TestCase(Pivots.BottomRight, 110, 110)]
        [TestCase(Pivots.Center, 60, 60)]
        [TestCase(Pivots.TopLeft, 10, 10)]
        [TestCase(Pivots.TopRight, 110, 10)]
        public void VerifyThatCorrectClicakblePointIsCalculated(Pivots pivotPoint, double expectedX, double expectedY)
        {
            var controlIdentity = Substitute.For<IControlIdentity>();
            controlIdentity.PivotPoint.Returns(pivotPoint);
            controlIdentity.XOffSet.Returns(10);
            controlIdentity.YOffSet.Returns(10);
            controlIdentity.Next.Returns((IControlIdentity)null);

            controlIdentity.GetClickablePoint(new System.Drawing.Rectangle(0, 0, 100, 100), out double x, out double y);
         
            Assert.AreEqual(expectedX, x);
            Assert.AreEqual(expectedY, y);
        }
    }
}
