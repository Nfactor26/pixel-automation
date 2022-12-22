﻿using Dawn;
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
        private readonly IWorkspaceManagerFactory workspaceManagerFactory;
        private readonly IReferenceManagerFactory referenceManagerFactory;
        private readonly ISerializer serializer;
        private readonly IApplicationDataManager applicationDataManager;
        private readonly IPrefabDataManager prefabDataManager;
        private readonly IProjectDataManager projectDataManager;
        private readonly INotificationManager notificationManager;
        private readonly ApplicationSettings applicationSettings;

        public VersionManagerFactory(ISerializer serializer, IWorkspaceManagerFactory workspaceManagerFactory, 
            IReferenceManagerFactory referenceManagerFactory, IApplicationDataManager applicationDataManager,
            IPrefabDataManager prefabDataManager, IProjectDataManager projectDataManager,
            INotificationManager notificationManager, ApplicationSettings applicationSettings)
        {
            this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
            this.workspaceManagerFactory = Guard.Argument(workspaceManagerFactory, nameof(workspaceManagerFactory)).NotNull().Value;
            this.referenceManagerFactory = Guard.Argument(referenceManagerFactory, nameof(referenceManagerFactory)).NotNull().Value;
            this.applicationDataManager = Guard.Argument(applicationDataManager, nameof(applicationDataManager)).NotNull().Value;
            this.projectDataManager = Guard.Argument(projectDataManager, nameof(projectDataManager)).NotNull().Value;
            this.prefabDataManager = Guard.Argument(prefabDataManager, nameof(prefabDataManager)).NotNull().Value;
            this.notificationManager = Guard.Argument(notificationManager, nameof(notificationManager)).NotNull().Value;
            this.applicationSettings = Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
        }

        public IVersionManager CreatePrefabVersionManager(PrefabProject prefabProject)
        {          
            return new PrefabVersionManagerViewModel(prefabProject, this.workspaceManagerFactory, this.referenceManagerFactory, this.serializer, this.prefabDataManager,
                this.notificationManager, this.applicationSettings);
        }

        public IVersionManager CreateProjectVersionManager(AutomationProject automationProject)
        {           
            return new ProjectVersionManagerViewModel(automationProject, this.workspaceManagerFactory, this.referenceManagerFactory, this.serializer, this.projectDataManager, 
                this.notificationManager, this.applicationSettings);
        }

        public IVersionManager CreatePrefabReferenceManager(IReferenceManager referenceManager)
        {
            return new PrefabReferenceManagerViewModel(this.prefabDataManager, referenceManager, this.notificationManager);
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
