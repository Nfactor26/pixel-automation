using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Components.Loops;

/// <summary>
/// Base class for loop components e.g. for loop, while loop, etc.
/// </summary>
public abstract class LoopEntity : Entity, ILoop
{
    [NonSerialized]
    protected bool exitCriteriaSatisfied;
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

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="tag"></param>
    public LoopEntity(string name, string tag) : base(name, tag)
    {

    }

    public override void ResetComponent()
    {
        base.ResetComponent();
        this.ExitCriteriaSatisfied = false;
    }
    
    protected async Task<ScriptResult> ExecuteScript(string scriptToExecute)
    {
        IScriptEngine scriptExecutor = this.EntityManager.GetScriptEngine();
        ScriptResult result = await scriptExecutor.ExecuteScriptAsync(scriptToExecute);
        return result;
    }

    protected async Task<ScriptResult> ExecuteScriptFile(string scriptFile)
    {
        IScriptEngine scriptExecutor = this.EntityManager.GetScriptEngine();
        ScriptResult result = await scriptExecutor.ExecuteFileAsync(scriptFile);
        return result;
    }

}
