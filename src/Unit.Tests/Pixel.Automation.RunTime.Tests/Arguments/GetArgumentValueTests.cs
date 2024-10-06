using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Automation.RunTime.Tests
{
    /// <summary>
    /// Test cases covering various scenarios for GetValue operation on ArgumentProcessor for a given argument
    /// </summary>
    public class GetArgumentValueTest
    {
        private IScriptEngine defaultScriptEngine;

        [OneTimeSetUp]
        public void Setup()
        {
            defaultScriptEngine = Substitute.For<IScriptEngine>();
        }

        /// <summary>
        /// Validate that InvalidOperationException is throw when requested type is not assignable from argument type 
        /// </summary>
        [Test]    
        public async Task ShouldThrowExecptionIfArgumentTypeIsNotAssignableToRequestedType()
        {           
            ArgumentProcessor argumentProcessor = new ArgumentProcessor();
            argumentProcessor.Initialize(defaultScriptEngine, new object());
            var argument = new InArgument<int>() { Mode = ArgumentMode.Default, DefaultValue = 100 };

            Assert.ThrowsAsync<InvalidOperationException>(async () => await argumentProcessor.GetValueAsync<bool>(argument));
            await Task.CompletedTask;
        }

        /// <summary>
        /// Validate that Get operation can fetch correct value when argument has a default value of value type
        /// and argument is configured to use default mode.
        /// </summary>
        [Test]
        public async Task CanGetDefaultValueOfInArgumentHavingValueTypesInDefaultMode()
        {           
            ArgumentProcessor argumentProcessor = new ArgumentProcessor();
            argumentProcessor.Initialize(defaultScriptEngine, new object());
            var argument = new InArgument<int>() { Mode = ArgumentMode.Default, DefaultValue = 100 };

            var result = await argumentProcessor.GetValueAsync<int>(argument);

            Assert.That(result, Is.EqualTo(100));
        }

        /// <summary>
        /// Validate that Get operation can fetch correct value when argument has a default value of reference type
        /// and argument is configured to use default mode.
        /// </summary>
        [Test]
        public async Task CanGetDefaultValueOfInArgumentHavingReferenceTypesInDefaultMode()
        { 
            ArgumentProcessor argumentProcessor = new ArgumentProcessor();
            argumentProcessor.Initialize(defaultScriptEngine, new object());
            List<int> numbers = new List<int>() { 1, 2, 3, 4, 5 };
            var argument = new InArgument<List<int>>() { Mode = ArgumentMode.Default, DefaultValue = numbers };

            var result = await argumentProcessor.GetValueAsync<List<int>>(argument);

            Assert.That(result, Is.SameAs(numbers));
        }

        /// <summary>
        /// Validate that when property path is not configured in DataBound mode, an exception is thrown
        /// </summary>
        [Test]
        public void ShouldThrowExceptionIfArgumentIsNotCorrectlyConfigured()
        {             
            ArgumentProcessor argumentProcessor = new ArgumentProcessor();
            argumentProcessor.Initialize(defaultScriptEngine, new object());
            var argument = new InArgument<bool>() { Mode = ArgumentMode.DataBound };

            Assert.ThrowsAsync<ArgumentNotConfiguredException>(async () => { await argumentProcessor.GetValueAsync<bool>(argument); });
        }

        /// <summary>
        /// Validate that Get operation can fetch simple properties from globals object in data bound mode.
        /// Simple property here means properties that are not nested.
        /// </summary>
        [Test]
        public async Task CanGetSimplePropertyValueInDataBoundModeFromGlobalsObject()
        {
            IScriptEngine scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.HasScriptVariable(Arg.Any<string>()).Returns(false);
            Person person = new Person() { Name = "Sheldon Cooper", Age = 40 };
            ArgumentProcessor argumentProcessor = new ArgumentProcessor();
            argumentProcessor.Initialize(scriptEngine, person);
            var argument = new InArgument<string>() { Mode = ArgumentMode.DataBound, PropertyPath = "Name" };

            var result = await argumentProcessor.GetValueAsync<string>(argument);

            Assert.That(result, Is.EqualTo("Sheldon Cooper"));
        }


        /// <summary>
        /// Validate that Get operation can fetch nested properties from globals object in data bound mode.
        /// UI will allow binding until 2 levels only i.e. PropertyA.PropertyB . However, more nested properties can be used.
        /// </summary>
        [Test]
        public async Task CanGetNestedPropertyValueInDataBoundModeFromGlobalsObject()
        {
            IScriptEngine scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.HasScriptVariable(Arg.Any<string>()).Returns(false);
            Person person = new Person() { Name = "Sheldon Cooper", Age = 40 , Address = new Address() { City = "East Texas", Country = "US" } };
            ArgumentProcessor argumentProcessor = new ArgumentProcessor();
            argumentProcessor.Initialize(scriptEngine, person);
            var argument = new InArgument<string>() { Mode = ArgumentMode.DataBound, PropertyPath = "Address.City" };

            var result = await argumentProcessor.GetValueAsync<string>(argument);

            Assert.That(result, Is.EqualTo("East Texas"));
        }

        /// <summary>
        /// Validate that Get operation can fetch simple properties from script variable in data bound mode.
        /// Simple property here means properties that are not nested.
        /// </summary>
        [Test]
        public async Task CanGetSimplePropertyValueInDataBoundModeFromScriptVariable()
        {
            Person person = new Person() { Name = "Sheldon Cooper", Age = 40 };
            IScriptEngine scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.HasScriptVariable(Arg.Is<string>("person")).Returns(true);
            scriptEngine.GetVariableValue<Person>(Arg.Is<string>("person")).Returns(person);

            ArgumentProcessor argumentProcessor = new ArgumentProcessor();
            argumentProcessor.Initialize(scriptEngine, person);
            var argument = new InArgument<Person>() { Mode = ArgumentMode.DataBound, PropertyPath = "person" };

            var result = await argumentProcessor.GetValueAsync<Person>(argument);

            Assert.That(result, Is.SameAs(person));
        }

        /// <summary>
        /// Validate that Get operation can fetch nested properties from script variable in data bound mode.
        /// UI will allow binding until 2 levels only i.e. PropertyA.PropertyB . However, more nested properties can be used.
        /// </summary>
        [Test]
        public async Task CanGetNestedPropertyValueInDataBoundModeFromScriptVariable()
        {
            Person person = new Person() { Name = "Sheldon Cooper", Age = 40, Address = new Address() { City = "East Texas", Country = "US" } };
            IScriptEngine scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.HasScriptVariable(Arg.Is<string>("person")).Returns(true);
            scriptEngine.GetVariableValue<object>(Arg.Is<string>("person")).Returns(person);

            ArgumentProcessor argumentProcessor = new ArgumentProcessor();
            argumentProcessor.Initialize(scriptEngine, person);
            var argument = new InArgument<string>() { Mode = ArgumentMode.DataBound, PropertyPath = "person.Address.City" };

            var result =  await argumentProcessor.GetValueAsync<string>(argument);

            Assert.That(result, Is.EqualTo("East Texas"));
        }


        /// <summary>
        /// When trying to GetValue , if root property is a variable declared in script variable and globals also has same property,
        /// variable declared in script should be retrieved as script variables have higher preference.
        /// In test , we have globals Person which has a Name property. We have also Name as a script variable.
        /// Trying to get value of Name will get value from script variable and not on globals object.
        /// </summary>
        [Test]
        public async Task ValidateThatScriptVariablesHaveHigherPreference()
        {
            string Name = "Howard Wolowitz";
            Person person = new Person() { Name = "Sheldon Cooper", Age = 40 };
            IScriptEngine scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.HasScriptVariable(Arg.Is<string>("Name")).Returns(true);
            scriptEngine.GetVariableValue<string>(Arg.Is<string>("Name")).Returns(Name);
            ArgumentProcessor argumentProcessor = new ArgumentProcessor();
            argumentProcessor.Initialize(scriptEngine, person);
            var argument = new OutArgument<string>() { Mode = ArgumentMode.DataBound, PropertyPath = "Name" };

            var result = await argumentProcessor.GetValueAsync<string>(argument);

            scriptEngine.Received(1).HasScriptVariable("Name");
            scriptEngine.Received(1).GetVariableValue<string>("Name");
            Assert.That(result, Is.EqualTo("Howard Wolowitz"));

        }

        /// <summary>
        /// Validate that values can be retrieved using a custom script. Script can fetch from globals or script variable.
        /// Script can even return a hard coded value in script.
        /// </summary>
        [Test]
        public async Task CanGetValueInScriptedMode()
        {
            IScriptEngine scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.CreateDelegateAsync<Func<int>>(Arg.Any<string>()).Returns(() => { return 200; });
            ArgumentProcessor argumentProcessor = new ArgumentProcessor();
            argumentProcessor.Initialize(scriptEngine, new object());
            var argument = new InArgument<int>() { Mode = ArgumentMode.Scripted , ScriptFile = "script.csx" };

            var result = await argumentProcessor.GetValueAsync<int>(argument);

            Assert.That(result, Is.EqualTo(200));
        }

        /// <summary>
        /// Validate that if any exception is thrown while executing script , Get operation throws it back.
        /// </summary>
        [Test]
        public async Task ShouldThrowAnyExceptionGeneratedWhileExecutingScriptInScriptedMode()
        {
            IScriptEngine scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.CreateDelegateAsync<Func<int>>(Arg.Any<string>()).Returns(() => { throw new NotImplementedException(); });
            ArgumentProcessor argumentProcessor = new ArgumentProcessor();
            argumentProcessor.Initialize(scriptEngine, new object());
            var argument = new InArgument<int>() { Mode = ArgumentMode.Scripted, ScriptFile = "script.csx" };

            Assert.ThrowsAsync<NotImplementedException>(async () => await argumentProcessor.GetValueAsync<int>(argument));
            await Task.CompletedTask;

        }

    }
}