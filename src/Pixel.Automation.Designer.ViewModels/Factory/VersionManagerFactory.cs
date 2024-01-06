using Caliburn.Micro;
using Dawn;
using Notifications.Wpf.Core;
using Pixel.Automation.AppExplorer.ViewModels.Prefab;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Designer.ViewModels.VersionManager;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Reference.Manager.Contracts;
using Pixel.Persistence.Services.Client;
using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;

namespace Pixel.Automation.Designer.ViewModels.Factory
{
    public class VersionManagerFactory : IVersionManagerFactory
    {
        private readonly IDataManagerFactory projectAssetsDataManagerFactory;
        private readonly IWorkspaceManagerFactory workspaceManagerFactory;
        private readonly IReferenceManagerFactory referenceManagerFactory;
        private readonly ISerializer serializer;
        private readonly IApplicationDataManager applicationDataManager;
        private readonly IPrefabDataManager prefabDataManager;
        private readonly IProjectDataManager projectDataManager;
        private readonly INotificationManager notificationManager;
        private readonly IEventAggregator eventAggregator;
        private readonly IApplicationFileSystem applicationFileSystem;
        private readonly ApplicationSettings applicationSettings;

        public VersionManagerFactory(ISerializer serializer, IWorkspaceManagerFactory workspaceManagerFactory, 
            IReferenceManagerFactory referenceManagerFactory, IApplicationDataManager applicationDataManager,
            IPrefabDataManager prefabDataManager, IProjectDataManager projectDataManager, IDataManagerFactory projectAssetsDataManagerFactory,
            IEventAggregator eventAggregator, INotificationManager notificationManager, ApplicationSettings applicationSettings, IApplicationFileSystem applicationFileSystem)
        {
            this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
            this.workspaceManagerFactory = Guard.Argument(workspaceManagerFactory, nameof(workspaceManagerFactory)).NotNull().Value;
            this.referenceManagerFactory = Guard.Argument(referenceManagerFactory, nameof(referenceManagerFactory)).NotNull().Value;
            this.applicationDataManager = Guard.Argument(applicationDataManager, nameof(applicationDataManager)).NotNull().Value;
            this.projectDataManager = Guard.Argument(projectDataManager, nameof(projectDataManager)).NotNull().Value;
            this.projectAssetsDataManagerFactory = Guard.Argument(projectAssetsDataManagerFactory, nameof(projectAssetsDataManagerFactory)).NotNull().Value;
            this.prefabDataManager = Guard.Argument(prefabDataManager, nameof(prefabDataManager)).NotNull().Value;
            this.eventAggregator = Guard.Argument(eventAggregator, nameof(eventAggregator)).NotNull().Value;
            this.notificationManager = Guard.Argument(notificationManager, nameof(notificationManager)).NotNull().Value;
            this.applicationSettings = Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
            this.applicationFileSystem = Guard.Argument(applicationFileSystem, nameof(applicationFileSystem)).NotNull().Value;
        }

        public IVersionManager CreatePrefabVersionManager(PrefabProject prefabProject)
        {          
            return new PrefabVersionManagerViewModel(prefabProject, this.workspaceManagerFactory, this.referenceManagerFactory, this.serializer, this.prefabDataManager,
               this.eventAggregator, this.notificationManager, this.applicationSettings);
        }

        public IVersionManager CreateProjectVersionManager(AutomationProject automationProject)
        {           
            return new ProjectVersionManagerViewModel(automationProject, this.workspaceManagerFactory, this.referenceManagerFactory, 
                this.applicationDataManager, this.projectDataManager, this.projectAssetsDataManagerFactory, this.prefabDataManager, this.serializer, applicationFileSystem,
               this.eventAggregator, this.notificationManager, this.applicationSettings);
        }

        public IVersionManager CreatePrefabReferenceManager(IReferenceManager referenceManager, Action<IEnumerable<PrefabReference>> onVersionChanged)
        {
            return new PrefabReferenceManagerViewModel(this.prefabDataManager, referenceManager, this.notificationManager, onVersionChanged);
        }

        public IVersionManager CreateControlReferenceManager(IReferenceManager referenceManager)
        {
            return new ControlReferenceManagerViewModel(this.applicationDataManager, referenceManager, this.notificationManager);
        }

        public IVersionManager CreateAssemblyReferenceManager(IFileSystem fileSystem, IReferenceManager referenceManager)
        {
            return new AssemblyReferenceManagerViewModel(fileSystem, this.serializer, referenceManager, this.notificationManager);
        }
    }
}
