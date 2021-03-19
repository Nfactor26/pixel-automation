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
    }
}