using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Interfaces;
using System.Collections.Generic;


namespace Pixel.Automation.Input.Devices.Tests
{
    class KyePressActorComponentTests
    {
        [Test]
        public void ValidateThatKeyPressActorCanKeyPressConfiguredKeys()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var syntheticKeyboard = Substitute.For<ISyntheticKeyboard>();
            syntheticKeyboard.GetSynthethicKeyCodes(Arg.Is<string>("C + A + K + E")).Returns(new[] { SyntheticKeyCode.VK_C, SyntheticKeyCode.VK_A, SyntheticKeyCode.VK_K, SyntheticKeyCode.VK_E });       

            entityManager.GetServiceOfType<ISyntheticKeyboard>().Returns(syntheticKeyboard);

            var keypressActor = new KeyPressActorComponent()
            {
                KeyPressMode = PressMode.KeyPress,
                KeySequence = "C + A + K + E",
                EntityManager = entityManager
            };
            keypressActor.Act();

            syntheticKeyboard.Received(1).GetSynthethicKeyCodes(Arg.Is<string>("C + A + K + E"));           
            syntheticKeyboard.Received(4).KeyPress(Arg.Any<SyntheticKeyCode>());
        }

        [Test]
        public void ValidateThatKeyPressActorCanKeyDownConfiguredKeys()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var syntheticKeyboard = Substitute.For<ISyntheticKeyboard>();
            syntheticKeyboard.GetSynthethicKeyCodes(Arg.Is<string>("C + A + K + E")).Returns(new[] { SyntheticKeyCode.VK_C, SyntheticKeyCode.VK_A, SyntheticKeyCode.VK_K, SyntheticKeyCode.VK_E });

            entityManager.GetServiceOfType<ISyntheticKeyboard>().Returns(syntheticKeyboard);

            var keypressActor = new KeyPressActorComponent()
            {
                KeyPressMode = PressMode.KeyDown,
                KeySequence = "C + A + K + E",
                EntityManager = entityManager
            };
            keypressActor.Act();

            syntheticKeyboard.Received(1).GetSynthethicKeyCodes(Arg.Is<string>("C + A + K + E"));
            syntheticKeyboard.Received(4).KeyDown(Arg.Any<SyntheticKeyCode>());
        }


        [Test]
        public void ValidateThatKeyPressActorCanKeyUpConfiguredKeys()
        {
            var entityManager = Substitute.For<IEntityManager>();

            var syntheticKeyboard = Substitute.For<ISyntheticKeyboard>();
            syntheticKeyboard.GetSynthethicKeyCodes(Arg.Is<string>("C + A + K + E")).Returns(new[] { SyntheticKeyCode.VK_C, SyntheticKeyCode.VK_A, SyntheticKeyCode.VK_K, SyntheticKeyCode.VK_E });

            entityManager.GetServiceOfType<ISyntheticKeyboard>().Returns(syntheticKeyboard);

            var keypressActor = new KeyPressActorComponent()
            {
                KeyPressMode = PressMode.KeyUp,
                KeySequence = "C + A + K + E",
                EntityManager = entityManager
            };
            keypressActor.Act();

            syntheticKeyboard.Received(1).GetSynthethicKeyCodes(Arg.Is<string>("C + A + K + E"));
            syntheticKeyboard.Received(4).KeyUp(Arg.Any<SyntheticKeyCode>());
        }
    }
}
