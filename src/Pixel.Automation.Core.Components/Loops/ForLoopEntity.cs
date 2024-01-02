using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components.Loops;

[DataContract]
[Serializable]
[ToolBoxItem("For Loop", "Loops", iconSource: null, description: "Contains a group of automation entity that will be prcossed in a for loop", tags: new string[] { "for loop" })]
[Scriptable("ScriptFile")]
[Initializer(typeof(ScriptFileInitializer))]
[NoDropTarget]
public class ForLoopEntity : LoopEntity
{
    private readonly ILogger logger = Log.ForContext<ForLoopEntity>();

    protected string scriptFile;
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

    public ForLoopEntity() : base("For Loop", "ForLoopEntity")
    {

    }

    public  override IEnumerable<Interfaces.IComponent> GetNextComponentToProcess()
    {           
        IFileSystem fileSystem = this.EntityManager.GetCurrentFileSystem();
        string[] statements = fileSystem.ReadAllText(Path.Combine(fileSystem.WorkingDirectory, this.scriptFile))?.Trim()
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
      
        _ = ExecuteScript(statements[0] + ";").Result; //execute the initialization part

        logger.Information(": Begin for loop");      
        for (int i = 0; ; i++)
        {
            //bool scriptResult = ExecuteConditionScript(statements[1]).Result;  //execute the condition part
            var scriptResult = ExecuteScript(statements[1]).Result;
            this.exitCriteriaSatisfied = !(bool)scriptResult.ReturnValue;
            if (this.exitCriteriaSatisfied)
            {
                logger.Information($"Loop condition evaluated to false after {0} iterations", i + 1);
                break;
            }

            logger.Information("Running iteration : {0}", i);

            var placeHolderEntity = this.GetFirstComponentOfType<PlaceHolderEntity>();
            var iterator = placeHolderEntity.GetNextComponentToProcess().GetEnumerator();
            while (iterator.MoveNext())
            {
                yield return iterator.Current;
            }          
            
            _ = ExecuteScript(statements[2] + ";").Result;  //Execute the increment statement
            
            this.ResetDescendants();
        }
        logger.Information(": End for loop");
    }  

    public override void ResolveDependencies()
    {
        if (this.Components.Count() > 0)
        {
            return;
        }

        PlaceHolderEntity statementsPlaceHolder = new PlaceHolderEntity("Statements");
        base.AddComponent(statementsPlaceHolder);            
    }

    public override Entity AddComponent(Interfaces.IComponent component)
    {
        return this;
    }

}
