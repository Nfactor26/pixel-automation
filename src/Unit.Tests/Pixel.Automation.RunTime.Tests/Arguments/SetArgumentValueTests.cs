using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Arguments;
using System;
using System.Threading.Tasks;

namespace Pixel.Automation.RunTime.Tests
{
    /// <summary>
    /// Test cases covering various scenarios for SetValue operation on ArgumentProcessor for a given argument
    /// </summary>
    public class SetArgumentValueTests
    {
        private IScriptEngine defaultScriptEngine;

        [OneTimeSetUp]
        public void Setup()
        {
            defaultScriptEngine = Substitute.For<IScriptEngine>();
        }

        /// <summary>
        /// Validate that InvalidOperationException is thrown if Argument type e.g. int is not assignable from specified type e.g. bool
        /// </summary>
        [Test]
        public async Task ShouldThrowExecptionIfArgumentTypeIsNotAssignableFromRequestedType()
        {          
            ArgumentProcessor argumentProcessor = new ArgumentProcessor();
            argumentProcessor.Initialize(defaultScriptEngine, new object());
            var argument = new OutArgument<int>() { Mode = ArgumentMode.Default };

            Assert.ThrowsAsync<InvalidOperationException>(async () => await argumentProcessor.SetValueAsync<bool>(argument, false));
            await Task.CompletedTask;
        }

        /// <summary>
        /// Validate that InvalidOperationException is thrown since SetValue operation is not applicable for a OutArgument<T>
        /// which doesn't have a default value. Only InArgument<T> have default value which can be retrieved only using GetValue operation
        /// </summary>
        [Test]
        public async Task ShouldThrowExceptionIfArgumentIsConfiguredForDefaultMode()
        {
            ArgumentProcessor argumentProcessor = new ArgumentProcessor();
            argumentProcessor.Initialize(defaultScriptEngine, new object());
            var argument = new OutArgument<int>() { Mode = ArgumentMode.Default };

            Assert.ThrowsAsync<InvalidOperationException>(async () => await argumentProcessor.SetValueAsync<int>(argument, 10));
            await Task.CompletedTask;
           
        }

        /// <summary>
        /// Several OutArgument<T> on components can not be configured as user might not be intersted in storing these values.
        /// For ex: FindControl has two OutArgument to store actual control and a boolean indicating whether control was found.
        /// User might only store the control and not configure the boolean Argument. To avoid validation logic in each actor component
        /// to check if OutArgument has Property Path configured or not before trying to set value , ArgumentProcessor will simply ignore
        /// those argument and only log that Property Path is not configured.
        /// </summary>
        [Test]
        public async Task ShouldNotThrowExceptionIfPropertyPathIsNotConfiguredInDataBoundMode()
        {          
            ArgumentProcessor argumentProcessor = new ArgumentProcessor();
            argumentProcessor.Initialize(defaultScriptEngine, new object());

            var argument = new OutArgument<bool>() { Mode = ArgumentMode.DataBound };
            await argumentProcessor.SetValueAsync<bool>(argument, true);           
        }

        /// <summary>
        /// Validate that non-nested property can be set on globals object
        /// </summary>
        [Test]
        public async Task CanSetSimplePropertyValueInDataBoundModeOnGlobalsObject()
        {
            IScriptEngine scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.HasScriptVariable(Arg.Any<string>()).Returns(false);
         
            Person person = new Person() { Name = "Sheldon Cooper", Age = 40 };
            ArgumentProcessor argumentProcessor = new ArgumentProcessor();
            argumentProcessor.Initialize(scriptEngine, person);
           
            var ageArgument = new OutArgument<int>() { Mode = ArgumentMode.DataBound, PropertyPath = "Age" };
            await argumentProcessor.SetValueAsync<int>(ageArgument, 50);
            Assert.AreEqual(50, person.Age);          

            var nameArgument = new OutArgument<string>() { Mode = ArgumentMode.DataBound, PropertyPath = "Name" };
            await argumentProcessor.SetValueAsync<string>(nameArgument, "Leonard Hofstadter");
            Assert.AreEqual("Leonard Hofstadter", person.Name);           
            await argumentProcessor.SetValueAsync<string>(nameArgument, null);
            Assert.AreEqual(null, person.Name);

            scriptEngine.Received(3).HasScriptVariable(Arg.Any<string>());
        }

        /// <summary>
        /// Validate that nested properties can be set on globals object.
        /// </summary>
        [Test]
        public async Task CanSetNestedPropertyValueInDataBoundModeOnGlobalsObject()
        {
            Person person = new Person() { Name = "Sheldon Cooper", Age = 40, Address = new Address() { City = "East Texas", Country = "US" } };
            IScriptEngine scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.HasScriptVariable(Arg.Any<string>()).Returns(false);
            ArgumentProcessor argumentProcessor = new ArgumentProcessor();
            argumentProcessor.Initialize(scriptEngine, person);
            var argument = new OutArgument<string>() { Mode = ArgumentMode.DataBound, PropertyPath = "Address.City" };

            await argumentProcessor.SetValueAsync<string>(argument, "Oklahoma");

            scriptEngine.Received(1).HasScriptVariable(Arg.Any<string>());
            Assert.AreEqual("Oklahoma", person.Address.City);
        }

        /// <summary>
        /// Validate that simple property can be set on script variable
        /// </summary>
        [Test]
        public async Task CanSetSimplePropertyValueInDataBoundModeOnScriptVariable()
        {
            int number = 10;
            IScriptEngine scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.HasScriptVariable(Arg.Is<string>("number")).Returns(true);
            scriptEngine.When(x => x.SetVariableValue<int>(Arg.Is<string>("number"), Arg.Any<int>())).Do(x =>
            {
                number = x.ArgAt<int>(1);
            });
            ArgumentProcessor argumentProcessor = new ArgumentProcessor();
            argumentProcessor.Initialize(scriptEngine, new object());
            var argument = new OutArgument<int>() { Mode = ArgumentMode.DataBound, PropertyPath = "number" };

            await argumentProcessor.SetValueAsync<int>(argument, 50);

            scriptEngine.Received(1).HasScriptVariable("number");
            scriptEngine.Received(1).SetVariableValue("number", 50);
            Assert.AreEqual(50, number);
        }


        /// <summary>
        /// Validate that nested property can be set on script variable
        /// </summary>
        [Test]
        public async Task CanSetNestedPropertyValueInDataBoundModeOnScriptVariable()
        {
            Person person = new Person() { Name = "Sheldon Cooper", Age = 40, Address = new Address() { City = "East Texas", Country = "US" } };
            IScriptEngine scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.HasScriptVariable(Arg.Is<string>("person")).Returns(true);
            scriptEngine.GetVariableValue<object>(Arg.Is<string>("person")).Returns(person);

            ArgumentProcessor argumentProcessor = new ArgumentProcessor();
            argumentProcessor.Initialize(scriptEngine, new object());
            var argument = new OutArgument<string>() { Mode = ArgumentMode.DataBound, PropertyPath = "person.Address.City" };

            await argumentProcessor.SetValueAsync<string>(argument, "Oklahoma");

            scriptEngine.Received(1).HasScriptVariable("person");
            scriptEngine.Received(1).GetVariableValue<object>("person");

            Assert.AreEqual("Oklahoma", person.Address.City);
        }


        /// <summary>
        /// When trying to SetValue , if root property is a variable declared in script variable and globals also has same property,
        /// variable declared in script should be modified as script variables have higher preference.
        /// In test , we have globals Person which has a Name property. We have also Name as a script variable.
        /// Trying to set value of Name will set value on script variable and not on globals object.
        /// </summary>
        [Test]
        public async Task ValidateThatScriptVariablesHaveHigherPreference()
        {
            Person person = new Person() { Name = "Sheldon Cooper", Age = 40 };
            IScriptEngine scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.HasScriptVariable(Arg.Is<string>("Name")).Returns(true);
            scriptEngine.When(x => x.SetVariableValue<string>(Arg.Is<string>("Name"), Arg.Any<string>())).Do(x =>
            {
                //Do nothing. We will just assert call to SetVariableValue was made
            });
            ArgumentProcessor argumentProcessor = new ArgumentProcessor();
            argumentProcessor.Initialize(scriptEngine, person);
            var argument = new OutArgument<string>() { Mode = ArgumentMode.DataBound, PropertyPath = "Name" };

            await argumentProcessor.SetValueAsync<string>(argument, "Howard Wolowitz");

            scriptEngine.Received(1).HasScriptVariable("Name");
            scriptEngine.Received(1).SetVariableValue("Name", "Howard Wolowitz");
            Assert.AreNotEqual("Howard Wolowitz", person.Name); //Asser that glboals-> Name was not modified.

        }

        /// <summary>
        /// SetValue operation should throw ArgumentException when property could not be located on target object i.e. either 
        /// global object or script varialbe object
        /// </summary>
        [Test]
        public async Task ShouldThrowArgumentExceptionWhenPropertyCanNotBeFoundOnTargetObject()
        {
            Person person = new Person() { Name = "Sheldon Cooper", Age = 40, Address = new Address() { City = "East Texas", Country = "US" } };
            IScriptEngine scriptEngine = Substitute.For<IScriptEngine>();
            ArgumentProcessor argumentProcessor = new ArgumentProcessor();
            argumentProcessor.Initialize(scriptEngine, person);
            var argument = new OutArgument<string>() { Mode = ArgumentMode.DataBound, PropertyPath = "Address.Street" };

            Assert.ThrowsAsync<ArgumentException>(async () => await argumentProcessor.SetValueAsync<string>(argument, "Oklahoma"));
            await Task.CompletedTask;
        }

        /// <summary>
        /// Validate that Argument processor should is able to set a value through a script for a given OutArgument<T>
        /// in ScriptedMode
        /// </summary>
        [Test]
        public async Task CanSetValueInScriptedMode()
        {
            Person person = new Person() { Name = "Sheldon Cooper", Age = 40, Address = new Address() { City = "East Texas", Country = "US" } };
          
            IScriptEngine scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.CreateDelegateAsync<Action<int>>(Arg.Any<string>()).Returns((i) => { person.Age = i; });
          
            ArgumentProcessor argumentProcessor = new ArgumentProcessor();
            argumentProcessor.Initialize(scriptEngine, new object());
            var argument = new OutArgument<int>() { Mode = ArgumentMode.Scripted, ScriptFile = "script.csx" };

            await argumentProcessor.SetValueAsync<int>(argument, 50);

            Assert.AreEqual(50, person.Age);
        }

        /// <summary>
        /// Validate that Argument processor throws any exception encountered while executing the script associated with OutArgument<T>
        /// in ScriptedMode
        /// </summary>
        [Test]
        public async Task ShouldThrowAnyExceptionGeneratedWhileExecutingScriptInScriptedMode()
        {
            IScriptEngine scriptEngine = Substitute.For<IScriptEngine>();
            Action<int> action = new Action<int>((i) => throw new NotImplementedException());
            scriptEngine.CreateDelegateAsync<Action<int>>(Arg.Any<string>()).Returns(action);

            ArgumentProcessor argumentProcessor = new ArgumentProcessor();
            argumentProcessor.Initialize(scriptEngine, new object());
            var argument = new OutArgument<int>() { Mode = ArgumentMode.Scripted, ScriptFile = "script.csx" };

            Assert.ThrowsAsync<NotImplementedException>(async () => await argumentProcessor.SetValueAsync<int>(argument, 50));
            await Task.CompletedTask;
        }

    }
}
