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
            Assert.IsFalse(outArgument.IsConfigured());
        }

        [Test]
        public void ValidateThatOutArgumentCanBeInitializedInDataBoundMode()
        {
            var outArgument = new OutArgument<Person>() { PropertyPath = "Age" };
            Assert.AreEqual(ArgumentMode.DataBound, outArgument.Mode);
            Assert.AreEqual("Age", outArgument.PropertyPath);
            Assert.AreEqual(typeof(Person), outArgument.GetArgumentType());
            Assert.IsFalse(outArgument.CanChangeType);
            Assert.IsTrue(outArgument.CanChangeMode);
            Assert.IsTrue(outArgument.IsConfigured());
            Assert.IsFalse(outArgument.AllowedModes.HasFlag(ArgumentMode.Default));
            Assert.IsTrue(outArgument.AllowedModes.HasFlag(ArgumentMode.DataBound));
            Assert.IsTrue(outArgument.AllowedModes.HasFlag(ArgumentMode.Scripted));
        }

        [Test]
        public void ValidateThatOutArgumentCanBeInitializedInScriptedMode()
        {
            var outArgument = new OutArgument<int>() { Mode = ArgumentMode.Scripted, ScriptFile = "Script.csx" };
            Assert.AreEqual(ArgumentMode.Scripted, outArgument.Mode);
            Assert.AreEqual("Script.csx", outArgument.ScriptFile);
            Assert.AreEqual(typeof(int), outArgument.GetArgumentType());
            Assert.IsTrue(outArgument.IsConfigured());
        }


        [Test]
        public void ValidateThatOutArgumentCanBeCloned()
        {
            var outArgument = new OutArgument<Person>() { PropertyPath = "Age", ScriptFile = "ArgumentScript.csx" };           
            var clone = outArgument.Clone() as OutArgument<Person>;

            Assert.AreEqual(outArgument.Mode, clone.Mode);
            Assert.AreEqual(outArgument.PropertyPath, clone.PropertyPath);        
            Assert.AreEqual(outArgument.GetArgumentType(), clone.GetArgumentType());
            Assert.AreEqual(outArgument.ScriptFile, clone.ScriptFile);
            Assert.AreEqual(outArgument.ArgumentType, clone.ArgumentType);
        }
    }
}
