using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Automation.Input.Devices.Tests
{
    class KeyPressActorComponentTests
    {
        [Test]
        public async Task ValidateThatKeyPressActorCanKeyPressConfiguredKeys()
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
            await keypressActor.ActAsync();

            syntheticKeyboard.Received(1).GetSynthethicKeyCodes(Arg.Is<string>("C + A + K + E"));           
            syntheticKeyboard.Received(4).KeyPress(Arg.Any<SyntheticKeyCode>());
        }

        [Test]
        public async Task ValidateThatKeyPressActorCanKeyDownConfiguredKeys()
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
            await keypressActor.ActAsync();

            syntheticKeyboard.Received(1).GetSynthethicKeyCodes(Arg.Is<string>("C + A + K + E"));
            syntheticKeyboard.Received(4).KeyDown(Arg.Any<SyntheticKeyCode>());
        }


        [Test]
        public async Task ValidateThatKeyPressActorCanKeyUpConfiguredKeys()
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
            await keypressActor.ActAsync();

            syntheticKeyboard.Received(1).GetSynthethicKeyCodes(Arg.Is<string>("C + A + K + E"));
            syntheticKeyboard.Received(4).KeyUp(Arg.Any<SyntheticKeyCode>());
        }


        /// <summary>
        /// Validate that KeyPressActor ValidateComponent() can report if KeySequence is configured.
        /// </summary>
        /// <param name="keySequence"></param>
        /// <param name="expectedResult"></param>
        [TestCase("Enter", true)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void ValidateThatKeyPressActorCanReportIfKyeSequenceIsConfigured(string keySequence, bool expectedResult)
        {
            var keyPressActor = new KeyPressActorComponent()
            {
                KeySequence = keySequence
            };
            bool isValid = keyPressActor.ValidateComponent();
            Assert.AreEqual(expectedResult, isValid);
        }
    }
}
