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

            Assert.AreEqual(1, result.ReturnValue);
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

            Assert.IsNotNull(scriptState.GetVariable("x"));
            Assert.AreEqual(1, scriptState.GetVariable("x").Value);
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

            Assert.IsTrue(person.Friends.Count() == 0);

            var result = await scriptRunner.ExecuteScriptAsync(scriptCode, scriptOptions, person);
            Assert.IsTrue(person.Friends.Count() > 0);
          
            var friend = person.Friends.FirstOrDefault();
            Assert.IsNotNull(friend);
            Assert.AreEqual("Sheldon Cooper", friend.Name);
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

            Assert.IsNotNull(scriptState.GetVariable("x"));
            Assert.AreEqual(1, scriptState.GetVariable("x").Value);

            //we again execute next script from previous state of ScriptRunner and capture new result and new state
            var scriptCodeNext = @"x = x + 1; int y = 0;";
            result = await scriptRunner.ExecuteScriptAsync(scriptCodeNext, scriptOptions, null,  scriptState);
            scriptState = result.CurrentState as ScriptState;

            Assert.IsNotNull(scriptState.GetVariable("x"));
            Assert.IsNotNull(scriptState.GetVariable("y"));
            Assert.AreEqual(2, scriptState.GetVariable("x").Value);
            Assert.AreEqual(0, scriptState.GetVariable("y").Value);
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
           
            Assert.AreEqual(8, sum);
        }

    }
}
