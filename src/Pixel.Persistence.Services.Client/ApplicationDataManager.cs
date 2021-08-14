using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Persistence.Core.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public class ApplicationDataManager : IApplicationDataManager
    {
        private readonly ILogger logger = Log.ForContext<ApplicationDataManager>();

        private readonly string controlsDirectory = "Controls";
        private readonly string prefabsDirectory = "Prefabs";
      

        private readonly IApplicationRepositoryClient appRepositoryClient;
        private readonly IControlRepositoryClient controlRepositoryClient;
        private readonly IProjectRepositoryClient projectRepositoryClient;
        private readonly IPrefabRepositoryClient prefabRepositoryClient;
        private readonly IMetaDataClient metaDataClient;
        private readonly ISerializer serializer;
        private readonly ITypeProvider typeProvider;
        private readonly ApplicationSettings applicationSettings;

        bool IsOnlineMode
        {
            get => !this.applicationSettings.IsOfflineMode;
        }

        #region Constructor

        public ApplicationDataManager(ISerializer serializer, ITypeProvider typeProvider, IMetaDataClient metaDataClient,
            IApplicationRepositoryClient appRepositoryClient, IControlRepositoryClient controlRepositoryClient,
            IProjectRepositoryClient projectRepositoryClient, IPrefabRepositoryClient prefabRepositoryClient, 
            ApplicationSettings applicationSettings)
        {
            this.serializer = Guard.Argument(serializer).NotNull().Value;
            this.typeProvider = Guard.Argument(typeProvider).NotNull().Value;
            this.metaDataClient = Guard.Argument(metaDataClient).NotNull().Value;
            this.appRepositoryClient = Guard.Argument(appRepositoryClient).NotNull().Value;
            this.controlRepositoryClient = Guard.Argument(controlRepositoryClient).NotNull().Value;
            this.projectRepositoryClient = Guard.Argument(projectRepositoryClient).NotNull().Value;
            this.prefabRepositoryClient = Guard.Argument(prefabRepositoryClient).NotNull().Value;
            this.applicationSettings = Guard.Argument(applicationSettings).NotNull();

            CreateLocalDataDirectories();

        }

        void CreateLocalDataDirectories()
        {
            if (!Directory.Exists(applicationSettings.ApplicationDirectory))
            {
                Directory.CreateDirectory(applicationSettings.ApplicationDirectory);
            }

            if (!Directory.Exists(applicationSettings.AutomationDirectory))
            {
                Directory.CreateDirectory(applicationSettings.AutomationDirectory);
            }
        }

        #endregion Constructor

        public void CleanLocalData()
        {
            if (Directory.Exists(applicationSettings.ApplicationDirectory))
            {
                Directory.Delete(applicationSettings.ApplicationDirectory, true);
                logger.Information($"Delted local application data directory {applicationSettings.ApplicationDirectory}.");
            }

            if (Directory.Exists(applicationSettings.AutomationDirectory))
            {
                Directory.Delete(applicationSettings.AutomationDirectory, true);
                logger.Information($"Delte local automation data directory {applicationSettings.AutomationDirectory}.");
            }

            CreateLocalDataDirectories();
        }

        #region Applications 


        /// <summary>
        /// Get all the applications available on disk
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ApplicationDescription> GetAllApplications()
        {
            foreach (var app in Directory.GetDirectories(this.applicationSettings.ApplicationDirectory))
            {
                string appFile = Directory.GetFiles(Path.Combine(this.applicationSettings.ApplicationDirectory, new DirectoryInfo(app).Name), "*.app", SearchOption.TopDirectoryOnly).FirstOrDefault();
                ApplicationDescription application = serializer.Deserialize<ApplicationDescription>(appFile);
                yield return application;
            }
            yield break;
        }


        /// <summary>
        /// Saves a new application to database
        /// </summary>
        /// <param name="applicationDescription"></param>
        public async Task AddOrUpdateApplicationAsync(ApplicationDescription applicationDescription)
        {
            Directory.CreateDirectory(Path.Combine(applicationSettings.ApplicationDirectory, applicationDescription.ApplicationId));
            Directory.CreateDirectory(Path.Combine(applicationSettings.ApplicationDirectory, applicationDescription.ApplicationId, controlsDirectory));
            Directory.CreateDirectory(Path.Combine(applicationSettings.ApplicationDirectory, applicationDescription.ApplicationId, prefabsDirectory));

            SaveApplicationToDisk(applicationDescription);
            
            if(IsOnlineMode)
            {
                await this.appRepositoryClient.AddOrUpdateApplication(applicationDescription);
            }
            
        }

        /// <summary>
        /// Download application data from database which are not already available on disk.
        /// Application data includes controls and prefabs belonging to an application.
        /// </summary>
        /// <returns></returns>
        public async Task DownloadApplicationsDataAsync()
        {
            if (IsOnlineMode)
            {
                var serverApplicationMetaDataCollection = await this.metaDataClient.GetApplicationMetaData();
                var localApplicationMetaDataCollection = GetLocalApplicationMetaData();
                foreach (var serverApplicationMetaData in serverApplicationMetaDataCollection)
                {
                    //download application file if there is a newer version available or data doesn't already exist locally
                    bool isNewerApplicationVersionAvailable = localApplicationMetaDataCollection.Any(a => a.ApplicationId.Equals(serverApplicationMetaData.ApplicationId) && (a.LastUpdated < serverApplicationMetaData.LastUpdated));
                    bool isApplicationNotAvailableLocally = !localApplicationMetaDataCollection.Any(a => a.ApplicationId.Equals(serverApplicationMetaData.ApplicationId));
                    if (isNewerApplicationVersionAvailable || isApplicationNotAvailableLocally)
                    {
                        var applicationDescription = await this.appRepositoryClient.GetApplication(serverApplicationMetaData.ApplicationId);
                        SaveApplicationToDisk(applicationDescription);
                        logger.Information("Updated local copy of Application : {applicationName} with Id {applicationId}.", applicationDescription.ApplicationName, applicationDescription.ApplicationId);
                    }

                    var localApplicationMetaData = localApplicationMetaDataCollection.FirstOrDefault(a => a.ApplicationId.Equals(serverApplicationMetaData.ApplicationId));

                    //download controls belonging to application if newer version of control is available or control data doesn't already exist locally
                    List<string> controlsToDownload = new List<string>();
                    foreach (var controlMetaData in serverApplicationMetaData.ControlsMeta)
                    {
                        bool isNewerControlVersionAvailable = localApplicationMetaData?.ControlsMeta.Any(c => c.ControlId.Equals(controlMetaData.ControlId) && (c.LastUpdated < controlMetaData.LastUpdated)) ?? false;
                        if (!isNewerControlVersionAvailable)
                        {
                            continue;
                        }
                        controlsToDownload.Add(controlMetaData.ControlId);
                    }

                    if (controlsToDownload.Any())
                    {
                        GetControlDataForApplicationRequest request = new GetControlDataForApplicationRequest()
                        {
                            ApplicationId = serverApplicationMetaData.ApplicationId,
                            ControlIdCollection = new List<string>(controlsToDownload)
                        };
                        var zippedContent = await this.controlRepositoryClient.GetControls(request);

                        using (var memoryStream = new MemoryStream(zippedContent, false))
                        {
                            var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
                            foreach (var entry in zipArchive.Entries)
                            {
                                var targetFile = Path.Combine(applicationSettings.ApplicationDirectory, serverApplicationMetaData.ApplicationId, controlsDirectory, entry.FullName);
                                if (!Directory.Exists(Path.GetDirectoryName(targetFile)))
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(targetFile));
                                }
                                entry.ExtractToFile(Path.Combine(applicationSettings.ApplicationDirectory, serverApplicationMetaData.ApplicationId, controlsDirectory, entry.FullName), true);
                            }
                        }
                        logger.Information("Updated local copy for {count} controls for application : {applicationName} with Id : {applicationId}.",
                            controlsToDownload.Count(), serverApplicationMetaData.ApplicationName, serverApplicationMetaData.ApplicationId);
                    }

                    //download prefabs belonging to application of newer version of prefab is available or prefab data doesn't already exist locally
                    foreach (var serverPrefabMetaData in serverApplicationMetaData.PrefabsMeta)
                    {
                        var localPrefabMetaData = localApplicationMetaData?.PrefabsMeta.FirstOrDefault(p => p.PrefabId.Equals(serverPrefabMetaData.PrefabId));
                        if (!(localPrefabMetaData?.LastUpdated.Equals(serverPrefabMetaData.LastUpdated) ?? false))
                        {
                            await DownloadPrefabDescriptionFileAsync(serverPrefabMetaData.ApplicationId, serverPrefabMetaData.PrefabId);
                            logger.Information($"PrefabDescription file for prefab {serverPrefabMetaData.PrefabId} belonging to application {serverPrefabMetaData.ApplicationId} has been updated.");
                        }
                        foreach (var serverPrefabVersionMetaData in serverPrefabMetaData.Versions)
                        {
                            var localPrefabVersionMetaData = localPrefabMetaData?.Versions.FirstOrDefault(v => v.Version.Equals(serverPrefabVersionMetaData.Version));
                            if (!(localPrefabMetaData?.LastUpdated.Equals(serverPrefabVersionMetaData.LastUpdated) ?? false))
                            {
                                await DownloadPrefabDataAsync(serverPrefabMetaData.ApplicationId, serverPrefabMetaData.PrefabId, serverPrefabVersionMetaData.Version);
                                logger.Information($"Prefab data files for version {serverPrefabVersionMetaData.Version} of prefab {serverPrefabMetaData.PrefabId} belonging to application {serverPrefabMetaData.ApplicationId} have been updated.");
                            }
                        }
                    }
                    CreateOrUpdateApplicationMetaData(serverApplicationMetaDataCollection);
                }
            }
        }

        /// <summary>
        /// Save the ApplicationDetails of ApplicationToolBoxItem to File
        /// </summary>
        /// <param name="application"></param>
        private string SaveApplicationToDisk(ApplicationDescription application)
        {

            string appDirectory = GetApplicationDirectory(application);
            if (!Directory.Exists(appDirectory))
            {
                Directory.CreateDirectory(appDirectory);
            }

            string appFile = GetApplicationFile(application);
            if (File.Exists(appFile))
            {
                File.Delete(appFile);
            }

            serializer.Serialize(appFile, application, typeProvider.KnownTypes["Default"]);
            return appFile;
        }


        private string GetApplicationDirectory(ApplicationDescription application)
        {
            return Path.Combine(applicationSettings.ApplicationDirectory, application.ApplicationId);
        }

        private string GetApplicationFile(ApplicationDescription application)
        {
            return Path.Combine(applicationSettings.ApplicationDirectory, application.ApplicationId, $"{application.ApplicationId}.app");
        }

        #endregion Applications

        #region Controls

        public async Task AddOrUpdateControlAsync(ControlDescription controlDescription)
        {
            SaveControlToDisk(controlDescription);
            if(IsOnlineMode)
            {
                await controlRepositoryClient.AddOrUpdateControl(controlDescription);
            }
        }

        public async Task<string> AddOrUpdateControlImageAsync(ControlDescription controlDescription, Stream stream, string imageResolution)
        {
            Directory.CreateDirectory(GetControlDirectory(controlDescription));
            string saveLocation = GetControlImageFile(controlDescription);
            using (FileStream fs = new FileStream(saveLocation, FileMode.Create))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fs);
            }
            if(IsOnlineMode)
            {
                await controlRepositoryClient.AddOrUpdateControlImage(controlDescription, saveLocation, imageResolution ?? "Default");
            }
            return saveLocation;
        }

        public IEnumerable<ControlDescription> GetAllControls(ApplicationDescription applicationDescription)
        {
            foreach (var controlId in applicationDescription.AvailableControls)
            {
                string controlFile = Path.Combine(applicationSettings.ApplicationDirectory, applicationDescription.ApplicationId, "Controls", controlId, $"{controlId}.dat");

                if (File.Exists(controlFile))
                {
                    ControlDescription controlDescription = serializer.Deserialize<ControlDescription>(controlFile);
                    yield return controlDescription;
                }
            }
            yield break;
        }

        private string SaveControlToDisk(ControlDescription controlDescription)
        {
            Directory.CreateDirectory(GetControlDirectory(controlDescription));
            string fileToCreate = GetControlFile(controlDescription);
            if (File.Exists(fileToCreate))
            {
                File.Delete(fileToCreate);
            }
            serializer.Serialize<ControlDescription>(fileToCreate, controlDescription);
            return fileToCreate;
        }


        private string GetControlFile(ControlDescription controlItem)
        {
            return Path.Combine(GetControlDirectory(controlItem), $"{controlItem.ControlId}.dat");
        }

        private string GetControlImageFile(ControlDescription controlItem)
        {
            return Path.Combine(GetControlDirectory(controlItem), "ScreenShot.Png");
        }

        private string GetControlDirectory(ControlDescription controlItem)
        {
            return Path.Combine(applicationSettings.ApplicationDirectory, controlItem.ApplicationId, controlsDirectory, controlItem.ControlId);
        }

        #endregion Controls

        #region Project

        public string GetProjectsRootDirectory()
        {
            return Path.Combine(Environment.CurrentDirectory, this.applicationSettings.AutomationDirectory);
        }

        public string GetProjectDirectory(AutomationProject automationProject)
        {
            return Path.Combine(Environment.CurrentDirectory, this.applicationSettings.AutomationDirectory, automationProject.Name);
        }

        public string GetProjectFile(AutomationProject automationProject)
        {
            return Path.Combine(Environment.CurrentDirectory, this.applicationSettings.AutomationDirectory, automationProject.Name, $"{automationProject.Name}.atm");
        }

        public IEnumerable<AutomationProject> GetAllProjects()
        {          
            foreach (var item in Directory.EnumerateDirectories(this.applicationSettings.AutomationDirectory))
            {
                string automationProjectFile = $"{item}\\{Path.GetFileName(item)}.atm";
                if(File.Exists(automationProjectFile))
                {
                    var automationProject = serializer.Deserialize<AutomationProject>(automationProjectFile, null);
                    yield return automationProject;
                    continue;
                }
                logger.Warning("Project file {file} doesn't exist.", automationProjectFile);
            }
            yield break;
        }

        public async Task AddOrUpdateProjectAsync(AutomationProject automationProject, VersionInfo projectVersion)
        {
            if(IsOnlineMode)
            {
                string projectFileLocation = Path.Combine(applicationSettings.AutomationDirectory, automationProject.Name, $"{automationProject.Name}.atm");
                await this.projectRepositoryClient.AddOrUpdateProject(automationProject, projectFileLocation);
                if (projectVersion != null)
                {
                    string versionDirectory = Path.Combine(applicationSettings.AutomationDirectory, automationProject.Name, projectVersion.ToString());
                    if (!Directory.Exists(versionDirectory))
                    {
                        throw new ArgumentException($"Save project version data failed.{projectVersion.ToString()} directory doesn't exist for project : {automationProject.Name}");
                    }
                    string zipLocation = Path.Combine(applicationSettings.AutomationDirectory, automationProject.Name, $"{automationProject.Name}.zip");
                    if (File.Exists(zipLocation))
                    {
                        File.Delete(zipLocation);
                    }
                    ZipFile.CreateFromDirectory(versionDirectory, zipLocation);
                    await this.projectRepositoryClient.AddOrUpdateProjectDataFiles(automationProject, projectVersion, zipLocation);
                    File.Delete(zipLocation);
                }
            }         
        }

        public async Task DownloadProjectsAsync()
        {
            if(IsOnlineMode)
            {
                var serverProjectsMetaDataCollection = await this.metaDataClient.GetProjectsMetaData();
                var localProjectsMetaDataCollection = GetLocalProjectsMetaData();
                foreach (var projectMetaData in serverProjectsMetaDataCollection)
                {
                    if (localProjectsMetaDataCollection.Any(a => a.ProjectId.Equals(projectMetaData.ProjectId) && a.LastUpdated < projectMetaData.LastUpdated)
                             || !localProjectsMetaDataCollection.Any(a => a.ProjectId.Equals(projectMetaData.ProjectId)))
                    {
                        var projectFile = await this.projectRepositoryClient.GetProjectFile(projectMetaData.ProjectId);
                        SaveProjectToDisk(projectFile);
                    }
                    CreateOrUpdateProjectsMetaData(serverProjectsMetaDataCollection);
                }
            }           
        }

        private string SaveProjectToDisk(AutomationProject automationProject)
        {

            string projectDirectory = Path.Combine(this.applicationSettings.AutomationDirectory, automationProject.Name);
            if (!Directory.Exists(projectDirectory))
            {
                Directory.CreateDirectory(projectDirectory);
            }

            string projectFile = Path.Combine(projectDirectory, $"{automationProject.Name}.atm");
            if (File.Exists(projectFile))
            {
                File.Delete(projectFile);
            }

            serializer.Serialize(projectFile, automationProject, typeProvider.KnownTypes["Default"]);
            return projectFile;
        }

        public async Task DownloadProjectDataAsync(AutomationProject automationProject, VersionInfo projectVersion)
        {
            if(IsOnlineMode)
            {
                var serverProjectMetaDataCollection = await this.metaDataClient.GetProjectMetaData(automationProject.ProjectId);
                var localProjectsMetaDataCollection = GetLocalProjectVersionMetaData(automationProject.Name);

                var serverVersion = serverProjectMetaDataCollection.FirstOrDefault(a => a.ProjectId.Equals(automationProject.ProjectId) &&
                    a.Version.Equals(projectVersion.ToString()));
                var localVersion = localProjectsMetaDataCollection.FirstOrDefault(a => a.ProjectId.Equals(automationProject.ProjectId) &&
                    a.Version.Equals(projectVersion.ToString()));

                if (serverVersion?.LastUpdated > (localVersion?.LastUpdated ?? DateTime.MinValue))
                {
                    var zippedContent = await this.projectRepositoryClient.GetProjectDataFiles(automationProject.ProjectId, projectVersion.ToString());
                    string versionDirectory = Path.Combine(this.applicationSettings.AutomationDirectory, automationProject.Name, projectVersion.ToString());
                    using (var memoryStream = new MemoryStream(zippedContent, false))
                    {
                        var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
                        zipArchive.ExtractToDirectory(versionDirectory, true);
                    }
                    
                    //append only entry for the projectVersion being downloaded to local copy of metadata.
                    var updatedLocalMetaDataCollection = new List<ProjectMetaData>(localProjectsMetaDataCollection);
                    updatedLocalMetaDataCollection.Add(serverVersion);
                    CreateOrUpdateProjectVersionMetaData(automationProject.Name, updatedLocalMetaDataCollection);
                }
            }          
        }

        #endregion Project

        #region Prefabs

        public async Task AddOrUpdatePrefabDescriptionAsync(PrefabDescription prefabDescription, VersionInfo prefabVersion)
        {
            if (IsOnlineMode)
            {
                var prefabFileSystem = new PrefabFileSystem(this.serializer, this.applicationSettings);
                prefabFileSystem.Initialize(prefabDescription.ApplicationId, prefabDescription.PrefabId, prefabVersion);           
                await this.prefabRepositoryClient.AddOrUpdatePrefabAsync(prefabDescription, prefabFileSystem.PrefabDescriptionFile);
                var applicationMetaData = GetLocalApplicationMetaData().FirstOrDefault(a => a.ApplicationId.Equals(prefabDescription.ApplicationId));
                applicationMetaData.AddOrUpdatePrefabMetaData(prefabDescription.PrefabId, prefabVersion.ToString(), prefabVersion.IsDeployed);

            }
        }

        public async Task AddOrUpdatePrefabDataFilesAsync(PrefabDescription prefabDescription, VersionInfo prefabVersion)
        {
            if(IsOnlineMode)
            {
                if (prefabVersion != null)
                {
                    var prefabFileSystem = new PrefabFileSystem(this.serializer, this.applicationSettings);
                    prefabFileSystem.Initialize(prefabDescription.ApplicationId, prefabDescription.PrefabId, prefabVersion);
                    if (!Directory.Exists(prefabFileSystem.WorkingDirectory))
                    {
                        throw new ArgumentException($"Version {prefabVersion.ToString()} data directory doesn't exist for prefab : {prefabDescription.PrefabName}");
                    }
                    string prefabsDirectory = Path.GetDirectoryName(prefabFileSystem.PrefabDescriptionFile);
                    string zipLocation = Path.Combine(prefabsDirectory, $"{prefabDescription.PrefabName}.zip");
                    if (File.Exists(zipLocation))
                    {
                        File.Delete(zipLocation);
                    }
                    ZipFile.CreateFromDirectory(prefabFileSystem.WorkingDirectory, zipLocation);
                    await this.prefabRepositoryClient.AddOrUpdatePrefabDataFilesAsync(prefabDescription, prefabVersion, zipLocation);
                    File.Delete(zipLocation);
                }
            }
        }

        public async Task DownloadPrefabDescriptionFileAsync(string applicationId, string prefabId)
        {
            if (IsOnlineMode)
            {
                var prefabDescription = await prefabRepositoryClient.GetPrefabFileAsync(prefabId);
                var prefabDirectory = Path.Combine(Environment.CurrentDirectory, applicationSettings.ApplicationDirectory, applicationId, prefabsDirectory, prefabId);
                var prefabDescriptionFile = Path.Combine(prefabDirectory, "PrefabDescription.dat");
                if(!Directory.Exists(prefabDirectory))
                {
                    Directory.CreateDirectory(prefabDirectory);
                }
                this.serializer.Serialize<PrefabDescription>(prefabDescriptionFile, prefabDescription);
            }         
        }
       
        public async Task DownloadPrefabDataAsync(string applicationId, string prefabId, string version)
        {
            if (IsOnlineMode)
            {
                var prefabFileSystem = new PrefabFileSystem(this.serializer, this.applicationSettings);
                prefabFileSystem.Initialize(applicationId, prefabId, new PrefabVersion(version));
                var zippedContent = await this.prefabRepositoryClient.GetPrefabDataFilesAsync(prefabId, version);               
                using (var memoryStream = new MemoryStream(zippedContent, false))
                {
                    var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
                    zipArchive.ExtractToDirectory(prefabFileSystem.WorkingDirectory, true);
                }               
            }
        }

        #endregion Prefabs

        #region Metadata

        private IEnumerable<ApplicationMetaData> GetLocalApplicationMetaData()
        {
            string metaDataFile = Path.Combine(applicationSettings.ApplicationDirectory, "Applications.meta");
            if (File.Exists(metaDataFile))
            {
                return this.serializer.Deserialize<List<ApplicationMetaData>>(metaDataFile);
            }
            return Array.Empty<ApplicationMetaData>();
        }

        private IEnumerable<ProjectMetaData> GetLocalProjectsMetaData()
        {
            string metaDataFile = Path.Combine(applicationSettings.AutomationDirectory, "Projects.meta");
            if (File.Exists(metaDataFile))
            {
                return this.serializer.Deserialize<List<ProjectMetaData>>(metaDataFile);
            }
            return Array.Empty<ProjectMetaData>();
        }

        private IEnumerable<ProjectMetaData> GetLocalProjectVersionMetaData(string projectName)
        {
            string metaDataFile = Path.Combine(applicationSettings.AutomationDirectory, projectName, "Versions.meta");
            if (File.Exists(metaDataFile))
            {
                return this.serializer.Deserialize<List<ProjectMetaData>>(metaDataFile);
            }
            return Array.Empty<ProjectMetaData>();
        }

        private void CreateOrUpdateProjectsMetaData(IEnumerable<ProjectMetaData> projects)
        {
            string metaDataFile = Path.Combine(this.applicationSettings.AutomationDirectory, "Projects.meta");
            if (File.Exists(metaDataFile))
            {
                File.Delete(metaDataFile);
            }
            this.serializer.Serialize<IEnumerable<ProjectMetaData>>(metaDataFile, projects);
        }

        private void CreateOrUpdateProjectVersionMetaData(string projectName, IEnumerable<ProjectMetaData> projects)
        {
            string metaDataFile = Path.Combine(applicationSettings.AutomationDirectory, projectName, "Versions.meta");
            if (File.Exists(metaDataFile))
            {
                File.Delete(metaDataFile);
            }
            this.serializer.Serialize<IEnumerable<ProjectMetaData>>(metaDataFile, projects);
        }

        private void CreateOrUpdateApplicationMetaData(IEnumerable<ApplicationMetaData> applicationMetaDatas)
        {
            string metaDataFile = Path.Combine(applicationSettings.ApplicationDirectory, "Applications.meta");
            if (File.Exists(metaDataFile))
            {
                File.Delete(metaDataFile);
            }
            this.serializer.Serialize<IEnumerable<ApplicationMetaData>>(metaDataFile, applicationMetaDatas);
            logger.Information("Local application metadata has been updated.");
        }

        #endregion Metadata
    }
}
