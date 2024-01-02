using Pixel.Automation.Core.Attributes;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components.Loops;

[DataContract]
[Serializable]
[ToolBoxItem("While Loop", "Loops", iconSource: null, description: "Contains a group of automation entity that will be prcossed in a while loop", tags: new string[] { "while loop" })]
[Scriptable("ScriptFile")]
[Initializer(typeof(ScriptFileInitializer))]
[NoDropTarget]
public class WhileLoopEntity : LoopEntity
{
    private readonly ILogger logger = Log.ForContext<WhileLoopEntity>();

    protected string scriptFile;
    [DataMember(Order = 200)]
    [Browsable(false)]
    public string ScriptFile
    {
        get => scriptFile;
        set => scriptFile = value;
    }    
  
    public WhileLoopEntity() : base("While Loop", "WhileLoopEntity")
    {

    }

    public override IEnumerable<Interfaces.IComponent> GetNextComponentToProcess()
    {       
        int iteration = 0;
        logger.Information(": Begin while loop");
        while (true)
        {               
            var scriptResult = ExecuteScriptFile(scriptFile).Result;
            this.exitCriteriaSatisfied = !(bool)scriptResult.ReturnValue;
            if(this.exitCriteriaSatisfied)
            {
                logger.Information($"While condition evaluated to false after {0} iterations",iteration);
                break;
            }
                            
            logger.Information("Running iteration : {0}", iteration);

            var placeHolderEntity = this.GetFirstComponentOfType<PlaceHolderEntity>();
            var iterator = placeHolderEntity.GetNextComponentToProcess().GetEnumerator();
            while (iterator.MoveNext())
            {
                yield return iterator.Current;                   
            }

            this.ResetDescendants();

            iteration++;
        }
        logger.Information(": End while loop");
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
