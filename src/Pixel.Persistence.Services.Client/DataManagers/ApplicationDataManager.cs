using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public class ApplicationDataManager : IApplicationDataManager
    {
        private readonly ILogger logger = Log.ForContext<ApplicationDataManager>();

        private readonly IApplicationRepositoryClient appRepositoryClient;
        private readonly IControlRepositoryClient controlRepositoryClient;              
        private readonly ISerializer serializer;
        private readonly ITypeProvider typeProvider;
        private readonly ApplicationSettings applicationSettings;
        private readonly IApplicationFileSystem applicationFileSystem;
        private readonly DateTime lastUpdated;

        bool IsOnlineMode
        {
            get => !this.applicationSettings.IsOfflineMode;
        }

        #region Constructor

        public ApplicationDataManager(ISerializer serializer, ITypeProvider typeProvider, IApplicationRepositoryClient appRepositoryClient,
            IControlRepositoryClient controlRepositoryClient, ApplicationSettings applicationSettings, IApplicationFileSystem applicationFileSystem)
        {
            this.serializer = Guard.Argument(serializer).NotNull().Value;
            this.typeProvider = Guard.Argument(typeProvider).NotNull().Value;            
            this.appRepositoryClient = Guard.Argument(appRepositoryClient).NotNull().Value;
            this.controlRepositoryClient = Guard.Argument(controlRepositoryClient).NotNull().Value;            
            this.applicationSettings = Guard.Argument(applicationSettings).NotNull();
            this.applicationFileSystem = Guard.Argument(applicationFileSystem).NotNull().Value;            
            
            CreateLocalDataDirectories();

            if(IsOnlineMode)
            {
                string lastUpdatedDataFile = Path.Combine(applicationSettings.ApplicationDirectory, Constants.LastUpdatedFileName);
                if (File.Exists(lastUpdatedDataFile))
                {
                    if (!DateTime.TryParse(File.ReadAllText(lastUpdatedDataFile), out lastUpdated))
                    {
                        throw new Exception($"Failed to read last updated data from file : {lastUpdatedDataFile}");
                    }
                    File.Delete(lastUpdatedDataFile);
                }
                else
                {
                    lastUpdated = DateTime.MinValue.ToUniversalTime();
                }
                File.WriteAllText(lastUpdatedDataFile, DateTime.Now.ToUniversalTime().ToString("O"));
            }            
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

        public async Task UpdateApplicationRepository()
        {
            await DownloadApplicationsAsync();
            var applications = GetAllApplications();
            foreach(var application in applications)
            {
                await DownloadControlsAsync(application.ApplicationId);
            }
        }

        public async Task DownloadApplicationsAsync()
        {
            if(IsOnlineMode)
            {
                var applications = await this.appRepositoryClient.GetApplications(this.lastUpdated);
                foreach (var application in applications)
                {
                    SaveApplicationToDisk(application);
                    logger.Information("Updated local copy of Application : {applicationName}", application.ApplicationName);
                }
            }            
        }

        public async Task DownloadControlsAsync(string applicationId)
        {
            if(IsOnlineMode)
            {
                var controls = await this.controlRepositoryClient.GetControls(applicationId, this.lastUpdated);
                foreach (var control in controls)
                {
                    SaveControlToDisk(control);
                    logger.Information("Updated local copy of control : {0} for application : {1}", control.ControlName, control.ApplicationId);
                }

                var controlImages = await this.controlRepositoryClient.GetControlImages(applicationId, this.lastUpdated);
                foreach (var image in controlImages)
                {
                    string imageFile = Path.Combine(applicationSettings.ApplicationDirectory, applicationId, Constants.ControlsDirectory, image.ControlId,
                        image.Version, image.FileName);
                    if(File.Exists(imageFile))
                    {
                        File.Delete(imageFile);
                    }
                    using(MemoryStream ms = new MemoryStream(image.Bytes))
                    {
                        using(FileStream fs = new FileStream(imageFile, FileMode.CreateNew))
                        {
                            ms.Seek(0, SeekOrigin.Begin);
                            ms.CopyTo(fs);
                        }
                    }
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
       
    }
}
