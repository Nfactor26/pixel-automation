using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Loops
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("For Loop", "Loops", iconSource: null, description: "Contains a group of automation entity that will be prcossed in a for loop", tags: new string[] { "for loop" })]
    [Scriptable("ScriptFile")]
    [Initializer(typeof(ScriptFileInitializer))]
    public class ForLoopEntity : Entity, ILoop
    {
        protected string scriptFile = $"{Guid.NewGuid().ToString()}.csx";
        /// <summary>
        /// Script file holds the exit criteria script
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public string ScriptFile
        {
            get => scriptFile;
            set => scriptFile = value;
        }


        [NonSerialized]
        bool exitCriteriaSatisfied;
        [Browsable(false)]     
        public bool ExitCriteriaSatisfied
        {
            get
            {
                return exitCriteriaSatisfied;
            }

            set
            {
                this.exitCriteriaSatisfied = value;
            }
        }        

        public ForLoopEntity() : base("For Loop", "ForLoopEntity")
        {

        }

        public  override IEnumerable<Core.Interfaces.IComponent> GetNextComponentToProcess()
        {
            IArgumentProcessor argumentProcessor = this.EntityManager.GetServiceOfType<IArgumentProcessor>();
            IFileSystem fileSystem = this.EntityManager.GetCurrentFileSystem();
            string[] statements = File.ReadAllText(Path.Combine(fileSystem.ScriptsDirectory,this.scriptFile))?.Trim()
                .Split(new char[] {';'});

            //Number of statements is 4 when ; is placed after incrment statement otherwise 3.
            if (statements.Length < 3 || statements.Length > 4)
            {
                throw new FormatException($"For loop statement for componet with Id : {Id} is incorrectly formed." +
                  $"statement must have exactly three parts structured in the form initialization;condition;increment");
            }              
            if(statements.Length == 4 && !string.IsNullOrEmpty(statements[3]))
            {
                throw new FormatException($"For loop statement for componet with Id : {Id} is incorrectly formed." +
                               $"statement must have exactly three parts structured in the form initialization;condition;increment");
            }

           var initResult = ExecuteScript(statements[0]).Result; //execute the initialization part

            int iteration = 0;
            for (int i = 0; ; i++)
            {
                ScriptResult scriptResult = ExecuteScript(statements[1]).Result;  //execute the condition part
                this.exitCriteriaSatisfied = !(bool)scriptResult.ReturnValue;
                if (this.exitCriteriaSatisfied)
                {
                    Log.Information($"loop condition evaluated to false for For loop component with Id : {Id} " +
                     $"after {iteration} iterations", iteration, this.Id);
                    break;
                }

                Log.Debug("Running iteration : {Iteration} of For Loop component with Id : {Id}", i, this.Id);

                var iterator = base.GetNextComponentToProcess().GetEnumerator();
                while (iterator.MoveNext())
                {
                    yield return iterator.Current;
                }

                //Reset any inner loop before running next iteration
                foreach (var loop in this.GetInnerLoops())
                {
                    (loop as Entity).ResetHierarchy();
                }

                iteration++;

                _ = ExecuteScript(statements[2]).Result;  //Execute the increment statement
            }

        }

        private async Task<ScriptResult> ExecuteScript(string scriptToExecute)
        {
            IScriptEngine scriptExecutor = this.EntityManager.GetServiceOfType<IScriptEngine>();
            ScriptResult result = await scriptExecutor.ExecuteScriptAsync(scriptToExecute);         
            return result;
        }

        public override void ResetComponent()
        {
            base.ResetComponent();           
            this.ExitCriteriaSatisfied = false;
        }

        public override void ResolveDependencies()
        {
            if (this.Components.Count() > 0)
            {
                return;
            }

            PlaceHolderEntity statementsPlaceHolder = new PlaceHolderEntity("Statements");
            this.AddComponent(statementsPlaceHolder);
            
        }
    }
}
