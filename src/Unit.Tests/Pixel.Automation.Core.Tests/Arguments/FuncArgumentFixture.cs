using NUnit.Framework;
using Pixel.Automation.Core.Arguments;

namespace Pixel.Automation.Core.Tests.Arguments
{

    class FuncArgumentFixture
    {
        [Test]
        public void ValidateThatFuncArugmentCanBeInitialized()
        {
            var funcArgument = new FuncArgument<int>() { ScriptFile = "FuncScript.csx" };

            Assert.AreEqual(ArgumentMode.Scripted, funcArgument.Mode);
            Assert.AreEqual("FuncScript.csx", funcArgument.ScriptFile);
            Assert.IsFalse(funcArgument.CanChangeType);
            Assert.IsFalse(funcArgument.CanChangeMode);
            Assert.AreEqual(typeof(int), funcArgument.GetArgumentType());
            Assert.IsFalse(funcArgument.AllowedModes.HasFlag(ArgumentMode.Default));
            Assert.IsFalse(funcArgument.AllowedModes.HasFlag(ArgumentMode.DataBound));
            Assert.IsTrue(funcArgument.AllowedModes.HasFlag(ArgumentMode.Scripted));
        }

        [Test]
        public void ValidateThatFuncArgumentCanBeCloned()
        {
            var funcArgument = new FuncArgument<int>() { ScriptFile = "FuncScript.csx" };
            var clone = funcArgument.Clone() as FuncArgument<int>;

            Assert.AreEqual(funcArgument.Mode, clone.Mode);
            Assert.AreEqual(funcArgument.PropertyPath, clone.PropertyPath);
            Assert.AreEqual(funcArgument.GetArgumentType(), clone.GetArgumentType());
            Assert.AreEqual(funcArgument.ScriptFile, clone.ScriptFile);
            Assert.AreEqual(funcArgument.ArgumentType, clone.ArgumentType);
        }
    }
}
