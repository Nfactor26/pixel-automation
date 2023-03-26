using System.Threading.Tasks;

namespace Pixel.Automation.Core.Interfaces
{
    /// <summary>
    /// Interface to be implemented by processors.
    /// A processor is responsible for iterating through its child components and processing them 
    /// </summary>
    public interface IEntityProcessor : IComponent
    {
        /// <summary>
        /// Configure post delay amount that should be introduced 
        /// after executiong of each actor.
        /// </summary>       
        /// <param name="postDelayAmount"></param>
        void ConfigureDelay(int postDelayAmount);

        /// <summary>
        /// Reset post delay values to 0
        /// </summary>
        void ResetDelay();

        /// <summary>
        /// Initiate processing of child components
        /// </summary>
        /// <returns></returns>
        Task BeginProcessAsync();       

        /// <summary>
        /// Call Rest on child components 
        /// </summary>
        void ResetComponents();

    }
}
