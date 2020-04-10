﻿using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Scripting.Components
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Execute [Editor]", "Scripting", iconSource: null, description: "Execute any provided script", tags: new string[] { "Scripted Action", "Scripting" })]
    [Scriptable("ScriptFile")]
    public class ScriptedActionActorComponent : ScriptedComponentBase
    {

        public ScriptedActionActorComponent() : base("Scripted Action", "ScriptedAction")
        {

        }

        public override async Task ActAsync()
        {
            IScriptEngine scriptEngine = this.EntityManager.GetServiceOfType<IScriptEngine>();
            var action = await scriptEngine.CreateDelegateAsync<Action<IApplication, IComponent>>(this.scriptFile);
            action(this.EntityManager.GetApplicationDetails(this), this);
        }
     
    }
}
