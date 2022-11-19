using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Persistence.Services.Client.Interfaces;
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
        private readonly IMetaDataClient metaDataClient;
        private readonly ISerializer serializer;
        private readonly ITypeProvider typeProvider;
        private readonly ApplicationSettings applicationSettings;
        private readonly IApplicationFileSystem applicationFileSystem;
        private readonly IAutomationsRepositoryClient automationRepositoryClient;
        private readonly IProjectFilesRepositoryClient projectFilesRepositoryClient;

        bool IsOnlineMode
        {
            get => !this.applicationSettings.IsOfflineMode;
        }

        #region Constructor

        public ApplicationDataManager(ISerializer serializer, ITypeProvider typeProvider, IMetaDataClient metaDataClient,
            IApplicationRepositoryClient appRepositoryClient, IControlRepositoryClient controlRepositoryClient,           
            IAutomationsRepositoryClient automationRepositoryClient, IProjectFilesRepositoryClient projectFilesRepositoryClient,
            ApplicationSettings applicationSettings, IApplicationFileSystem applicationFileSystem)
        {
            this.serializer = Guard.Argument(serializer).NotNull().Value;
            this.typeProvider = Guard.Argument(typeProvider).NotNull().Value;
            this.metaDataClient = Guard.Argument(metaDataClient).NotNull().Value;
            this.appRepositoryClient = Guard.Argument(appRepositoryClient).NotNull().Value;
            this.controlRepositoryClient = Guard.Argument(controlRepositoryClient).NotNull().Value;
            this.projectFilesRepositoryClient = Guard.Argument(projectFilesRepositoryClient).NotNull().Value;          
            this.applicationSettings = Guard.Argument(applicationSettings).NotNull();
            this.applicationFileSystem = Guard.Argument(applicationFileSystem).NotNull().Value;
            this.automationRepositoryClient = Guard.Argument(automationRepositoryClient, nameof(automationRepositoryClient)).NotNull().Value;
            
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
                        var request = new Core.Models.GetControlDataForApplicationRequest()
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

            serializer.Serialize(appFile, application, typeProvider.GetKnownTypes());
            return appFile;
        }

        #endregion Applications

        #region Controls

        /// <inheritdoc/>      
        public async Task AddOrUpdateControlAsync(ControlDescription controlDescription)
        {
            SaveControlToDisk(controlDescription);
            if(IsOnlineMode)
            {
                await controlRepositoryClient.AddOrUpdateControl(controlDescription);
            }
        }

        /// <inheritdoc/>      
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

        /// <inheritdoc/>      
        public async Task DeleteControlImageAsync(ControlDescription controlDescription, string imageFile)
        {
           if(!File.Exists(imageFile))
           {
                throw new FileNotFoundException($"{imageFile} doesn't exist");
           }
           File.Delete(imageFile);
           if (IsOnlineMode)
           {
                await controlRepositoryClient.DeleteControlImageAsync(controlDescription, imageFile);
           }
        }

        /// <inheritdoc/>      
        public IEnumerable<ControlDescription> GetAllControls(ApplicationDescription applicationDescription)
        {
            foreach(var screen in applicationDescription.AvailableControls)
            {
                foreach(var controlId in screen.Value)
                {
                    string controlDirectory = Path.Combine(applicationSettings.ApplicationDirectory, applicationDescription.ApplicationId, Constants.ControlsDirectory, controlId);
                    foreach(var revision in Directory.EnumerateDirectories(controlDirectory))
                    {
                        string controlPath = Path.Combine(revision, $"{controlId}.dat");
                        if (File.Exists(controlPath))
                        {
                            ControlDescription controlDescription = serializer.Deserialize<ControlDescription>(controlPath);
                            yield return controlDescription;
                        }
                    }                 
                }              
            }
            yield break;
        }

        /// <inheritdoc/>       
        public IEnumerable<ControlDescription> GetControlsForScreen(ApplicationDescription applicationDescription, string screenName)
        {
            if(applicationDescription.AvailableControls.ContainsKey(screenName))
            {
                foreach (var controlId in applicationDescription.AvailableControls[screenName])
                {
                    string controlDirectory = Path.Combine(applicationSettings.ApplicationDirectory, applicationDescription.ApplicationId, Constants.ControlsDirectory, controlId);
                    var latestRevision = Directory.EnumerateDirectories(controlDirectory).Max(a => a);
                    string controlPath = Path.Combine(latestRevision, $"{controlId}.dat");
                    if (File.Exists(controlPath))
                    {
                        ControlDescription controlDescription = serializer.Deserialize<ControlDescription>(controlPath);
                        yield return controlDescription;
                    }
                }               
            }
            yield break;
        }

       /// <inheritdoc />
       public IEnumerable<ControlDescription> GetControlsById(string applicationId, string controlId)
        {
            string controlDirectory = Path.Combine(applicationSettings.ApplicationDirectory, applicationId, Constants.ControlsDirectory, controlId);
            foreach (var revision in Directory.EnumerateDirectories(controlDirectory))
            {
                string controlPath = Path.Combine(revision, $"{controlId}.dat");
                if (File.Exists(controlPath))
                {
                    ControlDescription controlDescription = serializer.Deserialize<ControlDescription>(controlPath);
                    yield return controlDescription;
                }
            }
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
            return Path.Combine(applicationSettings.ApplicationDirectory, controlItem.ApplicationId, Constants.ControlsDirectory, controlItem.ControlId, controlItem.Version.ToString());
        }

        #endregion Controls
             
        #region Metadata

        private IEnumerable<Core.Models.ApplicationMetaData> GetLocalApplicationMetaData()
        {
            string metaDataFile = Path.Combine(applicationSettings.ApplicationDirectory, Constants.ApplicationsMeta);
            if (File.Exists(metaDataFile))
            {
                return this.serializer.Deserialize<List<Core.Models.ApplicationMetaData>>(metaDataFile);
            }
            return Array.Empty<Core.Models.ApplicationMetaData>();
        }

        private void CreateOrUpdateApplicationMetaData(IEnumerable<Core.Models.ApplicationMetaData> applicationMetaDatas)
        {
            string metaDataFile = Path.Combine(applicationSettings.ApplicationDirectory, Constants.ApplicationsMeta);
            if (File.Exists(metaDataFile))
            {
                File.Delete(metaDataFile);
            }
            this.serializer.Serialize<IEnumerable<Core.Models.ApplicationMetaData>>(metaDataFile, applicationMetaDatas);
            logger.Information("Local application metadata has been updated.");
        }
        
        #endregion Metadata
    }
}
