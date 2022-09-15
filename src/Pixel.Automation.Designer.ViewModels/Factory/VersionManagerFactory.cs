﻿using Dawn;
using Pixel.Automation.AppExplorer.ViewModels.Prefab;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Designer.ViewModels.VersionManager;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client;
using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Scripting.Reference.Manager.Contracts;

namespace Pixel.Automation.Designer.ViewModels.Factory
{
    public class VersionManagerFactory : IVersionManagerFactory
    {
        private readonly IWorkspaceManagerFactory workspaceManagerFactory;
        private readonly IReferenceManagerFactory referenceManagerFactory;
        private readonly ISerializer serializer;
        private readonly IApplicationDataManager applicationDataManager;
        private readonly ApplicationSettings applicationSettings;

        public VersionManagerFactory(ISerializer serializer, IWorkspaceManagerFactory workspaceManagerFactory, IReferenceManagerFactory referenceManagerFactory,
            IApplicationDataManager applicationDataManager, ApplicationSettings applicationSettings)
        {
            this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
            this.workspaceManagerFactory = Guard.Argument(workspaceManagerFactory, nameof(workspaceManagerFactory)).NotNull().Value;
            this.referenceManagerFactory = Guard.Argument(referenceManagerFactory, nameof(referenceManagerFactory)).NotNull().Value;
            this.applicationDataManager = Guard.Argument(applicationDataManager, nameof(applicationDataManager)).NotNull().Value;       
            this.applicationSettings = Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
        }

        public IVersionManager CreatePrefabVersionManager(PrefabProject prefabProject)
        {          
            return new PrefabVersionManagerViewModel(prefabProject, this.workspaceManagerFactory, this.referenceManagerFactory, this.serializer, this.applicationDataManager, this.applicationSettings);
        }

        public IVersionManager CreateProjectVersionManager(AutomationProject automationProject)
        {           
            return new ProjectVersionManagerViewModel(automationProject, this.workspaceManagerFactory, this.referenceManagerFactory, this.serializer, this.applicationDataManager, this.applicationSettings);
        }

        public IVersionManager CreatePrefabReferenceManager(IProjectFileSystem projectFileSystem)
        {
            return new PrefabReferenceManagerViewModel(projectFileSystem, this.applicationDataManager);
        }

        public IVersionManager CreateControlReferenceManager(IFileSystem fileSystem)
        {
            return new ControlReferenceManagerViewModel(fileSystem, this.applicationDataManager);
        }

        public IVersionManager CreateAssemblyReferenceManager(IFileSystem fileSystem)
        {
            return new AssemblyReferenceManagerViewModel(fileSystem, this.serializer);
        }
    }
}
