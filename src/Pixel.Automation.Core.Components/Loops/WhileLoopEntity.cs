using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Loops;

[DataContract]
[Serializable]
[ToolBoxItem("While Loop", "Loops", iconSource: null, description: "Contains a group of automation entity that will be prcossed in a while loop", tags: new string[] { "while loop" })]
[Scriptable("ScriptFile")]
[Initializer(typeof(ScriptFileInitializer))]
[NoDropTarget]
public class WhileLoopEntity : Entity, ILoop
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

  
    public WhileLoopEntity() : base("While Loop", "WhileLoopEntity")
    {

    }

    public override IEnumerable<Core.Interfaces.IComponent> GetNextComponentToProcess()
    {                
        int iteration = 0;
        logger.Information(": Begin while loop");
        while (true)
        {               
            ScriptResult scriptResult = ExecuteScript().Result;
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
            
            //Reset any inner loop before running next iteration
            foreach (var loop in this.GetInnerLoops())
            {                   
                (loop as Entity).ResetHierarchy();
            }

            iteration++;
        }
        logger.Information(": End while loop");
    }

    private async Task<ScriptResult> ExecuteScript()
    {

        IScriptEngine scriptExecutor = this.EntityManager.GetScriptEngine();
        ScriptResult result = await scriptExecutor.ExecuteFileAsync(this.scriptFile);           
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
        base.AddComponent(statementsPlaceHolder);
        
    }
    public override Entity AddComponent(Interfaces.IComponent component)
    {          
        return this;
    }
}
