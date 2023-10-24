using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Decisions;

[DataContract]
[Serializable]
[ToolBoxItem("If", "Decisions", iconSource: null, description: "Wraps a group of automation entity that are processed only if the if criteria is satisfied ", tags: new string[] { "Deicsion", "DecisionGroup", "Entity" })]
[Scriptable("ScriptFile")]
[Initializer(typeof(ScriptFileInitializer))]
[NoDropTarget]
public class IfEntity : Entity
{
    private readonly ILogger logger = Log.ForContext<IfEntity>();

    protected string scriptFile;
    [DataMember(Order = 200)]
    [Browsable(false)]
    public string ScriptFile
    {
        get => scriptFile;
        set => scriptFile = value;
    }

    public IfEntity() : base("If", "IfEntity")
    {           

    }     

    public override IEnumerable<Interfaces.IComponent> GetNextComponentToProcess()
    {

        ScriptResult scriptResult = ExecuteScript().Result;

        IEnumerator<Interfaces.IComponent> conditionalBlockIterator = default;
        switch((bool)scriptResult.ReturnValue)
        {
            case true:
                logger.Information("If block evaluated to 'true' condition");
                Entity thenBlock = this.GetComponentsByName("Then").Single() as Entity;
                conditionalBlockIterator = thenBlock.GetNextComponentToProcess().GetEnumerator();                  
                break;
            case false:
                logger.Information("If block evaluated to 'false' condition");
                Entity elseBlock = this.GetComponentsByName("Else").Single() as Entity;
                conditionalBlockIterator = elseBlock.GetNextComponentToProcess().GetEnumerator();
                break;
        }

        while (conditionalBlockIterator.MoveNext())
        {
            yield return conditionalBlockIterator.Current;
        }         
    }

    private async Task<ScriptResult> ExecuteScript()
    {
        IScriptEngine scriptExecutor = this.EntityManager.GetScriptEngine();
        ScriptResult result = await scriptExecutor.ExecuteFileAsync(this.scriptFile);        
        return result;          
    }


    public override void ResolveDependencies()
    {
        if (this.Components.Count() > 0)
        {
            return;
        }

        PlaceHolderEntity thenBlock = new PlaceHolderEntity("Then");
        PlaceHolderEntity elseBlock = new PlaceHolderEntity("Else");  
                
        base.AddComponent(thenBlock);
        base.AddComponent(elseBlock);          
    }

    public override Entity AddComponent(Interfaces.IComponent component)
    {
        return this;
    }

}
