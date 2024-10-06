using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Test.Helpers;

namespace Pixel.Automation.Core.Tests.Arguments
{
    class OutArgumentFixture
    {
        /// <summary>
        /// Although, OutArgument can be intialized in Default mode, however, Default is not a valid mode for OutArgument.
        /// Trying to call IsConfigured() will always return false.
        /// </summary>
        [Test]
        public void ValidateThatOutArgumentIsConfiguredIsFalseInDefaultMode()
        {
            var outArgument = new OutArgument<int>() { Mode = ArgumentMode.Default };
            Assert.That(outArgument.IsConfigured() == false);
        }

        [Test]
        public void ValidateThatOutArgumentCanBeInitializedInDataBoundMode()
        {
            var outArgument = new OutArgument<Person>() { PropertyPath = "Age" };
            Assert.That(outArgument.Mode, Is.EqualTo(ArgumentMode.DataBound));
            Assert.That(outArgument.PropertyPath, Is.EqualTo("Age"));
            Assert.That(outArgument.GetArgumentType(), Is.EqualTo(typeof(Person)));
            Assert.That(outArgument.CanChangeType == false);
            Assert.That(outArgument.CanChangeMode);
            Assert.That(outArgument.IsConfigured());
            Assert.That(outArgument.AllowedModes.HasFlag(ArgumentMode.Default) == false);
            Assert.That(outArgument.AllowedModes.HasFlag(ArgumentMode.DataBound));
            Assert.That(outArgument.AllowedModes.HasFlag(ArgumentMode.Scripted));
        }

        [Test]
        public void ValidateThatOutArgumentCanBeInitializedInScriptedMode()
        {
            var outArgument = new OutArgument<int>() { Mode = ArgumentMode.Scripted, ScriptFile = "Script.csx" };
            Assert.That(outArgument.Mode, Is.EqualTo(ArgumentMode.Scripted));
            Assert.That(outArgument.ScriptFile, Is.EqualTo("Script.csx"));
            Assert.That(outArgument.GetArgumentType(), Is.EqualTo(typeof(int)));
            Assert.That(outArgument.IsConfigured());
        }


        [Test]
        public void ValidateThatOutArgumentCanBeCloned()
        {
            var outArgument = new OutArgument<Person>() { PropertyPath = "Age", ScriptFile = "ArgumentScript.csx" };           
            var clone = outArgument.Clone() as OutArgument<Person>;

            Assert.That(clone.Mode, Is.EqualTo(outArgument.Mode));
            Assert.That(clone.PropertyPath, Is.EqualTo(outArgument.PropertyPath));        
            Assert.That(clone.GetArgumentType(), Is.EqualTo(outArgument.GetArgumentType()));
            Assert.That(clone.ScriptFile, Is.EqualTo(outArgument.ScriptFile));
            Assert.That(clone.ArgumentType, Is.EqualTo(outArgument.ArgumentType));
        }
    }
}
