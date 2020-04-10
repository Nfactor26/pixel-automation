using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Scripting.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Execute [Inline]", "Scripting", iconSource: null, description: "Assign value to a variable", tags: new string[] { "Assign", "Scripting" })]
    [Scriptable("ScriptFile")]
    public class ScriptedAssignActorComponent : ScriptedComponentBase
    {       
        public ScriptedAssignActorComponent() : base("Assign", "ScriptedAssign")
        {           
        }

        public override async Task ActAsync()
        {
            IScriptEngine scriptExecutor = this.EntityManager.GetServiceOfType<IScriptEngine>();
            _ = await scriptExecutor.ExecuteFileAsync(this.scriptFile);
        }
      
    }
}
