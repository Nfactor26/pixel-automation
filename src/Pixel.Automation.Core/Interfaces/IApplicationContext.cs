namespace Pixel.Automation.Core.Interfaces
{
    /// <summary>
    /// An entity implementing this interface creates an application scope for its children
    /// </summary>
    public interface IApplicationContext
    {
        /// <summary>
        /// Set the application id for which scope is created
        /// </summary>
        /// <param name="targetAppId"></param>
        void SetAppContext(string targetAppId);

        /// <summary>
        /// Get the application id for this scope
        /// </summary>
        /// <returns></returns>
        string GetAppContext();
    }
}
