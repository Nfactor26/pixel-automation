namespace Pixel.Automation.Core.Interfaces
{
    public interface IApplicationEntity
    {
        string ApplicationFile { get; set; }
 
        string ApplicationId { get; set; }

        IApplication GetTargetApplicationDetails();

        T GetTargetApplicationDetails<T>() where T : class, IApplication;
    }
}