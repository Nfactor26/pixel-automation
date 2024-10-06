using InputSimulatorStandard;
using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Native.Windows.Device;
using MouseButton = Pixel.Automation.Core.Devices.MouseButton;

namespace Pixel.Automation.Nativew.Windows.Tests.Device
{
    class SyntheticMouseFixture
    {
        private readonly SyntheticMouse syntheticMouse;
        private readonly IMouseSimulator mouseSimulator;
   
        public SyntheticMouseFixture()
        {
            this.mouseSimulator = Substitute.For<IMouseSimulator>();
            this.syntheticMouse = new SyntheticMouse(mouseSimulator);
        }

        [TearDown]
        public void TearDown()
        {
            this.mouseSimulator.ClearReceivedCalls();
        }

        [Test]
        public void ValidateThatSyntheticMouseCanBeInitialized()
        {
            var syntheticMouse = new SyntheticMouse();
            Assert.That(syntheticMouse is not null);
        }

        [Test]
        public void ValidateThatMouseWheelTickSizeCanBeChanged()
        {
            syntheticMouse.MouseWheelClickSize = 10;
            Assert.That(syntheticMouse.MouseWheelClickSize, Is.EqualTo(10));
        }

        [Test]
        public void VerifyThatSyntheticMouseCanDoLeftMouseDown()
        {        
            syntheticMouse.ButtonDown(MouseButton.LeftButton);
            mouseSimulator.Received(1).LeftButtonDown();
        }

        [Test]
        public void VerifyThatSyntheticMouseCanDoRightMouseDown()
        {          
            syntheticMouse.ButtonDown(MouseButton.RightButton);
            mouseSimulator.Received(1).RightButtonDown();
        }

        [Test]
        public void VerifyThatSyntheticMouseCanDoMiddleMouseDown()
        {
            syntheticMouse.ButtonDown(MouseButton.MiddleButton);
            mouseSimulator.Received(1).MiddleButtonDown();
        }

        [Test]
        public void VerifyThatSyntheticMouseCanDoLeftMouseUp()
        {           
            syntheticMouse.ButtonUp(MouseButton.LeftButton);
            mouseSimulator.Received(1).LeftButtonUp();
        }


        [Test]
        public void VerifyThatSyntheticMouseCanDoRightMouseUp()
        { 
            syntheticMouse.ButtonUp(MouseButton.RightButton);
            mouseSimulator.Received(1).RightButtonUp();
        }


        [Test]
        public void VerifyThatSyntheticMouseCanDoMiddleMouseUp()
        {
            syntheticMouse.ButtonUp(MouseButton.MiddleButton);
            mouseSimulator.Received(1).MiddleButtonUp();
        }


        [Test]
        public void VerifyThatSyntheticMouseCanDoLeftClick()
        {
            syntheticMouse.Click(MouseButton.LeftButton);
            mouseSimulator.Received(1).LeftButtonClick();
        }


        [Test]
        public void VerifyThatSyntheticMouseCanDoRightClick()
        {           
            syntheticMouse.Click(MouseButton.RightButton);
            mouseSimulator.Received(1).RightButtonClick();
        }


        [Test]
        public void VerifyThatSyntheticMouseCanDoMiddleClick()
        {
            syntheticMouse.Click(MouseButton.MiddleButton);
            mouseSimulator.Received(1).MiddleButtonClick();
        }


        [Test]
        public void VerifyThatSyntheticMouseCanDoLeftDoubleClick()
        {
            var mouseSimulator = Substitute.For<IMouseSimulator>();
            var syntheticMouse = new SyntheticMouse(mouseSimulator);
            syntheticMouse.DoubleClick(MouseButton.LeftButton);
            mouseSimulator.Received(1).LeftButtonDoubleClick();
        }


        [Test]
        public void VerifyThatSyntheticMouseCanDoRightDoubleClick()
        {
            syntheticMouse.DoubleClick(MouseButton.RightButton);
            mouseSimulator.Received(1).RightButtonDoubleClick();
        }


        [Test]
        public void VerifyThatSyntheticMouseCanDoMiddleDoubleClick()
        {          
            syntheticMouse.DoubleClick(MouseButton.MiddleButton);
            mouseSimulator.Received(1).MiddleButtonDoubleClick();
        }

        [Test]
        public void VerifyThatSyntheticMouseCanMoveMouseByGivenAmount()
        {           
            syntheticMouse.MoveMouseBy(10, 10, Core.Devices.SmoothMode.None);
            mouseSimulator.Received(1).MoveMouseTo(Arg.Any<double>(), Arg.Any<double>());
        }

        [Test]
        public void VerifyThatSyntheticMouseCanMoveMouseToAGivenCoordinate()
        {
            syntheticMouse.MoveMouseTo(new Core.Devices.ScreenCoordinate(100, 100), Core.Devices.SmoothMode.Interpolated);
            //There will be multiple calls to move mouse since we are using interpolated smooth mode
            mouseSimulator.Received().MoveMouseTo(Arg.Any<double>(), Arg.Any<double>());
        }

        [Test]
        public void VerifyThatSyntheticMouseCanDoHorizontalScroll()
        {
            syntheticMouse.HorizontalScroll(5);
            mouseSimulator.Received(1).HorizontalScroll(Arg.Is<int>(5));
        }

        [Test]
        public void VerifyThatSynthethicMouseCanDoVerticalScroll()
        {
            syntheticMouse.VerticalScroll(5);
            mouseSimulator.Received(1).VerticalScroll(Arg.Is<int>(5));
        }
    }
}
