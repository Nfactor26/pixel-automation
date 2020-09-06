namespace Pixel.Automation.Core.Interfaces
{
    public interface IApplicationEntity
    {
        string ApplicationFile { get; set; }
 
        string ApplicationId { get; set; }

        IApplication GetTargetApplicationDetails();

        void SetTargetApplicationDetails(IApplication applicationDetails);

        T GetTargetApplicationDetails<T>() where T : class, IApplication;
    }
}