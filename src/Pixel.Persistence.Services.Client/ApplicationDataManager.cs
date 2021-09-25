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

        private readonly IApplicationRepositoryClient appRepositoryClient;
        private readonly IControlRepositoryClient controlRepositoryClient;
        private readonly IProjectRepositoryClient projectRepositoryClient;
        private readonly IPrefabRepositoryClient prefabRepositoryClient;
        private readonly IMetaDataClient metaDataClient;
        private readonly ISerializer serializer;
        private readonly ITypeProvider typeProvider;
        private readonly ApplicationSettings applicationSettings;
        private readonly IApplicationFileSystem applicationFileSystem;

        bool IsOnlineMode
        {
            get => !this.applicationSettings.IsOfflineMode;
        }

        #region Constructor

        public ApplicationDataManager(ISerializer serializer, ITypeProvider typeProvider, IMetaDataClient metaDataClient,
            IApplicationRepositoryClient appRepositoryClient, IControlRepositoryClient controlRepositoryClient,
            IProjectRepositoryClient projectRepositoryClient, IPrefabRepositoryClient prefabRepositoryClient, 
            ApplicationSettings applicationSettings, IApplicationFileSystem applicationFileSystem)
        {
            this.serializer = Guard.Argument(serializer).NotNull().Value;
            this.typeProvider = Guard.Argument(typeProvider).NotNull().Value;
            this.metaDataClient = Guard.Argument(metaDataClient).NotNull().Value;
            this.appRepositoryClient = Guard.Argument(appRepositoryClient).NotNull().Value;
            this.controlRepositoryClient = Guard.Argument(controlRepositoryClient).NotNull().Value;
            this.projectRepositoryClient = Guard.Argument(projectRepositoryClient).NotNull().Value;
            this.prefabRepositoryClient = Guard.Argument(prefabRepositoryClient).NotNull().Value;
            this.applicationSettings = Guard.Argument(applicationSettings).NotNull();
            this.applicationFileSystem = Guard.Argument(applicationFileSystem).NotNull().Value;
            
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

        #endregion Constructor

        #region Applications 

        /// <summary>
        /// Get all the applications available on disk
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ApplicationDescription> GetAllApplications()
        {
            foreach (var app in Directory.GetDirectories(this.applicationSettings.ApplicationDirectory))
            {
                string appFile = Directory.GetFiles(app, "*.app", SearchOption.TopDirectoryOnly).FirstOrDefault();
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
            Directory.CreateDirectory(Path.Combine(applicationSettings.ApplicationDirectory, applicationDescription.ApplicationId, Constants.ControlsDirectory));           

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
                        bool isNewerControlVersionAvailable = localApplicationMetaData?.ControlsMeta.Any(c => c.ControlId.Equals(controlMetaData.ControlId) && (c.LastUpdated < controlMetaData.LastUpdated)) ?? true;
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
                                var targetFile = Path.Combine(applicationSettings.ApplicationDirectory, serverApplicationMetaData.ApplicationId, Constants.ControlsDirectory, entry.FullName);
                                if (!Directory.Exists(Path.GetDirectoryName(targetFile)))
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(targetFile));
                                }
                                entry.ExtractToFile(Path.Combine(applicationSettings.ApplicationDirectory, serverApplicationMetaData.ApplicationId, Constants.ControlsDirectory, entry.FullName), true);
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
                            await DownloadPrefabFileAsync(serverPrefabMetaData.ApplicationId, serverPrefabMetaData.PrefabId);
                            logger.Information($"PrefabProject file for prefab {serverPrefabMetaData.PrefabId} belonging to application {serverPrefabMetaData.ApplicationId} has been updated.");
                        }
                        foreach (var serverPrefabVersionMetaData in serverPrefabMetaData.Versions)
                        {
                            var localPrefabVersionMetaData = localPrefabMetaData?.Versions.FirstOrDefault(v => v.Version.Equals(serverPrefabVersionMetaData.Version));
                            var prefabProject = new PrefabProject() { ApplicationId = serverPrefabMetaData.ApplicationId, PrefabId = serverPrefabMetaData.PrefabId };
                            if (!(localPrefabMetaData?.LastUpdated.Equals(serverPrefabVersionMetaData.LastUpdated) ?? false))
                            {
                                await DownloadPrefabDataAsync(prefabProject, serverPrefabVersionMetaData.Version);
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
            string appDirectory = this.applicationFileSystem.GetApplicationDirectory(application);
            if (!Directory.Exists(appDirectory))
            {
                Directory.CreateDirectory(appDirectory);
            }

            string appFile = this.applicationFileSystem.GetApplicationFile(application);
            if (File.Exists(appFile))
            {
                File.Delete(appFile);
            }

            serializer.Serialize(appFile, application, typeProvider.KnownTypes["Default"]);
            return appFile;
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

        public async Task<string> AddOrUpdateControlImageAsync(ControlDescription controlDescription, Stream stream)
        {
            Directory.CreateDirectory(GetControlDirectory(controlDescription));
            //we need a new file name each time we change the image so that application can update image without restart.
            //This is due to caching mechanism of Bitmap which doesn't monitor file content for change but only responds to if file
            //is a different file.
            string saveLocation = Path.Combine(GetControlDirectory(controlDescription), $"{Path.GetRandomFileName()}.Png");
            using (FileStream fs = new FileStream(saveLocation, FileMode.Create))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fs);
            }
            if(IsOnlineMode)
            {
                await controlRepositoryClient.AddOrUpdateControlImage(controlDescription, saveLocation);
            }
            return saveLocation;
        }

        public IEnumerable<ControlDescription> GetAllControls(ApplicationDescription applicationDescription)
        {
            foreach (var controlId in applicationDescription.AvailableControls)
            {
                string controlFile = Path.Combine(applicationSettings.ApplicationDirectory, applicationDescription.ApplicationId, Constants.ControlsDirectory, controlId, $"{controlId}.dat");

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

        private string GetControlDirectory(ControlDescription controlItem)
        {
            return Path.Combine(applicationSettings.ApplicationDirectory, controlItem.ApplicationId, Constants.ControlsDirectory, controlItem.ControlId);
        }

        #endregion Controls

        #region Project
       
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
                string projectFileLocation = this.applicationFileSystem.GetAutomationProjectFile(automationProject);
                await this.projectRepositoryClient.AddOrUpdateProject(automationProject, projectFileLocation);
                if (projectVersion != null)
                {
                    string versionDirectory = Path.Combine(this.applicationFileSystem.GetAutomationProjectWorkingDirectory(automationProject, projectVersion));
                    if (!Directory.Exists(versionDirectory))
                    {
                        throw new ArgumentException($"Save project version data failed.{projectVersion} directory doesn't exist for project : {automationProject.Name}");
                    }
                    string zipLocation = Path.Combine(this.applicationFileSystem.GetAutomationProjectDirectory(automationProject), $"{automationProject.ProjectId}.zip");
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

        public async Task DownloadProjectDataAsync(AutomationProject automationProject, VersionInfo projectVersion)
        {
            if (IsOnlineMode)
            {
                var serverProjectMetaDataCollection = await this.metaDataClient.GetProjectMetaData(automationProject.ProjectId);
                var localProjectsMetaDataCollection = GetLocalProjectVersionMetaData(automationProject);

                var serverVersion = serverProjectMetaDataCollection.FirstOrDefault(a => a.ProjectId.Equals(automationProject.ProjectId) &&
                    a.Version.Equals(projectVersion.ToString()));
                var localVersion = localProjectsMetaDataCollection.FirstOrDefault(a => a.ProjectId.Equals(automationProject.ProjectId) &&
                    a.Version.Equals(projectVersion.ToString()));

                if (serverVersion?.LastUpdated > (localVersion?.LastUpdated ?? DateTime.MinValue))
                {
                    var zippedContent = await this.projectRepositoryClient.GetProjectDataFiles(automationProject.ProjectId, projectVersion.ToString());
                    string versionDirectory = this.applicationFileSystem.GetAutomationProjectWorkingDirectory(automationProject, projectVersion);
                    using (var memoryStream = new MemoryStream(zippedContent, false))
                    {
                        var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
                        zipArchive.ExtractToDirectory(versionDirectory, true);
                    }

                    //append only entry for the projectVersion being downloaded to local copy of metadata.
                    var updatedLocalMetaDataCollection = new List<ProjectMetaData>(localProjectsMetaDataCollection);
                    updatedLocalMetaDataCollection.Add(serverVersion);
                    CreateOrUpdateProjectVersionMetaData(automationProject, updatedLocalMetaDataCollection);
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

        private void SaveProjectToDisk(AutomationProject automationProject)
        {
            string projectDirectory = this.applicationFileSystem.GetAutomationProjectDirectory(automationProject);
            if (!Directory.Exists(projectDirectory))
            {
                Directory.CreateDirectory(projectDirectory);
            }

            string projectFile = this.applicationFileSystem.GetAutomationProjectFile(automationProject);
            if (File.Exists(projectFile))
            {
                File.Delete(projectFile);
            }

            serializer.Serialize(projectFile, automationProject, typeProvider.KnownTypes["Default"]);          
        }
      
        #endregion Project

        #region Prefabs

        private readonly Dictionary<string, List<PrefabProject>> prefabsCache = new ();

        /// <summary>
        /// Get all the applications available on disk
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PrefabProject> GetAllPrefabs(string applicationId)
        {
            if(!prefabsCache.ContainsKey(applicationId))
            {
                prefabsCache.Add(applicationId, LoadPrefabsFromLocalStorage(applicationId));
            }
            return prefabsCache[applicationId];
        }

        public PrefabProject GetPrefab(string applicationId, string prefabId)
        {           
           var prefab = GetAllPrefabs(applicationId).FirstOrDefault(p => p.PrefabId.Equals(prefabId));
           return prefab ?? throw new ArgumentException($"No Prefab exists with applicationId {applicationId} and prefabId {prefabId}");
        }

        public async Task AddOrUpdatePrefabAsync(PrefabProject prefabProject, VersionInfo prefabVersion)
        {
            if (IsOnlineMode)
            {
                string prefabProjectFile = this.applicationFileSystem.GetPrefabProjectFile(prefabProject);
                await this.prefabRepositoryClient.AddOrUpdatePrefabAsync(prefabProject, prefabProjectFile);              
            }
           
            //when a new prefab is created , add it to the prefabs cache
            if(prefabsCache.ContainsKey(prefabProject.ApplicationId))
            {
                var prefabsForApplicationId = prefabsCache[prefabProject.ApplicationId];
                if (!prefabsForApplicationId.Any(p =>  p.PrefabId.Equals(prefabProject.PrefabId)))
                {
                    prefabsForApplicationId.Add(prefabProject);
                }
            }
            else
            {
                prefabsCache.Add(prefabProject.ApplicationId, new List<PrefabProject>() { prefabProject });
            }         
        }

        public async Task AddOrUpdatePrefabDataFilesAsync(PrefabProject prefabProject, VersionInfo prefabVersion)
        {
            Guard.Argument(prefabProject).NotNull();
            Guard.Argument(prefabVersion).NotNull();
            if(IsOnlineMode)
            {
                string prefabWorkingDirectory = this.applicationFileSystem.GetPrefabProjectWorkingDirectory(prefabProject, prefabVersion);
                string prefabDirectory = this.applicationFileSystem.GetPrefabProjectDirectory(prefabProject);
                if (!Directory.Exists(prefabWorkingDirectory))
                {
                    throw new ArgumentException($"Version {prefabVersion} data directory doesn't exist for prefab : {prefabProject.PrefabName}");
                }

                string zipLocation = Path.Combine(prefabDirectory, $"{prefabProject.PrefabId}.zip");
                if (File.Exists(zipLocation))
                {
                    File.Delete(zipLocation);
                }
                ZipFile.CreateFromDirectory(prefabWorkingDirectory, zipLocation);
                await this.prefabRepositoryClient.AddOrUpdatePrefabDataFilesAsync(prefabProject, prefabVersion, zipLocation);
                File.Delete(zipLocation);
            }
        }

        public async Task DownloadPrefabFileAsync(string applicationId, string prefabId)
        {
            if (IsOnlineMode)
            {
                var prefabProject = await prefabRepositoryClient.GetPrefabFileAsync(prefabId);
                string prefabDirectory = this.applicationFileSystem.GetPrefabProjectDirectory(prefabProject);
                var prefabProjectFile = this.applicationFileSystem.GetPrefabProjectFile(prefabProject);
                if(!Directory.Exists(prefabDirectory))
                {
                    Directory.CreateDirectory(prefabDirectory);
                }
                this.serializer.Serialize<PrefabProject>(prefabProjectFile, prefabProject);
            }         
        }
       
        public async Task DownloadPrefabDataAsync(PrefabProject prefabProject, string version)
        {
            if (IsOnlineMode)
            {               
                var prefabWorkingDirectory = this.applicationFileSystem.GetPrefabProjectWorkingDirectory(prefabProject, version);                
                var zippedContent = await this.prefabRepositoryClient.GetPrefabDataFilesAsync(prefabProject.PrefabId, version);         
                using (var memoryStream = new MemoryStream(zippedContent, false))
                {
                    var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
                    zipArchive.ExtractToDirectory(prefabWorkingDirectory, true);
                }               
            }
        }

        private List<PrefabProject> LoadPrefabsFromLocalStorage(string applicationId)
        {
            List<PrefabProject> prefabProjects = new ();
            var prefabDirectory = this.applicationFileSystem.GetApplicationPrefabsDirectory(applicationId);
            if(Directory.Exists(prefabDirectory))
            {
                foreach (var prefab in Directory.GetDirectories(prefabDirectory))
                {
                    string prefabProjectFile = Directory.GetFiles(prefab, "*.atm", SearchOption.TopDirectoryOnly).FirstOrDefault();
                    if (File.Exists(prefabProjectFile))
                    {
                        PrefabProject prefabProject = serializer.Deserialize<PrefabProject>(prefabProjectFile);
                        prefabProjects.Add(prefabProject);
                        continue;
                    }
                    logger.Warning("Prefab project file : {0} doesn't exist", prefabProjectFile);
                }
            }
            return prefabProjects;
        }

        #endregion Prefabs

        #region Metadata

        private IEnumerable<ApplicationMetaData> GetLocalApplicationMetaData()
        {
            string metaDataFile = Path.Combine(applicationSettings.ApplicationDirectory, Constants.ApplicationsMeta);
            if (File.Exists(metaDataFile))
            {
                return this.serializer.Deserialize<List<ApplicationMetaData>>(metaDataFile);
            }
            return Array.Empty<ApplicationMetaData>();
        }

        private void CreateOrUpdateApplicationMetaData(IEnumerable<ApplicationMetaData> applicationMetaDatas)
        {
            string metaDataFile = Path.Combine(applicationSettings.ApplicationDirectory, Constants.ApplicationsMeta);
            if (File.Exists(metaDataFile))
            {
                File.Delete(metaDataFile);
            }
            this.serializer.Serialize<IEnumerable<ApplicationMetaData>>(metaDataFile, applicationMetaDatas);
            logger.Information("Local application metadata has been updated.");
        }

        private IEnumerable<ProjectMetaData> GetLocalProjectsMetaData()
        {
            string metaDataFile = Path.Combine(applicationSettings.AutomationDirectory, Constants.ProjectsMeta);
            if (File.Exists(metaDataFile))
            {
                return this.serializer.Deserialize<List<ProjectMetaData>>(metaDataFile);
            }
            return Array.Empty<ProjectMetaData>();
        }     
      
        private void CreateOrUpdateProjectsMetaData(IEnumerable<ProjectMetaData> projects)
        {
            string metaDataFile = Path.Combine(this.applicationSettings.AutomationDirectory, Constants.ProjectsMeta);
            if (File.Exists(metaDataFile))
            {
                File.Delete(metaDataFile);
            }
            this.serializer.Serialize<IEnumerable<ProjectMetaData>>(metaDataFile, projects);
        }

        private IEnumerable<ProjectMetaData> GetLocalProjectVersionMetaData(AutomationProject automationProject)
        {
            string metaDataFile = Path.Combine(this.applicationFileSystem.GetAutomationProjectDirectory(automationProject), Constants.VersionsMeta);
            if (File.Exists(metaDataFile))
            {
                return this.serializer.Deserialize<List<ProjectMetaData>>(metaDataFile);
            }
            return Array.Empty<ProjectMetaData>();
        }

        private void CreateOrUpdateProjectVersionMetaData(AutomationProject automationProject, IEnumerable<ProjectMetaData> projects)
        {
            string metaDataFile = Path.Combine(this.applicationFileSystem.GetAutomationProjectDirectory(automationProject), Constants.VersionsMeta);
            if (File.Exists(metaDataFile))
            {
                File.Delete(metaDataFile);
            }
            this.serializer.Serialize<IEnumerable<ProjectMetaData>>(metaDataFile, projects);
        }     

        #endregion Metadata
    }
}
