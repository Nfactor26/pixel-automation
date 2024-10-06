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

            Assert.That(funcArgument.Mode, Is.EqualTo(ArgumentMode.Scripted));
            Assert.That(funcArgument.ScriptFile, Is.EqualTo("FuncScript.csx"));
            Assert.That(funcArgument.CanChangeType == false);
            Assert.That(funcArgument.CanChangeMode == false);
            Assert.That(funcArgument.GetArgumentType(), Is.EqualTo(typeof(int)));
            Assert.That(funcArgument.AllowedModes.HasFlag(ArgumentMode.Default) == false);
            Assert.That(funcArgument.AllowedModes.HasFlag(ArgumentMode.DataBound) == false);
            Assert.That(funcArgument.AllowedModes.HasFlag(ArgumentMode.Scripted));
        }

        [Test]
        public void ValidateThatFuncArgumentCanBeCloned()
        {
            var funcArgument = new FuncArgument<int>() { ScriptFile = "FuncScript.csx" };
            var clone = funcArgument.Clone() as FuncArgument<int>;

            Assert.That(clone.Mode, Is.EqualTo(funcArgument.Mode));
            Assert.That(clone.PropertyPath, Is.EqualTo(funcArgument.PropertyPath));
            Assert.That(clone.GetArgumentType(), Is.EqualTo(funcArgument.GetArgumentType()));
            Assert.That(clone.ScriptFile, Is.EqualTo(funcArgument.ScriptFile));
            Assert.That(clone.ArgumentType, Is.EqualTo(funcArgument.ArgumentType));
        }
    }
}
