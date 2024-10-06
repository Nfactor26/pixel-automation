using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Test.Helpers;

namespace Pixel.Automation.Core.Tests.Arguments
{
    class InArgumentFixture
    {
        [Test]
        public void ValidateThatInArgumentCanBeInitializedWithDefaultMode()
        {
            var inArgument = new InArgument<int>() { DefaultValue = 26 };

            Assert.That(inArgument.Mode, Is.EqualTo(ArgumentMode.Default));
            Assert.That(inArgument.DefaultValue, Is.EqualTo(26));         
            Assert.That(inArgument.IsConfigured());
            Assert.That(inArgument.GetDefaultValue(), Is.EqualTo(26));
            Assert.That(inArgument.CanChangeType == false);
            Assert.That(inArgument.CanChangeMode);
            Assert.That(inArgument.GetArgumentType(), Is.EqualTo(typeof(int)));
            Assert.That(inArgument.AllowedModes.HasFlag(ArgumentMode.Default));
            Assert.That(inArgument.AllowedModes.HasFlag(ArgumentMode.DataBound));
            Assert.That(inArgument.AllowedModes.HasFlag(ArgumentMode.Scripted));
        }

        [Test]
        public void ValidateThatInArgumentCanBeInitializedWithScriptedMode()
        {
            var inArgument = new InArgument<string>() { Mode = ArgumentMode.Scripted, ScriptFile = "Script.csx" };

            Assert.That(inArgument.Mode, Is.EqualTo(ArgumentMode.Scripted));
            Assert.That(inArgument.ScriptFile, Is.EqualTo("Script.csx"));
            Assert.That(inArgument.DefaultValue is null);
            Assert.That(inArgument.GetDefaultValue() is null);
            Assert.That(inArgument.IsConfigured());             
            Assert.That(inArgument.GetArgumentType(), Is.EqualTo(typeof(string)));
            Assert.That(inArgument.ArgumentType, Is.EqualTo("String"));        
        }

        [Test]
        public void ValidateThatInArgumentCanBeInitializedWithDataBoundMode()
        {
            var inArgument = new InArgument<Person>() { Mode = ArgumentMode.DataBound, PropertyPath = "Age" };

            Assert.That(inArgument.Mode, Is.EqualTo(ArgumentMode.DataBound));
            Assert.That(inArgument.PropertyPath, Is.EqualTo("Age"));
            Assert.That(inArgument.DefaultValue is null);
            Assert.That(inArgument.GetDefaultValue() is null);
            Assert.That(inArgument.IsConfigured());
            Assert.That(inArgument.GetArgumentType(), Is.EqualTo(typeof(Person)));            
            Assert.That(string.IsNullOrEmpty(inArgument.ScriptFile));           
            Assert.That(inArgument.ArgumentType, Is.EqualTo("Person"));
        }

        [Test]
        public void ValidateThatInArgumentCanBeCloned()
        {
            var inArgument = new InArgument<Person>() { Mode = ArgumentMode.DataBound, PropertyPath = "Age", ScriptFile = "ArgumentScript.csx" };
            var clone = inArgument.Clone() as InArgument<Person>;
           
            Assert.That(clone.Mode, Is.EqualTo(inArgument.Mode));
            Assert.That(clone.PropertyPath, Is.EqualTo(inArgument.PropertyPath));
            Assert.That(clone.DefaultValue, Is.EqualTo(inArgument.DefaultValue));           
            Assert.That(clone.GetArgumentType(), Is.EqualTo(inArgument.GetArgumentType()));
            Assert.That(clone.ScriptFile, Is.EqualTo(inArgument.ScriptFile));
            Assert.That(clone.ArgumentType, Is.EqualTo(inArgument.ArgumentType));
        }
    }
}
