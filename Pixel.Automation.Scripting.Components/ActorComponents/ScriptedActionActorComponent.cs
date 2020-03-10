using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Models;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Scripting.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Execute", "Scripting", iconSource: null, description: "Execute any provided script", tags: new string[] { "Scripted Action", "Scripting" })]
    public class ScriptedActionActorComponent : ScriptedComponentBase
    {

        public ScriptedActionActorComponent() : base("Scripted Action", "ScriptedAction")
        {

        }

        public override void Act()
        {
            var result  = ExecuteScript();
            result.GetAwaiter().GetResult();
        }


        async Task<ScriptResult> ExecuteScript()
        {
            Entity controlEntity = default;
            if (this.Parent is ControlEntity)
            {
                controlEntity = this.Parent;
            }

            Type scriptDataType = typeof(ActorScriptArguments<>).MakeGenericType(this.EntityManager.Arguments.GetType());
            var scriptData = Activator.CreateInstance(scriptDataType, new[] { this.EntityManager, this.EntityManager.GetApplicationDetails(this), controlEntity, this.EntityManager.Arguments });
           
         
            IScriptEngine scriptEngine = this.EntityManager.GetServiceOfType<IScriptEngine>();

            ScriptResult scriptResult = await scriptEngine.ExecuteFileAsync(this.scriptFile, scriptData, null);           
            scriptResult = await scriptEngine.ExecuteScriptAsync("TryExecute(EntityManager,Application,ControlEntity,DataModel)", scriptData, scriptResult.CurrentState);          
            return scriptResult;
        }

    }
}
