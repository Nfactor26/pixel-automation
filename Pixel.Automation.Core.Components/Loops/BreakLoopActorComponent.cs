using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Components.Loops
{
    [DataContract]
    [Serializable]
    [ToolBoxItem("Break", "Loops", iconSource: null, description: "Break the execution of loop", tags: new string[] { "Break", "Loop" })]
    [Description("Set the ExitCriteriaSatisfied property of the first ancestor ILoopComponent to true in order to stop any further iteration by loop")]
    public class BreakLoopActorComponent : ActorComponent
    {   

        public BreakLoopActorComponent() : base("Break","BreakLoop")
        {

        }

        public override void Act()
        {
            Entity currentParent = this.Parent;
            while (currentParent != null)
            {
                if (currentParent is ILoop)
                {
                    break;
                }
                currentParent = currentParent.Parent;
            }
            if (currentParent == null)
            {
                throw new InvalidOperationException("Decision Evaluator must be a child of Entity that implemnets IConditional or ILoop interface");
            }
            (currentParent as ILoop).ExitCriteriaSatisfied = true;
        }
    }
}
