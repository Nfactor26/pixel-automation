using Dawn;
using Pixel.Automation.Core.Models;
using System;
using System.IO;

namespace Pixel.Automation.Core
{
    public class ApplicationFileSystem : IApplicationFileSystem
    {
        private readonly ApplicationSettings applicationSettings;

        public ApplicationFileSystem(ApplicationSettings applicationSettings)
        {
            Guard.Argument(applicationSettings).NotNull();
            this.applicationSettings = applicationSettings;
        }

        public string GetAutomationsDirectory()
        {
            return Path.Combine(Environment.CurrentDirectory, applicationSettings.AutomationDirectory);
        }

        public string GetAutomationProjectDirectory(AutomationProject automationProject)
        {
            return Path.Combine(Environment.CurrentDirectory, applicationSettings.AutomationDirectory, automationProject.ProjectId);
        }

        public string GetAutomationProjectWorkingDirectory(AutomationProject automationProject, VersionInfo versionInfo)
        {
            return Path.Combine(Environment.CurrentDirectory, applicationSettings.AutomationDirectory,
                automationProject.ProjectId, versionInfo.ToString());
        }

        public string GetAutomationProjectFile(AutomationProject automationProject)
        {
            return GetAutomationProjectFile(automationProject.ProjectId);
        }

        public string GetAutomationProjectFile(string projectId)
        {
            return Path.Combine(Environment.CurrentDirectory, applicationSettings.AutomationDirectory, projectId, $"{projectId}.atm");
        }

        public string GetApplicationPrefabsDirectory(string applicationId)
        {
            return Path.Combine(Environment.CurrentDirectory, applicationSettings.ApplicationDirectory, applicationId,
                Constants.PrefabsDirectory);
        }

        public string GetPrefabProjectDirectory(PrefabProject prefabProject)
        {
            return Path.Combine(Environment.CurrentDirectory, applicationSettings.ApplicationDirectory, prefabProject.ApplicationId,
                Constants.PrefabsDirectory, prefabProject.ProjectId);
        }

        public string GetPrefabProjectWorkingDirectory(PrefabProject prefabProject, VersionInfo versionInfo)
        {
            return Path.Combine(Environment.CurrentDirectory, applicationSettings.ApplicationDirectory, prefabProject.ApplicationId,
                Constants.PrefabsDirectory, prefabProject.ProjectId, versionInfo.ToString());
        }

        public string GetPrefabProjectWorkingDirectory(PrefabProject prefabProject, string versionInfo)
        {
            return Path.Combine(Environment.CurrentDirectory, applicationSettings.ApplicationDirectory, prefabProject.ApplicationId,
                Constants.PrefabsDirectory, prefabProject.ProjectId, versionInfo);
        }

        public string GetPrefabProjectFile(PrefabProject prefabProject)
        {
            return Path.Combine(Environment.CurrentDirectory, applicationSettings.ApplicationDirectory, prefabProject.ApplicationId,
                Constants.PrefabsDirectory, prefabProject.ProjectId, $"{prefabProject.ProjectId}.atm");
        }

        public string GetApplicationsDirectory()
        {
            return applicationSettings.ApplicationDirectory;
        }

        public string GetApplicationDirectory(ApplicationDescription application)
        {
            return Path.Combine(applicationSettings.ApplicationDirectory, application.ApplicationId);
        }

        public string GetApplicationFile(ApplicationDescription application)
        {
            return Path.Combine(applicationSettings.ApplicationDirectory, application.ApplicationId, $"{application.ApplicationId}.app");
        }

    }
}
