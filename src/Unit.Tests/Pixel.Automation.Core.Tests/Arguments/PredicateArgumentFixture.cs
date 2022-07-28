using NUnit.Framework;
using Pixel.Automation.Core.Arguments;

namespace Pixel.Automation.Core.Tests.Arguments
{
    class PredicateArgumentFixture
    {
        [Test]
        public void ValidateThatPredicateArugmentCanBeInitialized()
        {
            var predicateArgument = new PredicateArgument<int>() { ScriptFile = "PredicateScript.csx" };
            
            Assert.AreEqual(ArgumentMode.Scripted, predicateArgument.Mode);
            Assert.AreEqual("PredicateScript.csx", predicateArgument.ScriptFile);
            Assert.IsFalse(predicateArgument.CanChangeType);
            Assert.IsFalse(predicateArgument.CanChangeMode);
            Assert.AreEqual(typeof(int), predicateArgument.GetArgumentType());
            Assert.IsFalse(predicateArgument.AllowedModes.HasFlag(ArgumentMode.Default));
            Assert.IsFalse(predicateArgument.AllowedModes.HasFlag(ArgumentMode.DataBound));
            Assert.IsTrue(predicateArgument.AllowedModes.HasFlag(ArgumentMode.Scripted));
        }

        [Test]
        public void ValidateThatPredicateArgumentCanBeCloned()
        {
            var predicateArgument = new PredicateArgument<int>() { ScriptFile = "PredicateScript.csx" };
            var clone = predicateArgument.Clone() as PredicateArgument<int>;

            Assert.AreEqual(predicateArgument.Mode, clone.Mode);
            Assert.AreEqual(predicateArgument.PropertyPath, clone.PropertyPath);
            Assert.AreEqual(predicateArgument.GetArgumentType(), clone.GetArgumentType());
            Assert.AreEqual(predicateArgument.ScriptFile, clone.ScriptFile);
            Assert.AreEqual(predicateArgument.ArgumentType, clone.ArgumentType);
        }
    }
}
