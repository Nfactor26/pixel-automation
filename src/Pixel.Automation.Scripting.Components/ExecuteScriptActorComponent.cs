using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Scripting.Components
{
    /// <summary>
    /// Use <see cref="ExecuteScriptActorComponent"/> to execute actions using custom script.
    /// </summary>
    [DataContract]
    [Serializable]
    [Scriptable("ScriptFile")]
    [Initializer(typeof(ScriptFileInitializer))]
    [ToolBoxItem("Script File", "Scripting", iconSource: null, description: "Execute any provided script", tags: new string[] { "Scripted Action", "Scripting" })]   
    public class ExecuteScriptActorComponent : ActorComponent
    {
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

        public ExecuteScriptActorComponent() : base("Execute Script", "ExecuteScript")
        {

        }

        public override async Task ActAsync()
        {
            IScriptEngine scriptEngine = this.EntityManager.GetScriptEngine();
            var action = await scriptEngine.CreateDelegateAsync<Action<IApplication, IComponent>>(this.scriptFile);
            action(this.EntityManager.GetOwnerApplication(this), this);
        }
     
    }
}
