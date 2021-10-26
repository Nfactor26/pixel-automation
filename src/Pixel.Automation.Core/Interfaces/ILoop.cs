namespace Pixel.Automation.Core.Interfaces
{
    /// <summary>
    /// A component that supports looping over it's child components
    /// e.g. while loop component, for loop component, etc.
    /// </summary>
    public interface ILoop : IComponent
    {
        /// <summary>
        /// Indicates if the terminating condition of loop is satisfied
        /// </summary>
        bool ExitCriteriaSatisfied
        {
            get;set;
        }  
      
    }
}
