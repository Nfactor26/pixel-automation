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

            //Assert.IsFalse(inArgument.CanChangeMode);
            //Assert.IsFalse(inArgument.CanChangeType);
            //Assert.IsTrue(string.IsNullOrEmpty(inArgument.ScriptFile));
            //Assert.IsTrue(string.IsNullOrEmpty(inArgument.PropertyPath));          
            //Assert.AreEqual("int", inArgument.ArgumentType);

            var copyOfInArgument = inArgument.Clone() as InArgument<int>;
            Assert.AreEqual(inArgument, copyOfInArgument);
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
            //Assert.AreEqual("string", inArgument.ArgumentType);

            //we don't allow two arguments to point to the same script file. Hence, two scripted arguments can't be equal.
            var copyOfInArgument = inArgument.Clone() as InArgument<string>;
            Assert.AreNotEqual(inArgument, copyOfInArgument);
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
            
            //Assert.IsTrue(string.IsNullOrEmpty(inArgument.ScriptFile));           
            //Assert.AreEqual("Person", inArgument.ArgumentType);

            var copyOfInArgument = inArgument.Clone() as InArgument<Person>;
            Assert.AreEqual(inArgument, copyOfInArgument);
        }
    }
}
