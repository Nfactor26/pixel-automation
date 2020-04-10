using System.Threading.Tasks;

namespace Pixel.Automation.Core.Interfaces
{
    /// <summary>
    /// Interface to be implemented by processors.
    /// A processor is responsible for iterating through its child components and processing them 
    /// </summary>
    public interface IEntityProcessor
    {       
        /// <summary>
        /// Initiate processing of child components
        /// </summary>
        /// <returns></returns>
        Task BeginProcess();

        /// <summary>
        /// Call Rest on child components 
        /// </summary>
        void ResetComponents();

    }
}
