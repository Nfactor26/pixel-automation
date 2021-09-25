using NUnit.Framework;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pixel.Automation.Core.Tests.Arguments
{
    class InArgumentFixture
    {
        [Test]
        public void ValidateThatInArgumentCanBeInitializedWithDefaultMode()
        {
            var inArgument = new InArgument<int>() { DefaultValue = 26 };

            Assert.AreEqual(ArgumentMode.Default, inArgument.Mode);
            Assert.AreEqual(26, inArgument.DefaultValue);         
            Assert.IsTrue(inArgument.IsConfigured());
            Assert.AreEqual(26, inArgument.GetDefaultValue());
            Assert.AreEqual(typeof(int), inArgument.GetArgumentType());            
        }

        [Test]
        public void ValidateThatInArgumentCanBeInitializedWithScriptedMode()
        {
            var inArgument = new InArgument<string>() { Mode = ArgumentMode.Scripted, ScriptFile = "Script.csx" };

            Assert.AreEqual(ArgumentMode.Scripted, inArgument.Mode);
            Assert.AreEqual("Script.csx", inArgument.ScriptFile);
            Assert.AreEqual(string.Empty, inArgument.DefaultValue);
            Assert.AreEqual(string.Empty, inArgument.GetDefaultValue());
            Assert.IsTrue(inArgument.IsConfigured());             
            Assert.AreEqual(typeof(string), inArgument.GetArgumentType());
            Assert.AreEqual("String", inArgument.ArgumentType);        
        }

        [Test]
        public void ValidateThatInArgumentCanBeInitializedWithDataBoundMode()
        {
            var inArgument = new InArgument<Person>() { Mode = ArgumentMode.DataBound, PropertyPath = "Age" };

            Assert.AreEqual(ArgumentMode.DataBound, inArgument.Mode);
            Assert.AreEqual("Age", inArgument.PropertyPath);
            Assert.IsNotNull(inArgument.DefaultValue);
            Assert.IsNotNull(inArgument.GetDefaultValue());
            Assert.IsTrue(inArgument.IsConfigured());
            Assert.AreEqual(typeof(Person), inArgument.GetArgumentType());            
            Assert.IsTrue(string.IsNullOrEmpty(inArgument.ScriptFile));           
            Assert.AreEqual("Person", inArgument.ArgumentType);
        }

        [Test]
        public void ValidateThatInArgumentCanBeCloned()
        {
            var inArgument = new InArgument<Person>() { Mode = ArgumentMode.DataBound, PropertyPath = "Age", ScriptFile = "ArgumentScript.csx" };
            var clone = inArgument.Clone() as InArgument<Person>;
           
            Assert.AreEqual(inArgument.Mode, clone.Mode);
            Assert.AreEqual(inArgument.PropertyPath, clone.PropertyPath);
            Assert.AreEqual(inArgument.DefaultValue, clone.DefaultValue);           
            Assert.AreEqual(inArgument.GetArgumentType(), clone.GetArgumentType());
            Assert.AreEqual(inArgument.ScriptFile, clone.ScriptFile);
            Assert.AreEqual(inArgument.ArgumentType, clone.ArgumentType);
        }
    }
}
