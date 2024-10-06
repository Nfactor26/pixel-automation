using Microsoft.CodeAnalysis.Scripting;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Scripting.Engine.CSharp.Tests
{
    public class ScriptRunnerTests
    { 
        /// <summary>
        /// Execute a script which returns a value and verify that result of script execution captures
        /// the actual value returned by script.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task CanExecuteScriptAndReturnValues()
        {
            ScriptRunner scriptRunner = new ScriptRunner();
            var scriptOptions = ScriptOptions.Default.AddReferences(typeof(int).Assembly);
            var scriptCode = @"int x = 0; x = x + 1; return x;";

            var result = await scriptRunner.ExecuteScriptAsync(scriptCode, scriptOptions, null);

            Assert.That(result.ReturnValue, Is.EqualTo(1));
        }

        [TestCase("x == 0", true)]
        [TestCase("x != 0", false)]
        public async Task CanExecuteCondition(string condition, bool expected)
        {
            ScriptRunner scriptRunner = new ScriptRunner();
            var scriptOptions = ScriptOptions.Default.AddReferences(typeof(int).Assembly);           
            var scriptResult = await scriptRunner.ExecuteScriptAsync("int x = 0;", scriptOptions, null);
            var continuationResult = await scriptRunner.ExecuteScriptAsync(condition, scriptOptions, null, scriptResult.CurrentState);

            Assert.That((bool)continuationResult.ReturnValue, Is.EqualTo(expected));
        }

        /// <summary>
        /// Execute a script which defines a integer variable x and modifies it values.
        /// Assert that captured script state has this variable x and it's value is what is expected.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task CanExecuteScriptAndHaveCorrectState()
        {
            ScriptRunner scriptRunner = new ScriptRunner();
            var scriptOptions = ScriptOptions.Default.AddReferences(typeof(int).Assembly);
            var scriptCode = @"int x = 0; x = x + 1;";

            var result = await scriptRunner.ExecuteScriptAsync(scriptCode, scriptOptions, null);
            var scriptState = result.CurrentState as ScriptState;

            Assert.That(scriptState.GetVariable("x") is not null);
            Assert.That(scriptState.GetVariable("x").Value, Is.EqualTo(1));
        }

        /// <summary>
        /// Execute a script with globals object. Script modifies the globals object.
        /// Verify that any changes in globals object from script are reflected on globals object after script is executed.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task CanExecuteScriptWithGlobalsAndModifyGlobalsState()
        {
            Person person = new Person() { Name = "Leonard Hofstadter", Age = 44 };
            ScriptRunner scriptRunner = new ScriptRunner();
            var scriptOptions = ScriptOptions.Default.AddReferences(typeof(int).Assembly, typeof(Person).Assembly, typeof(List<>).Assembly);
            scriptOptions = scriptOptions.AddImports(typeof(List<>).Namespace, typeof(Person).Namespace);
            var scriptCode = @"using System.Collections.Generic; Friends.Add(new Person(){ Name = ""Sheldon Cooper"", Age = 40  });";

            Assert.That(person.Friends.Count() == 0);

            var result = await scriptRunner.ExecuteScriptAsync(scriptCode, scriptOptions, person);
            Assert.That(person.Friends.Count() > 0);
          
            var friend = person.Friends.FirstOrDefault();
            Assert.That(friend is not null);
            Assert.That(friend.Name, Is.EqualTo("Sheldon Cooper"));
        }

        /// <summary>
        /// Verify that ScriptRunner can execute scripts while taking into account a previous state ( result of execution of a previous script) when specified.
        /// ScriptRunner doesn't maintain state however. If execution should happen from a previous state, previous state must be passed to the ScriptRunner.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task CanContinueExecutionFromPreviousState()
        {
            ScriptRunner scriptRunner = new ScriptRunner();
            var scriptOptions = ScriptOptions.Default.AddReferences(typeof(int).Assembly);
            var scriptCode = @"int x = 0; x = x + 1;";

            //previous state is not specified here
            var result = await scriptRunner.ExecuteScriptAsync(scriptCode, scriptOptions, null);
            var scriptState = result.CurrentState as ScriptState;

            Assert.That(scriptState.GetVariable("x") is not null);
            Assert.That(scriptState.GetVariable("x").Value, Is.EqualTo(1));

            //we again execute next script from previous state of ScriptRunner and capture new result and new state
            var scriptCodeNext = @"x = x + 1; int y = 0;";
            result = await scriptRunner.ExecuteScriptAsync(scriptCodeNext, scriptOptions, null,  scriptState);
            scriptState = result.CurrentState as ScriptState;

            Assert.That(scriptState.GetVariable("x") is not null);
            Assert.That(scriptState.GetVariable("y") is not null);
            Assert.That(scriptState.GetVariable("x").Value, Is.EqualTo(2));
            Assert.That(scriptState.GetVariable("y").Value, Is.EqualTo(0));
        }

        /// <summary>
        /// Verify that ScriptRunner can return a method as a Func<T, TResult> which can be used to execute the method
        /// by passing parameters T
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task CanGetObjectDefinedInScriptAndInvokeMethodOnThemWithParameters()
        {
            ScriptRunner scriptRunner = new ScriptRunner();
            var scriptOptions = ScriptOptions.Default.AddReferences(typeof(int).Assembly, typeof(Func<>).Assembly);
            scriptOptions = scriptOptions.WithImports(typeof(int).Namespace);
            var scriptCode = @"int Sum(int x, int y) { return x + y ;}  return (Func<int,int,int>)Sum;";
         
            var result =  await scriptRunner.ExecuteScriptAsync(scriptCode, scriptOptions, null, null);
            var fn = (Func<int, int, int>)(result.ReturnValue);
            int sum = fn(2, 6);
           
            Assert.That(sum, Is.EqualTo(8));
        }

    }
}
