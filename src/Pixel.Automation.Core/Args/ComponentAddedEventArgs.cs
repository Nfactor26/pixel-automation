using Pixel.Automation.Core.Interfaces;
using System;

namespace Pixel.Automation.Core
{
    /// <summary>
    /// During process configuration , whenever a component is added , ComponentAddedEventArgs message will be broadcasted by the <see cref="IEventAggregator"/>
    /// </summary>
    public class ComponentAddedEventArgs : EventArgs
    {
        /// <summary>
        /// Component which has been added
        /// </summary>
        public IComponent AddedComponent { get; }
       
        /// <summary>
        /// Entity to which component has been added as a child
        /// </summary>
        public Entity AddedToEntity { get; }

        public ComponentAddedEventArgs(IComponent addedComponent, Entity addedToEntity)
        {
            this.AddedComponent = addedComponent;
            this.AddedToEntity = addedToEntity;
        }

    }
}
