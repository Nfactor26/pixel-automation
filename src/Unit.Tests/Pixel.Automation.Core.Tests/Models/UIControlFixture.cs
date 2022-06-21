using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Tests.Models
{

    public interface IDummyControl
    {

    }

    public class DummyUIControl : UIControl
    {
        public DummyUIControl(IDummyControl underlyingControl)
        {
            this.TargetControl = underlyingControl;
        }

        public override async Task<Rectangle> GetBoundingBoxAsync()
        {
            return await Task.FromResult(Rectangle.Empty);
        }

        public override async Task<(double,double)> GetClickablePointAsync()
        {
            return await Task.FromResult((0, 0));
        }
    }

    class UIControlFixture
    {
        [Test]
        public void ValidateThatUIControlCanBeInitialized()
        {
            var underlyingControl = Substitute.For<IDummyControl>();
            var uiControl = new DummyUIControl(underlyingControl);

            var apiControl = uiControl.GetApiControl<IDummyControl>();
            Assert.AreSame(underlyingControl, apiControl);
        }

        [Test]
        public void ValidateThatGetAPIControlThrowsExceptionIfUnderlyingControlDoesNotMatchRequestedType()
        {
            var underlyingControl = Substitute.For<IDummyControl>();
            var uiControl = new DummyUIControl(underlyingControl);

           Assert.Throws<InvalidCastException>(() => uiControl.GetApiControl<IControlIdentity>());
            
        }
    }
}
