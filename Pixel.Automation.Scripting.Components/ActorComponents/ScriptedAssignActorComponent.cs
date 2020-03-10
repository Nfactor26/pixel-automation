using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Models;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Scripting.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Assign", "Scripting", iconSource: null, description: "Assign value to a variable", tags: new string[] { "Assign", "Scripting" })]
    public class ScriptedAssignActorComponent : ScriptedComponentBase
    {       
        public ScriptedAssignActorComponent() : base("Assign", "ScriptedAssign")
        {           
        }

        public override void Act()
        {
            var result =  ExecuteScript();
            result.GetAwaiter().GetResult();
        }

        async Task<ScriptResult> ExecuteScript()
        {
            //TODO : We can keep this on entity manager itself instead of creating it everytime
            // or initialize scriptExecutor with this one time like done for ArgumentProcessor
            //But then scriptExecutor will work only with ScriptGlobals ?? Think a better alternative.
            //Type scriptDataType = typeof(ScriptGlobals<>).MakeGenericType(this.EntityManager.Arguments.GetType());
            //var scriptData = Activator.CreateInstance(scriptDataType, new[] { this.EntityManager, this.EntityManager.Arguments });

            IScriptEngine scriptExecutor = this.EntityManager.GetServiceOfType<IScriptEngine>();
            ScriptResult result = await scriptExecutor.ExecuteFileAsync(this.scriptFile, this.EntityManager.Arguments, null);         
            return result;

        }
    }
}
