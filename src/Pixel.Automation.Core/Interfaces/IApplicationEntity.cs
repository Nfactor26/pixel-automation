namespace Pixel.Automation.Core.Interfaces
{
    public interface IApplicationEntity
    {
        string ApplicationFile { get; set; }
 
        string ApplicationId { get; set; }

        IApplication GetTargetApplicationDetails();

        void SetTargetApplicationDetails(IApplication applicationDetails);

        T GetTargetApplicationDetails<T>() where T : class, IApplication;

        /// <summary>
        /// Reload application details. This is required so that editor can reload application details
        /// in reponse to a change in applcation details from application explorer
        /// </summary>
        void Reload();

        /// <summary>
        /// Indicates whether an existing process can be used as target application for automation.
        /// </summary>
        bool CanUseExisting { get; }

        /// <summary>
        /// Launch the Application
        /// </summary>
        void Launch();

        /// <summary>
        /// Close the Application
        /// </summary>
        void Close();

        /// <summary>
        /// Use an existing application which might already be launched.
        /// </summary>
        /// <param name="targetApplication">Identified application instance</param>
        void UseExisting(ApplicationProcess targetApplication);
    }
}