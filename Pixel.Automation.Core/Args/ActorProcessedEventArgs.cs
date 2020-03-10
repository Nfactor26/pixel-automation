using Pixel.Automation.Core.Interfaces;
using System;

namespace Pixel.Automation.Core
{
    /// <summary>
    /// Whenver an actor is processed by the processor component , ActorProcessEventArgs message will be broadcasted by the <see cref="IEventAggregator"/>
    /// </summary>
    public class ActorProcessedEventArgs : EventArgs
    {
        /// <summary>
        /// Component which has been processed
        /// </summary>
        public IComponent ProcessedActor { get; }
       
        /// <summary>
        /// Indicates whether the component was successfully processed
        /// </summary>
        public bool IsSuccess { get; }

        public ActorProcessedEventArgs(ActorComponent processedActor,bool isSuccess)
        {
            this.IsSuccess = isSuccess;
            this.ProcessedActor = processedActor;
        }
    }
}
