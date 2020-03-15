using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Coree.Components.Decisions
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("If", "Decisions", iconSource: null, description: "Wraps a group of automation entity that are processed only if the if criteria is satisfied ", tags: new string[] { "Deicsion", "DecisionGroup", "Entity" })]
    public class IfEntity : Entity
    {
        protected string scriptFile = $"{Guid.NewGuid().ToString()}.csx";
        [DataMember]
        [Browsable(false)]
        public string ScriptFile
        {
            get => scriptFile;
            set => scriptFile = value;
        }

        public IfEntity() : base("If", "IfEntity")
        {           

        }     

        public override IEnumerable<Core.Interfaces.IComponent> GetNextComponentToProcess()
        {

            ScriptResult scriptResult = ExecuteScript().Result;

            IEnumerator<Core.Interfaces.IComponent> conditionalBlockIterator = default;
            switch((bool)scriptResult.ReturnValue)
            {
                case true:
                    Entity thenBlock = this.GetComponentsByName("Then").Single() as Entity;
                    conditionalBlockIterator = thenBlock.GetNextComponentToProcess().GetEnumerator();                  
                    break;
                case false:
                    Entity elseBlock = this.GetComponentsByName("Else").Single() as Entity;
                    conditionalBlockIterator = elseBlock.GetNextComponentToProcess().GetEnumerator();
                    break;
            }

            while (conditionalBlockIterator.MoveNext())
            {
                yield return conditionalBlockIterator.Current;
            }         
        }

        private async Task<ScriptResult> ExecuteScript()
        {
            IScriptEngine scriptExecutor = this.EntityManager.GetServiceOfType<IScriptEngine>();
            ScriptResult result = await scriptExecutor.ExecuteFileAsync(this.scriptFile);        
            return result;          
        }

        public override void ResetComponent()
        {
            base.ResetComponent();           
        }

        public override void ResolveDependencies()
        {
            if (this.Components.Count() > 0)
                return;

            PlaceHolderEntity thenBlock = new PlaceHolderEntity("Then");
            PlaceHolderEntity elseBlock = new PlaceHolderEntity("Else");  
                    
            this.AddComponent(thenBlock);
            this.AddComponent(elseBlock);          
        }


    }
}
