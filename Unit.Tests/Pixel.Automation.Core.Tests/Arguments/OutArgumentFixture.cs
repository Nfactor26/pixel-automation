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
            Assert.IsTrue(outArgument.IsConfigured());

            var clone = outArgument.Clone() as OutArgument<Person>;
            Assert.AreEqual(clone, outArgument);
        }

        [Test]
        public void ValidateThatOutArgumentCanBeInitializedInScriptedMode()
        {
            var outArgument = new OutArgument<int>() { Mode = ArgumentMode.Scripted, ScriptFile = "Script.csx" };
            Assert.AreEqual(ArgumentMode.Scripted, outArgument.Mode);
            Assert.AreEqual("Script.csx", outArgument.ScriptFile);
            Assert.AreEqual(typeof(int), outArgument.GetArgumentType());
            Assert.IsTrue(outArgument.IsConfigured());

            //Argument's can't refer duplciate script at the moment. Hence, cloning doesn't assign same script file and clones are not equal
            var clone = outArgument.Clone() as OutArgument<int>;
            Assert.AreNotEqual(clone, outArgument);
        }
    }
}
