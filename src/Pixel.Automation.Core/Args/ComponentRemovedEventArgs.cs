using Pixel.Automation.Core.Interfaces;
using System;

namespace Pixel.Automation.Core
{
    /// <summary>
    /// During design time , whenever a component is deleted , a ComponentRemoveEventArgs message is broadcasted by the <see cref="IEventAggregator"/>
    /// </summary>
    public class ComponentRemovedEventArgs : EventArgs
    {
        /// <summary>
        /// Component which has been deleted
        /// </summary>
        public IComponent RemovedComponent { get; }
       
        /// <summary>
        /// Parent entity from which component has been deleted
        /// </summary>
        public Entity RemovedFromEntity { get; }

        public ComponentRemovedEventArgs(IComponent removedComponent, Entity removedFromEntity)
        {
            this.RemovedComponent = removedComponent;
            this.RemovedFromEntity = removedFromEntity;
        }

    }
}
