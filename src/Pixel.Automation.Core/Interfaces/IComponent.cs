using System.Threading.Tasks;

namespace Pixel.Automation.Core.Interfaces
{
    /// <summary>
    /// Interface for a component 
    /// </summary>
    public interface IComponent
    {      

        /// <summary>
        /// Parent Entity of the component
        /// </summary>
        Entity Parent
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the EntityManager for the component
        /// </summary>
        IEntityManager EntityManager
        {
            get;
            set;
        }


        /// <summary>
        /// Unique Id of the component
        /// </summary>
        string Id
        {
            get;           
        }

        /// <summary>
        /// Name assigned to this component
        /// </summary>
        string Name
        {
            get;
            set;
        }

        /// <summary>
        /// A tag that can be used to identify this component
        /// </summary>
        string Tag
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether this component is enabled or not. A disabled component is not processed.
        /// </summary>
        bool IsEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates the order  amongst its sibling components at which this will be executed
        /// </summary>
        int ProcessOrder
        {
            get;
            set;
        }

        /// <summary>
        /// Processor will call BeforeProcess on  a component before processing it
        /// </summary>
        Task BeforeProcessAsync();

        /// <summary>
        /// Processor will call OnCompletion on a component after processing it
        /// </summary>
        Task OnCompletionAsync();

        /// <summary>
        /// Processor will call OnFault on all entities that are being processed with the faulting component
        /// </summary>
        Task OnFaultAsync(IComponent faultingComponent);

        /// <summary>
        /// Validate if the component is configured properly.
        /// </summary>
        /// <returns></returns>
        bool ValidateComponent();

        /// <summary>
        /// Reset any changes made to component during automation so that it can be reused again 
        /// </summary>
        /// <returns></returns>
        void ResetComponent();

        /// <summary>
        /// Reset the id of the component to a new value
        /// </summary>
        //void ResetId();

        /// <summary>
        /// Add any required components at appropriate places in the entity component hierarchy
        /// </summary>
        void ResolveDependencies();
    }
}
