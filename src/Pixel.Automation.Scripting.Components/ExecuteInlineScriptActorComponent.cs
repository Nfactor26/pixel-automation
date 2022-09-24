using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Scripting.Components
{
    /// <summary>
    /// Use <see cref="ExecuteInlineScriptActorComponent"/> to execute a short inline script e.g. to assing some data to a variable or making an assertion , etc.
    /// </summary>
    [DataContract]
    [Serializable]
    [Scriptable("ScriptFile")]
    [Initializer(typeof(ScriptFileInitializer))]
    [ToolBoxItem("Script [Inline]", "Scripting", iconSource: null, description: "Assign value to a variable", tags: new string[] { "Assign", "Scripting" })]   
    public class ExecuteInlineScriptActorComponent : ActorComponent
    {
        protected string scriptFile;
        [DataMember]
        [DisplayName("Script File")]
        [ReadOnly(true)]
        public string ScriptFile
        {
            get => scriptFile;
            set
            {
                scriptFile = value;
                OnPropertyChanged();
            }
        }

        public ExecuteInlineScriptActorComponent() : base("Execute Inline Script", "ExecuteInlineScript")
        {           
        }

        public override async Task ActAsync()
        {
            IScriptEngine scriptExecutor = this.EntityManager.GetScriptEngine();
            _ = await scriptExecutor.ExecuteFileAsync(this.scriptFile);
        }
      
    }
}
