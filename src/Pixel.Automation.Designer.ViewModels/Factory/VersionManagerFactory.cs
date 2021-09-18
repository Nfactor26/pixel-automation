using Dawn;
using Pixel.Automation.AppExplorer.ViewModels.Prefab;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Designer.ViewModels.VersionManager;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client;
using Pixel.Scripting.Editor.Core.Contracts;

namespace Pixel.Automation.Designer.ViewModels.Factory
{
    public class VersionManagerFactory : IVersionManagerFactory
    {
        private readonly IWorkspaceManagerFactory workspaceManagerFactory;
        private readonly ISerializer serializer;
        private readonly IApplicationDataManager applicationDataManager;
        private readonly ApplicationSettings applicationSettings;

        public VersionManagerFactory(ISerializer serializer, IWorkspaceManagerFactory workspaceManagerFactory, IApplicationDataManager applicationDataManager,
           ApplicationSettings applicationSettings)
        {
            this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
            this.workspaceManagerFactory = Guard.Argument(workspaceManagerFactory, nameof(workspaceManagerFactory)).NotNull().Value;
            this.applicationDataManager = Guard.Argument(applicationDataManager, nameof(applicationDataManager)).NotNull().Value;
            //this.projectFileSystem = Guard.Argument(projectFileSystem, nameof(projectFileSystem)).NotNull().Value;
            this.applicationSettings = Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
        }

        public IVersionManager CreatePrefabVersionManager(PrefabProject prefabProject)
        {
            return new PrefabVersionManagerViewModel(prefabProject, this.workspaceManagerFactory, this.serializer, this.applicationDataManager, this.applicationSettings);
        }

        public IVersionManager CreateProjectVersionManager(AutomationProject automationProject)
        {
            return new ProjectVersionManagerViewModel(automationProject, this.workspaceManagerFactory, this.serializer, this.applicationDataManager, this.applicationSettings);
        }

        public IVersionManager CreatePrefabReferenceManager(IFileSystem projectFileSystem)
        {
            return new PrefabReferenceManagerViewModel(projectFileSystem, this.applicationDataManager);
        }
    }
}
