using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Interfaces;
using Serilog;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Scripting.Components;

/// <summary>
/// Use <see cref="ExecuteAsyncScriptActorComponent"/> to execute custom scripts having asyncrhonous operations.
/// </summary>
[DataContract]
[Serializable]
[Scriptable("ScriptFile")]
[Initializer(typeof(ScriptFileInitializer))]
[ToolBoxItem("Script File (async)", "Scripting", iconSource: null, description: "Execute script that use async api's", tags: new string[] { "script"})]   
public class ExecuteAsyncScriptActorComponent : ActorComponent
{
    private readonly ILogger logger = Log.ForContext<ExecuteAsyncScriptActorComponent>();

    protected string scriptFile;
    [DataMember]
    [System.ComponentModel.DisplayName("Script File")]      
    public string ScriptFile
    {
        get => scriptFile;
        set
        {
            scriptFile = value;
            OnPropertyChanged();
        }
    }

    public ExecuteAsyncScriptActorComponent() : base("Execute Async Script", "ExecuteAsyncScript")
    {

    }

    public override async Task ActAsync()
    {
        IScriptEngine scriptEngine = this.EntityManager.GetScriptEngine();
        var action = await scriptEngine.CreateDelegateAsync<Func<IApplication, IComponent, Task>>(this.ScriptFile);
        await action(this.EntityManager.GetOwnerApplication(this), this);
        logger.Information("Script file : '{0}' was executed", this.ScriptFile);
    }
 
}
