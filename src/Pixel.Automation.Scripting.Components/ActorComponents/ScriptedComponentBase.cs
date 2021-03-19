using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.Scripting.Components
{
    [DataContract]
    [Serializable]
    [Scriptable("ScriptFile")]
    [Initializer(typeof(ScriptFileInitializer))]
    public abstract class ScriptedComponentBase : AsyncActorComponent
    {
        protected string scriptFile;
        [DataMember]
        [Browsable(false)]
        public string ScriptFile
        {
            get => scriptFile;
            set => scriptFile = value;
        }      

        protected ScriptedComponentBase(string name,string tag) : base(name,tag)
        {

        }        
    }
}
