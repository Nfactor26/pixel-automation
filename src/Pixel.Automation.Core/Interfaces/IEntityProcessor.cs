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
        /// Configure the pre and post delay amount that should be introduced 
        /// before and after executiong of each actor respectively.
        /// </summary>
        /// <param name="preDelayAmount"></param>
        /// <param name="postDelayAmount"></param>
        void ConfigureDelay(int preDelayAmount, int postDelayAmount);

        /// <summary>
        /// Reset pre and post delay values to 0
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
