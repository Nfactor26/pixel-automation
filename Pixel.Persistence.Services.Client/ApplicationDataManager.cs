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
        private readonly string projectsDirectory = "Automations";

        private readonly IApplicationRepositoryClient appRepositoryClient;
        private readonly IControlRepositoryClient controlRepositoryClient;
        private readonly IProjectRepositoryClient projectRepositoryClient;
        private readonly IMetaDataClient metaDataClient;
        private readonly ISerializer serializer;
        private readonly ITypeProvider typeProvider;

        public ApplicationDataManager(ISerializer serializer, ITypeProvider typeProvider, IMetaDataClient metaDataClient,
            IApplicationRepositoryClient appRepositoryClient, IControlRepositoryClient controlRepositoryClient, IProjectRepositoryClient projectRepositoryClient)
        {
            this.serializer = serializer;
            this.typeProvider = typeProvider;
            this.metaDataClient = metaDataClient;
            this.appRepositoryClient = appRepositoryClient;
            this.controlRepositoryClient = controlRepositoryClient;
            this.projectRepositoryClient = projectRepositoryClient;
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
            if (File.Exists(appFile))
            {
                File.Delete(appFile);
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

        #region Project

        public async Task AddOrUpdateProjectAsync(AutomationProject automationProject, VersionInfo projectVersion)
        {
            string projectFileLocation = Path.Combine(projectsDirectory, automationProject.Name, $"{automationProject.Name}.atm");
            await this.projectRepositoryClient.AddOrUpdateProject(automationProject, projectFileLocation);
            if (projectVersion != null)
            {
                string versionDirectory = Path.Combine(projectsDirectory, automationProject.Name, projectVersion.ToString());
                if (!Directory.Exists(versionDirectory))
                {
                    throw new ArgumentException($"Save project version data failed.{projectVersion.ToString()} directory doesn't exist for project : {automationProject.Name}");
                }
                string zipLocation = Path.Combine(projectsDirectory, automationProject.Name, $"{automationProject.Name}.zip");
                if (File.Exists(zipLocation))
                {
                    File.Delete(zipLocation);
                }
                ZipFile.CreateFromDirectory(versionDirectory, zipLocation);
                await this.projectRepositoryClient.AddOrUpdateProjectDataFiles(automationProject, projectVersion, zipLocation);              
            }
        }

        public async Task DownloadProjectsAsync()
        {
            if (!Directory.Exists(this.projectsDirectory))
            {
                Directory.CreateDirectory(this.projectsDirectory);
            }
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
            }
            CreateOrUpdateProjectsMetaData(serverProjectsMetaDataCollection);
        }

        private string SaveProjectToDisk(AutomationProject automationProject)
        {

            string projectDirectory = Path.Combine(this.projectsDirectory, automationProject.Name);
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

            var serverProjectMetaDataCollection = await this.metaDataClient.GetProjectMetaData(automationProject.ProjectId);
            var localProjectsMetaDataCollection = GetLocalProjectVersionMetaData(automationProject.Name);

            var serverVersion = serverProjectMetaDataCollection.FirstOrDefault(a => a.ProjectId.Equals(automationProject.ProjectId));
            var localVersion = localProjectsMetaDataCollection.FirstOrDefault(a => a.ProjectId.Equals(automationProject.ProjectId));

            if (serverVersion?.LastUpdated  > (localVersion?.LastUpdated ?? DateTime.MinValue))
            {
                var zippedContent = await this.projectRepositoryClient.GetProjectDataFiles(automationProject.ProjectId, projectVersion.ToString());
                string versionDirectory = Path.Combine(this.projectsDirectory, automationProject.Name, projectVersion.ToString());
                using (var memoryStream = new MemoryStream(zippedContent, false))
                {
                    var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
                    zipArchive.ExtractToDirectory(versionDirectory);
                }
            }
            CreateOrUpdateProjectVersionMetaData(automationProject.Name, serverProjectMetaDataCollection);
        }

        #endregion Project

        private IEnumerable<ApplicationMetaData> GetLocalApplicationMetaData()
        {
            string metaDataFile = Path.Combine(applicationsRepository, "Applications.meta");
            if (File.Exists(metaDataFile))
            {
                return this.serializer.Deserialize<List<ApplicationMetaData>>(metaDataFile);
            }
            return Array.Empty<ApplicationMetaData>();
        }

        private IEnumerable<ProjectMetaData> GetLocalProjectsMetaData()
        {
            string metaDataFile = Path.Combine(projectsDirectory, "Projects.meta");
            if (File.Exists(metaDataFile))
            {
                return this.serializer.Deserialize<List<ProjectMetaData>>(metaDataFile);
            }
            return Array.Empty<ProjectMetaData>();
        }

        private IEnumerable<ProjectMetaData> GetLocalProjectVersionMetaData(string projectName)
        {
            string metaDataFile = Path.Combine(projectsDirectory, projectName, "Versions.meta");
            if (File.Exists(metaDataFile))
            {
                return this.serializer.Deserialize<List<ProjectMetaData>>(metaDataFile);
            }
            return Array.Empty<ProjectMetaData>();
        }

        private void CreateOrUpdateProjectsMetaData(IEnumerable<ProjectMetaData> projects)
        {
            string metaDataFile = Path.Combine(this.projectsDirectory, "Projects.meta");
            if (File.Exists(metaDataFile))
            {
                File.Delete(metaDataFile);
            }
            this.serializer.Serialize<IEnumerable<ProjectMetaData>>(metaDataFile, projects);
        }

        private void CreateOrUpdateProjectVersionMetaData(string projectName, IEnumerable<ProjectMetaData> projects)
        {
            string metaDataFile = Path.Combine(projectsDirectory, projectName, "Versions.meta");
            if (File.Exists(metaDataFile))
            {
                File.Delete(metaDataFile);
            }
            this.serializer.Serialize<IEnumerable<ProjectMetaData>>(metaDataFile, projects);
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
