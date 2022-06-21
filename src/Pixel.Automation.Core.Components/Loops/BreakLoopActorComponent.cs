using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Interfaces;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading.Tasks;

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

        public override async Task ActAsync()
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
                throw new InvalidOperationException("BreakLoopActor must be added inside a loop construct such as for loop, while loop, etc.");
            }
            (currentParent as ILoop).ExitCriteriaSatisfied = true;
            await Task.CompletedTask;
        }
    }
}
