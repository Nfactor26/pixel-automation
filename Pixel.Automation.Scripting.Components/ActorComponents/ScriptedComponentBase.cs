using Pixel.Automation.Core;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.Scripting.Components
{
    [DataContract]
    [Serializable]
    public abstract class ScriptedComponentBase : ActorComponent
    {
        protected string scriptFile = $"{Guid.NewGuid().ToString()}.csx";
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
