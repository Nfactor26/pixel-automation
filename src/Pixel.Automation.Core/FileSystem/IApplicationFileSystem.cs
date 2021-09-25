using Pixel.Automation.Core.Models;

namespace Pixel.Automation.Core
{
    public interface IApplicationFileSystem
    {
        string GetApplicationDirectory(ApplicationDescription application);
        string GetApplicationFile(ApplicationDescription application);
        string GetApplicationsDirectory();
        string GetAutomationProjectDirectory(AutomationProject automationProject);
        string GetAutomationProjectFile(AutomationProject automationProject);
        string GetAutomationProjectFile(string projectId);
        string GetAutomationProjectWorkingDirectory(AutomationProject automationProject, VersionInfo versionInfo);
        string GetAutomationsDirectory();
        string GetApplicationPrefabsDirectory(string applicationId);
        string GetPrefabProjectDirectory(PrefabProject prefabProject);
        string GetPrefabProjectFile(PrefabProject prefabProject);
        string GetPrefabProjectWorkingDirectory(PrefabProject prefabProject, string versionInfo);
        string GetPrefabProjectWorkingDirectory(PrefabProject prefabProject, VersionInfo versionInfo);
      
    }
}