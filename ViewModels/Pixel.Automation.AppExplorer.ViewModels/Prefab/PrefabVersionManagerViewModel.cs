using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Pixel.Automation.AppExplorer.ViewModels.Prefab
{
    public class PrefabVersionManagerViewModel : Screen , IVersionManager
    {
        private readonly ILogger logger = Log.ForContext<PrefabVersionManagerViewModel>();

        private readonly IWorkspaceManagerFactory workspaceManagerFactory;
        private readonly ISerializer serializer;
        private readonly IApplicationDataManager applicationDataManager;
        private readonly PrefabDescription prefabDescription;
        private readonly ApplicationSettings applicationSettings;

        public BindableCollection<PrefabVersionViewModel> AvailableVersions { get; set; } = new BindableCollection<PrefabVersionViewModel>();

        private PrefabVersionViewModel selectedVersion;
        public PrefabVersionViewModel SelectedVersion
        {
            get => this.selectedVersion;
            set
            {
                this.selectedVersion = value;
                NotifyOfPropertyChange(() => CanDeploy);
            }
        }


        public PrefabVersionManagerViewModel(PrefabDescription prefabDescription, IWorkspaceManagerFactory workspaceManagerFactory, 
            ISerializer serializer, IApplicationDataManager applicationDataManager,
            ApplicationSettings applicationSettings)
        {
            this.DisplayName = "Manage & Deploy Versions";
            this.workspaceManagerFactory = workspaceManagerFactory;
            this.serializer = serializer;
            this.applicationDataManager = applicationDataManager;
            this.prefabDescription = prefabDescription;
            this.applicationSettings = applicationSettings;
            foreach (var version in this.prefabDescription.AvailableVersions)
            {
                IPrefabFileSystem fileSystem = new PrefabFileSystem(serializer, applicationSettings);
                AvailableVersions.Add(new PrefabVersionViewModel(this.prefabDescription, version, fileSystem));
            }
        }

        public bool CanDeploy
        {
            get
            {
                return this.SelectedVersion?.IsActive ?? false;
            }

        }

        /// <summary>
        /// Create a copy of selected version and deploy the selected version.
        /// </summary>
        /// <returns></returns>
        public async Task Deploy()
        {
            try
            {
                if (this.SelectedVersion?.IsActive == true)
                {
                    //Create a new active version from selected version
                    PrefabVersion newVersion = this.SelectedVersion.Clone();

                    IPrefabFileSystem fileSystem = new PrefabFileSystem(serializer, applicationSettings);
                    fileSystem.Initialize(this.prefabDescription.ApplicationId, this.prefabDescription.PrefabId, newVersion);
                    AvailableVersions.Add(new PrefabVersionViewModel(this.prefabDescription, newVersion, fileSystem));

                    //Deploy the selected version
                    this.SelectedVersion.Deploy(this.workspaceManagerFactory);

                    this.prefabDescription.AvailableVersions.Add(newVersion);
                    serializer.Serialize<PrefabDescription>(fileSystem.PrefabDescriptionFile, this.prefabDescription);

                    await this.applicationDataManager.AddOrUpdatePrefabDescriptionAsync(this.prefabDescription, new PrefabVersion(this.SelectedVersion.Version));
                    await this.applicationDataManager.AddOrUpdatePrefabDataFilesAsync(this.prefabDescription, new PrefabVersion(this.SelectedVersion.Version));
                    await this.applicationDataManager.AddOrUpdatePrefabDataFilesAsync(this.prefabDescription, newVersion);
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }
    }
}
