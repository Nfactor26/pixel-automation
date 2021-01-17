using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Devices;
using Pixel.Automation.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Pixel.Automation.Input.Devices.Tests
{
    public class HotKeyActorComponentTests
    {       
        /// <summary>
        /// Validate that HotKeyActor can press configured keys properly
        /// </summary>
        [Test]
        public void ValidateThatHotKeyActorCanPressConfiguredHotKeys()
        {
            var entityManager = Substitute.For<IEntityManager>();
          
            var syntheticKeyboard = Substitute.For<ISyntheticKeyboard>();          
            syntheticKeyboard.GetSynthethicKeyCodes(Arg.Is<string>("Ctrl + C")).Returns(new[] { SyntheticKeyCode.LCONTROL, SyntheticKeyCode.VK_C });
            syntheticKeyboard.IsModifierKey(Arg.Is<SyntheticKeyCode>(SyntheticKeyCode.LCONTROL)).Returns(true);
            syntheticKeyboard.IsModifierKey(Arg.Is<SyntheticKeyCode>(SyntheticKeyCode.VK_C)).Returns(false);
            syntheticKeyboard.When(x => x.ModifiedKeyStroke(Arg.Any<IEnumerable<SyntheticKeyCode>>(), Arg.Any<IEnumerable<SyntheticKeyCode>>()))
                .Do((x) =>
                {
                    //force iteration of modifiers and keys so that we can assert there were 4 calls to IsModifierKey() 
                    var modifiers = x.ArgAt<IEnumerable<SyntheticKeyCode>>(0);
                    modifiers.ToList();

                    var keys = x.ArgAt<IEnumerable<SyntheticKeyCode>>(1);
                    keys.ToList();
                });

            entityManager.GetServiceOfType<ISyntheticKeyboard>().Returns(syntheticKeyboard);

            var hotKeyActor = new HotKeyActorComponent()
            {
                KeySequence = "Ctrl + C",
                EntityManager = entityManager
            };
            hotKeyActor.Act();

            syntheticKeyboard.Received(1).GetSynthethicKeyCodes(Arg.Is<string>("Ctrl + C"));
            syntheticKeyboard.Received(4).IsModifierKey(Arg.Any<SyntheticKeyCode>()); // twice to take and twice to skip
            syntheticKeyboard.Received(1).ModifiedKeyStroke(Arg.Any<IEnumerable<SyntheticKeyCode>>(), Arg.Any<IEnumerable<SyntheticKeyCode>>());
        }


        /// <summary>
        /// Validate that HotKeyActor ValidateComponent() can correctly report if KeySequence is configured.
        /// </summary>
        /// <param name="keySequence"></param>
        /// <param name="expectedResult"></param>
        [TestCase("Ctrl + C", true)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void ValidateThatHotKeyActorCanReportIfConfiguredCorrectly(string keySequence, bool expectedResult)
        {
            var hotKeyActor = new HotKeyActorComponent()
            {
                KeySequence =  keySequence
            };
            bool isValid = hotKeyActor.ValidateComponent();
            Assert.AreEqual(expectedResult, isValid);
        }
    }
}