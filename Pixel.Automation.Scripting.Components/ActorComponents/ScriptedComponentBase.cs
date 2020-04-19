using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;

namespace Pixel.Automation.Scripting.Components
{
    [DataContract]
    [Serializable]
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

        public override void ResolveDependencies()
        {
            var fileSystem = this.EntityManager.GetServiceOfType<IFileSystem>();
            this.scriptFile = Path.GetRelativePath(fileSystem.WorkingDirectory,  Path.Combine(fileSystem.ScriptsDirectory, $"{Guid.NewGuid().ToString()}.csx"));
        }
    }
}
