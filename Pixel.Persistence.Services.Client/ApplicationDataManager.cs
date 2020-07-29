using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Persistence.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public class ApplicationDataManager : IApplicationDataManager
    {
        private readonly string applicationsRepository = "ApplicationsRepository";
        private readonly string controlsDirectory = "Controls";
        private readonly string prefabsDirectory = "Prefabs";

        private readonly IApplicationRepositoryClient appRepositoryClient;
        private readonly IControlRepositoryClient controlRepositoryClient;
        private readonly IMetaDataClient metaDataClient;
        private readonly ISerializer serializer;
        private readonly ITypeProvider typeProvider;
        public ApplicationDataManager(ISerializer serializer, ITypeProvider typeProvider, IMetaDataClient metaDataClient,
            IApplicationRepositoryClient appRepositoryClient, IControlRepositoryClient controlRepositoryClient)
        {
            this.appRepositoryClient = appRepositoryClient;
            this.controlRepositoryClient = controlRepositoryClient;
            this.metaDataClient = metaDataClient;
            this.serializer = serializer;
            this.typeProvider = typeProvider;
        }

        #region Applications 

        /// <summary>
        /// Saves a new application to database
        /// </summary>
        /// <param name="applicationDescription"></param>
        public async Task AddOrUpdateApplicationAsync(ApplicationDescription applicationDescription)
        {
            Directory.CreateDirectory(Path.Combine(applicationsRepository, applicationDescription.ApplicationId));
            Directory.CreateDirectory(Path.Combine(applicationsRepository, applicationDescription.ApplicationId, controlsDirectory));
            Directory.CreateDirectory(Path.Combine(applicationsRepository, applicationDescription.ApplicationId, prefabsDirectory));

            string savedFile = SaveApplicationToDisk(applicationDescription);
            await this.appRepositoryClient.AddOrUpdateApplication(applicationDescription, savedFile);
        }

        /// <summary>
        /// Download application data from database which are not already available on disk along with their control data
        /// </summary>
        /// <returns></returns>
        public async Task DownloadApplicationsDataAsync()
        {
            if (!Directory.Exists(this.applicationsRepository))
            {
                Directory.CreateDirectory(this.applicationsRepository);
            }
            var serverApplicationMetaDataCollection = await this.metaDataClient.GetApplicationMetaData();
            var localApplicationMetaDataCollection = GetLocalApplicationMetaData();
            foreach (var applicationMetaData in serverApplicationMetaDataCollection)
            {
                if (localApplicationMetaDataCollection.Any(a => a.ApplicationId.Equals(applicationMetaData.ApplicationId) && a.LastUpdated < applicationMetaData.LastUpdated)
                         || !localApplicationMetaDataCollection.Any(a => a.ApplicationId.Equals(applicationMetaData.ApplicationId)))
                {
                    var applicationDescription = await this.appRepositoryClient.GetApplication(applicationMetaData.ApplicationId);
                    SaveApplicationToDisk(applicationDescription);
                }

                var localApplicationMetaData = localApplicationMetaDataCollection.FirstOrDefault(a => a.ApplicationId.Equals(applicationMetaData.ApplicationId));
                List<string> controlsToDownload = new List<string>();
                foreach (var controlMetaData in applicationMetaData.ControlsMeta)
                {
                    if (localApplicationMetaData?.ControlsMeta.Any(c => c.ControlId.Equals(controlMetaData.ControlId) && c.LastUpdated.Equals(controlMetaData.LastUpdated)) ?? false)
                    {
                        continue;
                    }
                    controlsToDownload.Add(controlMetaData.ControlId);
                }

                if (controlsToDownload.Any())
                {
                    GetControlDataForApplicationRequest request = new GetControlDataForApplicationRequest()
                    {
                        ApplicationId = applicationMetaData.ApplicationId,
                        ControlIdCollection = new List<string>(controlsToDownload)
                    };
                    var zippedContent = await this.controlRepositoryClient.GetControls(request);

                    using (var memoryStream = new MemoryStream(zippedContent, false))
                    {
                        var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
                        foreach (var entry in zipArchive.Entries)
                        {
                            var targetFile = Path.Combine(applicationsRepository, applicationMetaData.ApplicationId, controlsDirectory, entry.FullName);
                            if (!Directory.Exists(Path.GetDirectoryName(targetFile)))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(targetFile));
                            }
                            entry.ExtractToFile(Path.Combine(applicationsRepository, applicationMetaData.ApplicationId, controlsDirectory, entry.FullName), true);
                        }
                    }
                }
            }
            CreateOrUpdateApplicationMetaData(serverApplicationMetaDataCollection);

        }

        /// <summary>
        /// Get all the applications available on disk
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ApplicationDescription> GetAllApplications()
        {         
            foreach (var app in Directory.GetDirectories(this.applicationsRepository))
            {
                string appFile = Directory.GetFiles(Path.Combine(this.applicationsRepository, new DirectoryInfo(app).Name), "*.app", SearchOption.TopDirectoryOnly).FirstOrDefault();
                ApplicationDescription application = serializer.Deserialize<ApplicationDescription>(appFile);
                yield return application;
            }
            yield break;
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
            if (System.IO.File.Exists(appFile))
            {
                System.IO.File.Delete(appFile);
            }

            serializer.Serialize(appFile, application, typeProvider.KnownTypes["Default"]);
            return appFile;
        }


        private string GetApplicationDirectory(ApplicationDescription application)
        {
            return Path.Combine(applicationsRepository, application.ApplicationId);
        }

        private string GetApplicationFile(ApplicationDescription application)
        {
            return Path.Combine(applicationsRepository, application.ApplicationId, $"{application.ApplicationId}.app");
        }

        #endregion Applications

        #region Controls
       
        public async Task AddOrUpdateControlAsync(ControlDescription controlDescription)
        {          
            string savedFile = SaveControlToDisk(controlDescription);
            await controlRepositoryClient.AddOrUpdateControl(controlDescription, savedFile);
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
            await controlRepositoryClient.AddOrUpdateControlImage(controlDescription, saveLocation, imageResolution ?? "Default");
            return saveLocation;
        }

        public IEnumerable<ControlDescription> GetAllControls(ApplicationDescription applicationDescription)
        {
            foreach (var controlId in applicationDescription.AvailableControls)
            {
                string controlFile = Path.Combine("ApplicationsRepository", applicationDescription.ApplicationId, "Controls", controlId, $"{controlId}.dat");

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
            return Path.Combine(applicationsRepository, controlItem.ApplicationId, controlsDirectory, controlItem.ControlId);
        }

        #endregion Controls

        private IEnumerable<ApplicationMetaData> GetLocalApplicationMetaData()
        {
            string metaDataFile = Path.Combine(applicationsRepository, "Applications.meta");
            if (System.IO.File.Exists(metaDataFile))
            {
                return this.serializer.Deserialize<List<ApplicationMetaData>>(metaDataFile);
            }
            return Array.Empty<ApplicationMetaData>();
        }

        private void CreateOrUpdateApplicationMetaData(IEnumerable<ApplicationMetaData> applicationMetaDatas)
        {
            string metaDataFile = Path.Combine(applicationsRepository, "Applications.meta");
            if (System.IO.File.Exists(metaDataFile))
            {
                System.IO.File.Delete(metaDataFile);
            }
            this.serializer.Serialize<IEnumerable<ApplicationMetaData>>(metaDataFile, applicationMetaDatas);
        }
    }
}
