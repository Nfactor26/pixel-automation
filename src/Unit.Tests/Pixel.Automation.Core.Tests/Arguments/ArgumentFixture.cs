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
            Assert.That(argument.Mode, Is.EqualTo(ArgumentMode.DataBound));
            Assert.That(argument.CanChangeMode);
            Assert.That(argument.CanChangeType == false);
            Assert.That(string.IsNullOrEmpty(argument.ScriptFile));
            Assert.That(argument.PropertyPath, Is.EqualTo("Age"));
            Assert.That(argument.ArgumentType, Is.EqualTo("Person"));
            Assert.That(argument.IsConfigured());
            Assert.That(argument.AllowedModes.HasFlag(ArgumentMode.Default));
            Assert.That(argument.AllowedModes.HasFlag(ArgumentMode.DataBound));
            Assert.That(argument.AllowedModes.HasFlag(ArgumentMode.Scripted));

        }

        [Test]
        public void ValidateThatArugmentCanBeInitializedInScrpitedMode()
        {
            var argument = new DummyArgument<int>() { Mode = ArgumentMode.Scripted, ScriptFile = "Script.csx" };
            Assert.That(argument.Mode, Is.EqualTo(ArgumentMode.Scripted));
            Assert.That(argument.CanChangeMode);
            Assert.That(argument.CanChangeType == false);
            Assert.That(argument.ScriptFile, Is.EqualTo("Script.csx"));
            Assert.That(string.IsNullOrEmpty(argument.PropertyPath));
            Assert.That(argument.ArgumentType, Is.EqualTo("Int32"));
            Assert.That(argument.IsConfigured());
        }

        [Test]
        public void ValidateThatOnlyAllowedModesCanBeSetAsArgumentMode()
        {
            var argument = new DummyArgument<int>() { AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.Scripted, ScriptFile = "Script.csx" };
           
            Assert.That(argument.AllowedModes.HasFlag(ArgumentMode.Default) == false);
            Assert.That(argument.AllowedModes.HasFlag(ArgumentMode.DataBound));
            Assert.That(argument.AllowedModes.HasFlag(ArgumentMode.Scripted));
            Assert.That(argument.Mode, Is.EqualTo(ArgumentMode.Scripted));

            argument.Mode = ArgumentMode.Default;
       
            Assert.That(argument.Mode, Is.Not.EqualTo(ArgumentMode.Default));
            Assert.That(argument.Mode, Is.EqualTo(ArgumentMode.Scripted));

        }
    }
}
