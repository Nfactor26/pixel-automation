using System.Threading.Tasks;

namespace Pixel.Automation.Core.Interfaces
{
    public interface IApplicationEntity
    {
        /// <summary>
        /// Path to application file that contains the details of target application
        /// </summary>
        string ApplicationFile { get; set; }
 
        /// <summary>
        /// Unique application identifier of the target application
        /// </summary>
        string ApplicationId { get; set; }

        /// <summary>
        /// Indicates whether an existing process can be used as target application for automation.
        /// </summary>
        bool CanUseExisting { get; }

        /// <summary>
        /// Get the details of target application
        /// </summary>
        /// <returns><see cref="IApplication"/></returns>
        IApplication GetTargetApplicationDetails();

        /// <summary>
        /// Set the target application details on application entity
        /// </summary>
        /// <param name="applicationDetails"></param>
        void SetTargetApplicationDetails(IApplication applicationDetails);

        /// <summary>
        /// Get the details of target application as T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetTargetApplicationDetails<T>() where T : class, IApplication;

        /// <summary>
        /// Reload application details. This is required so that editor can reload application details
        /// in reponse to a change in applcation details from application explorer
        /// </summary>
        void Reload();

        /// <summary>
        /// Launch the Application
        /// </summary>
        Task LaunchAsync();

        /// <summary>
        /// Close the Application
        /// </summary>
        Task CloseAsync();        

        /// <summary>
        /// Use an existing application which might already be launched.
        /// </summary>
        /// <param name="targetApplication">Identified application instance</param>
        void UseExisting(ApplicationProcess targetApplication);

        /// <summary>
        /// Capture screen shot of the application window
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        Task CaptureScreenShotAsync(string filePath);
    }
}