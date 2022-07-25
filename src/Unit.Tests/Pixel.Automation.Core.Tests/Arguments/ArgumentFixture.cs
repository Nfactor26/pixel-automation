using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Test.Helpers;
using System;

namespace Pixel.Automation.Core.Tests.Arguments
{

    class DummyArgument<T> : Argument
    {
        public DummyArgument()
        {
            this.Mode = ArgumentMode.DataBound;
            this.AllowedModes = ArgumentMode.Default | ArgumentMode.DataBound | ArgumentMode.Scripted;
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public override Type GetArgumentType()
        {
            return typeof(T);
        }
    }

    class ArgumentFixture
    {
        [Test]
        public void ValidateThatArugmentCanBeInitializedInDataBoundMode()
        {
            var argument = new DummyArgument<Person>() { PropertyPath = "Age", CanChangeType =false };
            Assert.AreEqual(ArgumentMode.DataBound, argument.Mode);
            Assert.IsTrue(argument.CanChangeMode);
            Assert.IsFalse(argument.CanChangeType);
            Assert.IsTrue(string.IsNullOrEmpty(argument.ScriptFile));
            Assert.AreEqual("Age", argument.PropertyPath);
            Assert.AreEqual("Person", argument.ArgumentType);
            Assert.IsTrue(argument.IsConfigured());
            Assert.IsTrue(argument.AllowedModes.HasFlag(ArgumentMode.Default));
            Assert.IsTrue(argument.AllowedModes.HasFlag(ArgumentMode.DataBound));
            Assert.IsTrue(argument.AllowedModes.HasFlag(ArgumentMode.Scripted));

        }

        [Test]
        public void ValidateThatArugmentCanBeInitializedInScrpitedMode()
        {
            var argument = new DummyArgument<int>() { Mode = ArgumentMode.Scripted, ScriptFile = "Script.csx" };
            Assert.AreEqual(ArgumentMode.Scripted, argument.Mode);
            Assert.IsTrue(argument.CanChangeMode);
            Assert.IsFalse(argument.CanChangeType);
            Assert.AreEqual("Script.csx", argument.ScriptFile);
            Assert.IsTrue(string.IsNullOrEmpty(argument.PropertyPath));
            Assert.AreEqual("Int32", argument.ArgumentType);
            Assert.IsTrue(argument.IsConfigured());
        }

        [Test]
        public void ValidateThatOnlyAllowedModesCanBeSetAsArgumentMode()
        {
            var argument = new DummyArgument<int>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.Scripted, ScriptFile = "Script.csx" };
           
            Assert.IsFalse(argument.AllowedModes.HasFlag(ArgumentMode.Default));
            Assert.IsTrue(argument.AllowedModes.HasFlag(ArgumentMode.DataBound));
            Assert.IsTrue(argument.AllowedModes.HasFlag(ArgumentMode.Scripted));
            Assert.AreEqual(ArgumentMode.Scripted, argument.Mode);

            argument.Mode = ArgumentMode.Default;
       
            Assert.AreNotEqual(ArgumentMode.Default, argument.Mode);
            Assert.AreEqual(ArgumentMode.Scripted, argument.Mode);

        }
    }
}
