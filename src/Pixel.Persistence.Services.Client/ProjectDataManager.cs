using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Core.TestData;
using Pixel.Persistence.Services.Client.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client;

public class ProjectDataManager : IProjectDataManager
{
    private readonly ILogger logger = Log.ForContext<ProjectDataManager>();
    private readonly IAutomationsRepositoryClient projectsClient;
    private readonly IReferencesRepositoryClient referencesRepositoryClient;
    private readonly IProjectFilesRepositoryClient filesClient;  
    private readonly IApplicationFileSystem applicationFileSystem;    
    private readonly ISerializer serializer;
    private readonly ApplicationSettings applicationSettings;
  
    bool IsOnlineMode
    {
        get => !this.applicationSettings.IsOfflineMode;
    }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="projectsClient"></param>
    /// <param name="referencesRepositoryClient"></param>
    /// <param name="filesClient"></param>
    /// <param name="fixturesClient"></param>
    /// <param name="testsClient"></param>
    /// <param name="applicationFileSystem"></param>
    /// <param name="projectFileSystem"></param>
    /// <param name="applicationSettings"></param>
    /// <param name="testDataRepositoryClient"></param>
    public ProjectDataManager(ISerializer serializer, IAutomationsRepositoryClient projectsClient, IReferencesRepositoryClient referencesRepositoryClient,
        IProjectFilesRepositoryClient filesClient, IApplicationFileSystem applicationFileSystem, 
        ApplicationSettings applicationSettings)
    {
        this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
        this.projectsClient = Guard.Argument(projectsClient, nameof(projectsClient)).NotNull().Value;
        this.referencesRepositoryClient = Guard.Argument(referencesRepositoryClient, nameof(referencesRepositoryClient)).NotNull().Value;
        this.filesClient = Guard.Argument(filesClient, nameof(filesClient)).NotNull().Value;     
        this.applicationSettings = Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
        this.applicationFileSystem = Guard.Argument(applicationFileSystem, nameof(applicationFileSystem)).NotNull().Value;      
    }

    /// <inheritdoc/>  
    public async Task DownloadProjectsAsync()
    {
        if (IsOnlineMode)
        {
            var projects = await this.projectsClient.GetAllAsync();
            foreach (var project in projects)
            {
                var projectDirectory = this.applicationFileSystem.GetAutomationProjectDirectory(project);
                if (!Directory.Exists(projectDirectory))
                {
                    Directory.CreateDirectory(projectDirectory);
                }
                var projectFile = this.applicationFileSystem.GetAutomationProjectFile(project);
                if (File.Exists(projectFile))
                {
                    File.Delete(projectFile);
                }
                serializer.Serialize(projectFile, project);

            }

        }
    }

    /// <inheritdoc/>  
    public async Task DownloadProjectDataFilesAsync(AutomationProject automationProject, ProjectVersion projectVersion)
    {
        if (IsOnlineMode)
        {
            //Download project references data
            var projectReferences = await this.referencesRepositoryClient.GetProjectReferencesAsync(automationProject.ProjectId, projectVersion.ToString());
            if (projectReferences != null)
            {
                var prefabReferencesFile = Path.Combine(this.applicationFileSystem.GetAutomationProjectWorkingDirectory(automationProject, projectVersion), Constants.ReferencesFileName);
                this.serializer.Serialize(prefabReferencesFile, projectReferences);
            }
            await DownloadFilesWithTagsAsync(automationProject, projectVersion, new[] { automationProject.ProjectId });
        }
    }

    /// <summary>
    /// Download all the files having specific tags belonging to the version of the AutomationProject being managed
    /// </summary>
    /// <param name="tags"></param>
    /// <returns></returns>
    async Task DownloadFilesWithTagsAsync(AutomationProject automationProject, ProjectVersion projectVersion, string[] tags)
    {
        if (IsOnlineMode)
        {
            //Download data model files and scripts
            var zippedContent = await this.filesClient.DownloadProjectDataFilesWithTags(automationProject.ProjectId, projectVersion.ToString(), tags);
            if (zippedContent.Length > 0)
            {
                string versionDirectory = this.applicationFileSystem.GetAutomationProjectWorkingDirectory(automationProject, projectVersion);
                using (var memoryStream = new MemoryStream(zippedContent, false))
                {
                    var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
                    zipArchive.ExtractToDirectory(versionDirectory, true);
                }
            }
        }
    }

    /// <inheritdoc/>  
    public IEnumerable<AutomationProject> GetAllProjects()
    {
        foreach (var item in Directory.EnumerateDirectories(this.applicationSettings.AutomationDirectory))
        {
            string automationProjectFile = $"{item}\\{Path.GetFileName(item)}.atm";
            if (File.Exists(automationProjectFile))
            {
                var automationProject = serializer.Deserialize<AutomationProject>(automationProjectFile, null);
                yield return automationProject;
                continue;
            }
            logger.Warning("Project file {file} doesn't exist.", automationProjectFile);
        }
        yield break;
    }

    /// <inheritdoc/>  
    public async Task AddProjectAsync(AutomationProject automationProject)
    {
        if (IsOnlineMode)
        {
            await this.projectsClient.AddProjectAsync(automationProject);
        }
    }

    /// <inheritdoc/>  
    public async Task AddProjectVersionAsync(AutomationProject automationProject, ProjectVersion newVersion, ProjectVersion versionToClone)
    {
        if (IsOnlineMode)
        {
            await this.projectsClient.AddProjectVersionAsync(automationProject.ProjectId, newVersion, versionToClone);
        }
        else
        {
            var cloneFromVersionDirectory = Path.Combine(applicationFileSystem.GetAutomationProjectWorkingDirectory(automationProject, versionToClone));
            var newVersionDirectory = Path.Combine(applicationFileSystem.GetAutomationProjectWorkingDirectory(automationProject, newVersion));           
            Directory.CreateDirectory(newVersionDirectory);

            ////copy contents from previous version directory to new version directory
            CopyAll(new DirectoryInfo(cloneFromVersionDirectory), new DirectoryInfo(newVersionDirectory));

            void CopyAll(DirectoryInfo source, DirectoryInfo target)
            {
                // Copy each file into the new directory.
                foreach (FileInfo fi in source.GetFiles())
                {
                    fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
                }

                // Copy each subdirectory using recursion.
                foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
                {
                    DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                    CopyAll(diSourceSubDir, nextTargetSubDir);
                }
            }
        }

        automationProject.AvailableVersions.Add(newVersion);
        string automationProjectsFile =this.applicationFileSystem.GetAutomationProjectFile(automationProject);
        serializer.Serialize(automationProjectsFile, automationProject);

    }

    /// <inheritdoc/>  
    public async Task UpdateProjectVersionAsync(AutomationProject automationProject, ProjectVersion projectVersion)
    {
        if (IsOnlineMode)
        {
            await this.projectsClient.UpdateProjectVersionAsync(automationProject.ProjectId, projectVersion);
        }
        string automationProjectsFile = this.applicationFileSystem.GetAutomationProjectFile(automationProject);
        serializer.Serialize(automationProjectsFile, automationProject);
    }

    /// <summary>
    /// Add data file to the version of AutomationProject being managed.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="tag"></param>
    /// <returns></returns>
    public async Task AddDataFileAsync(AutomationProject automationProject, ProjectVersion projectVersion, string filePath, string tag)
    {
        if (IsOnlineMode)
        {
            var projectsDirectory = Path.Combine(Environment.CurrentDirectory, applicationSettings.AutomationDirectory, automationProject.ProjectId, projectVersion.ToString());
            await this.filesClient.AddProjectDataFile(new Core.Models.ProjectDataFile()
            {
                ProjectId = automationProject.ProjectId,
                ProjectVersion = projectVersion.ToString(),
                Tag = tag,
                FileName = Path.GetFileName(filePath),
                FilePath = Path.GetRelativePath(projectsDirectory, filePath),
            }, filePath);
        }
    }


    /// <inheritdoc/>  
    public async Task SaveProjectDataAsync(AutomationProject automationProject, ProjectVersion projectVersion)
    {
        if (IsOnlineMode)
        {

            string projectDirectory = Path.Combine(this.applicationFileSystem.GetAutomationProjectWorkingDirectory(automationProject, projectVersion));

            string processsFile = Path.Combine(projectDirectory, Constants.AutomationProcessFileName);
            await AddDataFileAsync(automationProject, projectVersion, processsFile, automationProject.ProjectId);


            //save the datamodel assembly file
            foreach (var file in Directory.EnumerateFiles(Path.Combine(projectDirectory, Constants.ReferencesDirectory), "*.*"))
            {
                await AddDataFileAsync(automationProject, projectVersion, file, automationProject.ProjectId);
            }

            foreach (var file in Directory.EnumerateFiles(Path.Combine(projectDirectory, Constants.DataModelDirectory), "*.cs"))
            {
                await AddDataFileAsync(automationProject, projectVersion, file, automationProject.ProjectId);
            }

            foreach (var file in Directory.EnumerateFiles(Path.Combine(projectDirectory, Constants.ScriptsDirectory), "*.csx"))
            {
                await AddDataFileAsync(automationProject, projectVersion, file, automationProject.ProjectId);
            }
        }
    }
}

public class TestAndFixtureAndTestDataManager : IProjectAssetsDataManager
{
    private readonly ILogger logger = Log.ForContext<ProjectDataManager>();

    private readonly IFixturesRepositoryClient fixturesClient;
    private readonly ITestsRepositoryClient testsClient;   
    private readonly IProjectFilesRepositoryClient filesClient;
    private readonly ITestDataRepositoryClient testDataRepositoryClient;
    private readonly IApplicationFileSystem applicationFileSystem;
    private readonly IProjectFileSystem projectFileSystem;
    private readonly ISerializer serializer;
    private readonly ApplicationSettings applicationSettings;
    private AutomationProject automationProject;
    private VersionInfo projectVersion;
    private DateTime lastUpdated;

    bool IsOnlineMode
    {
        get => !this.applicationSettings.IsOfflineMode;
    }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="serializer"></param>  
    /// <param name="filesClient"></param>
    /// <param name="fixturesClient"></param>
    /// <param name="testsClient"></param>
    /// <param name="applicationFileSystem"></param>
    /// <param name="projectFileSystem"></param>
    /// <param name="applicationSettings"></param>
    /// <param name="testDataRepositoryClient"></param>
    public TestAndFixtureAndTestDataManager(ISerializer serializer, IProjectFilesRepositoryClient filesClient,
        IFixturesRepositoryClient fixturesClient, ITestsRepositoryClient testsClient,
        IApplicationFileSystem applicationFileSystem, IProjectFileSystem projectFileSystem,
        ApplicationSettings applicationSettings, ITestDataRepositoryClient testDataRepositoryClient)
    {
        this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;     
        this.filesClient = Guard.Argument(filesClient, nameof(filesClient)).NotNull().Value;
        this.fixturesClient = Guard.Argument(fixturesClient, nameof(fixturesClient)).NotNull().Value;
        this.testsClient = Guard.Argument(testsClient, nameof(testsClient)).NotNull().Value;
        this.applicationSettings = Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
        this.applicationFileSystem = Guard.Argument(applicationFileSystem, nameof(applicationFileSystem)).NotNull().Value;
        this.projectFileSystem = Guard.Argument(projectFileSystem, nameof(projectFileSystem)).NotNull().Value;
        this.testDataRepositoryClient = Guard.Argument(testDataRepositoryClient, nameof(testDataRepositoryClient)).NotNull().Value;       
    }

    /// <inheritdoc/>  
    public void Initialize(AutomationProject automationProject, VersionInfo projectVersion)
    {
        this.automationProject = Guard.Argument(automationProject, nameof(automationProject)).NotNull();
        this.projectVersion = Guard.Argument(projectVersion, nameof(projectVersion)).NotNull();
        string lastUpdatedDataFile = Path.Combine(this.projectFileSystem.WorkingDirectory, Constants.LastUpdatedFileName);
        if (File.Exists(lastUpdatedDataFile))
        {
            if(!DateTime.TryParse(File.ReadAllText(lastUpdatedDataFile), out lastUpdated))
            {
                throw new Exception($"Failed to read last updated data from file : {lastUpdatedDataFile}");
            }
            File.Delete(lastUpdatedDataFile);
        }
        else
        {
            lastUpdated = DateTime.MinValue.ToUniversalTime();
        }
        File.WriteAllText(lastUpdatedDataFile, DateTime.Now.ToUniversalTime().ToString());
    }


    /// <summary>
    /// Add data file to the version of AutomationProject being managed.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="tag"></param>
    /// <returns></returns>
    async Task AddDataFileAsync(string filePath, string tag)
    {
        if (IsOnlineMode)
        {
            await this.filesClient.AddProjectDataFile(new Core.Models.ProjectDataFile()
            {
                ProjectId = this.automationProject.ProjectId,
                ProjectVersion = this.projectVersion.ToString(),
                Tag = tag,
                FileName = Path.GetFileName(filePath),
                FilePath = this.projectFileSystem.GetRelativePath(filePath),
            }, filePath);
        }
    }

    /// <summary>
    /// Download all the files having specific tags belonging to the version of the AutomationProject being managed
    /// </summary>
    /// <param name="tags"></param>
    /// <returns></returns>
    async Task DownloadFilesWithTagsAsync(string[] tags)
    {
        if (IsOnlineMode)
        {
            //Download data model files and scripts
            var zippedContent = await this.filesClient.DownloadProjectDataFilesWithTags(automationProject.ProjectId, projectVersion.ToString(), tags);
            if (zippedContent.Length > 0)
            {
                string versionDirectory = this.applicationFileSystem.GetAutomationProjectWorkingDirectory(automationProject, projectVersion);
                using (var memoryStream = new MemoryStream(zippedContent, false))
                {
                    var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
                    zipArchive.ExtractToDirectory(versionDirectory, true);
                }
            }
        }
    }

    #region Fixtures 

    /// <inheritdoc/>  
    public async Task<TestFixture> AddTestFixtureAsync(TestFixture testFixture)
    {
        var fixtureFiles = this.projectFileSystem.GetTestFixtureFiles(testFixture);
        if (string.IsNullOrEmpty(testFixture.ScriptFile))
        {
            testFixture.ScriptFile = this.projectFileSystem.GetRelativePath(fixtureFiles.ScriptFile);
            this.projectFileSystem.CreateOrReplaceFile(fixtureFiles.FixtureDirectory, Path.GetFileName(testFixture.ScriptFile), string.Empty);
        }
        this.projectFileSystem.SaveToFile<TestFixture>(testFixture, fixtureFiles.FixtureDirectory, Path.GetFileName(fixtureFiles.FixtureFile));
        this.projectFileSystem.SaveToFile<Entity>(testFixture.TestFixtureEntity, fixtureFiles.FixtureDirectory, Path.GetFileName(fixtureFiles.ProcessFile));

        if (IsOnlineMode)
        {
            await this.fixturesClient.AddFixtureAsync(this.automationProject.ProjectId, this.projectVersion.ToString(), testFixture);
            await AddDataFileAsync(fixtureFiles.ProcessFile, testFixture.FixtureId);
            await AddDataFileAsync(fixtureFiles.ScriptFile, testFixture.FixtureId);
        }

        return testFixture;
    }

    /// <inheritdoc/>  
    public async Task<TestFixture> UpdateTestFixtureAsync(TestFixture testFixture)
    {
        var fixtureFiles = this.projectFileSystem.GetTestFixtureFiles(testFixture);
        this.projectFileSystem.SaveToFile<TestFixture>(testFixture, fixtureFiles.FixtureDirectory, Path.GetFileName(fixtureFiles.FixtureFile));
        if (IsOnlineMode)
        {
            await this.fixturesClient.UpdateFixtureAsync(this.automationProject.ProjectId, this.projectVersion.ToString(), testFixture);
        }
        return testFixture;
    }

    /// <inheritdoc/>  
    public async Task DeleteTestFixtureAsync(TestFixture testFixture)
    {       
        if (IsOnlineMode)
        {
            await this.fixturesClient.DeleteFixtureAsync(this.automationProject.ProjectId, this.projectVersion.ToString(), testFixture.FixtureId);            
        }
        var fixtureFiles = this.projectFileSystem.GetTestFixtureFiles(testFixture);
        Directory.Delete(fixtureFiles.FixtureDirectory, true);
    }

    /// <inheritdoc/>  
    public async Task SaveFixtureDataAsync(TestFixture testFixture)
    {
        var fixtureFiles = this.projectFileSystem.GetTestFixtureFiles(testFixture);      
        this.projectFileSystem.SaveToFile<Entity>(testFixture.TestFixtureEntity, fixtureFiles.FixtureDirectory, Path.GetFileName(fixtureFiles.ProcessFile));     
        if (IsOnlineMode)
        {
            await AddDataFileAsync(fixtureFiles.ProcessFile, testFixture.FixtureId);
            foreach (var scriptFile in Directory.EnumerateFiles(fixtureFiles.FixtureDirectory, "*.csx"))
            {
                await AddDataFileAsync(scriptFile, testFixture.FixtureId);
            }
        }
    }

    /// <inheritdoc/>  
    public async Task DownloadFixtureDataAsync(TestFixture testFixture)
    {
        if (IsOnlineMode)
        {
            await DownloadFilesWithTagsAsync(new[] { testFixture.FixtureId });
        }
    }

    /// <inheritdoc/>  
    public async Task DownloadAllFixturesAsync()
    {
        if (IsOnlineMode)
        {
            var fixtures = await this.fixturesClient.GetAllForProjectAsync(automationProject.ProjectId, projectVersion.ToString(), lastUpdated);
            var workingDirectory = Path.Combine(Environment.CurrentDirectory, applicationSettings.AutomationDirectory, automationProject.ProjectId, projectVersion.ToString());
            foreach (var fixture in fixtures)
            {
                var fixtureDirectory = Path.Combine(workingDirectory, Constants.TestCasesDirectory, fixture.FixtureId);
                var fixtureFile = Path.Combine(fixtureDirectory, $"{fixture.FixtureId}.fixture");
                if (!Directory.Exists(fixtureDirectory))
                {
                    Directory.CreateDirectory(fixtureDirectory);
                }
                serializer.Serialize(fixtureFile, fixture);
            }
        }
    }

    #endregion Fixtures

    #region Tests

    /// <inheritdoc/>  
    public async Task<TestCase> AddTestCaseAsync(TestCase testCase)
    {
        var testCaseFiles = this.projectFileSystem.GetTestCaseFiles(testCase);
        if (string.IsNullOrEmpty(testCase.ScriptFile))
        {
            testCase.ScriptFile = this.projectFileSystem.GetRelativePath(testCaseFiles.ScriptFile);
            this.projectFileSystem.CreateOrReplaceFile(testCaseFiles.TestDirectory, Path.GetFileName(testCase.ScriptFile), string.Empty);
        }
        this.projectFileSystem.SaveToFile<TestCase>(testCase, testCaseFiles.TestDirectory);
        this.projectFileSystem.SaveToFile<Entity>(testCase.TestCaseEntity, testCaseFiles.TestDirectory, testCaseFiles.ProcessFile);

        if (IsOnlineMode)
        {
            await this.testsClient.AddTestCaseAsync(this.automationProject.ProjectId, this.projectVersion.ToString(), testCase);
            await AddDataFileAsync(testCaseFiles.ProcessFile, testCase.TestCaseId);
            await AddDataFileAsync(testCaseFiles.ScriptFile, testCase.TestCaseId);
        }
        return testCase;
    }

    /// <inheritdoc/>  
    public async Task<TestCase> UpdateTestCaseAsync(TestCase testCase)
    {
        var testCaseFiles = this.projectFileSystem.GetTestCaseFiles(testCase);
        this.projectFileSystem.SaveToFile<TestCase>(testCase, testCaseFiles.TestDirectory);
        if (IsOnlineMode)
        {
            await this.testsClient.UpdateTestCaseAsync(this.automationProject.ProjectId, this.projectVersion.ToString(), testCase);
        }
        return testCase;
    }

    /// <inheritdoc/>  
    public async Task DeleteTestCaseAsync(TestCase testCase)
    {       
        if (IsOnlineMode)
        {
            await this.testsClient.DeleteTestCaseAsync(this.automationProject.ProjectId, this.projectVersion.ToString(), testCase.TestCaseId);
        }
        var testCaseFiles = this.projectFileSystem.GetTestCaseFiles(testCase);
        Directory.Delete(testCaseFiles.TestDirectory, true);
    }

    /// <inheritdoc/>  
    public async Task SaveTestDataAsync(TestCase testCase)
    {
        await UpdateTestCaseAsync(testCase);
        var testCaseFiles = this.projectFileSystem.GetTestCaseFiles(testCase);
        this.projectFileSystem.SaveToFile<Entity>(testCase.TestCaseEntity, testCaseFiles.TestDirectory, testCaseFiles.ProcessFile);
        if (IsOnlineMode)
        {
            await AddDataFileAsync(testCaseFiles.ProcessFile, testCase.TestCaseId);
            foreach (var scriptFile in Directory.EnumerateFiles(testCaseFiles.TestDirectory, "*.csx"))
            {
                await AddDataFileAsync(scriptFile, testCase.FixtureId);
            }
        }
    }

    /// <inheritdoc/>  
    public async Task DownloadTestDataAsync(TestCase testCase)
    {
        if (IsOnlineMode)
        {
            await DownloadFilesWithTagsAsync(new[] { testCase.TestCaseId });
        }
    }

    /// <inheritdoc/>  
    public async Task DownloadAllTestsAsync()
    {
        if (IsOnlineMode)
        {
            var tests = await this.testsClient.GetAllForProjectAsync(automationProject.ProjectId, projectVersion.ToString(), lastUpdated);
            var workingDirectory = Path.Combine(Environment.CurrentDirectory, applicationSettings.AutomationDirectory, automationProject.ProjectId, projectVersion.ToString());
            foreach (var test in tests)
            {
                var fixtureDirectory = Path.Combine(workingDirectory, Constants.TestCasesDirectory, test.FixtureId);
                var testDirectory = Path.Combine(fixtureDirectory, test.TestCaseId);
                var testFile = Path.Combine(testDirectory, $"{test.TestCaseId}.test");
                if (!Directory.Exists(testDirectory))
                {
                    Directory.CreateDirectory(testDirectory);
                }
                serializer.Serialize(testFile, test);
            }
        }
    }


    #endregion Tests

    #region Test Data Source

    /// <inheritdoc/>  
    public async Task<TestDataSource> AddTestDataSourceAsync(TestDataSource dataSource)
    {
        this.projectFileSystem.SaveToFile<TestDataSource>(dataSource, projectFileSystem.TestDataRepository, $"{dataSource.DataSourceId}.dat");
        if (IsOnlineMode)
        {
            await this.testDataRepositoryClient.AddDataSourceAsync(this.automationProject.ProjectId, this.projectVersion.ToString(), dataSource);
            await SaveTestDataSourceDataAsync(dataSource);
        }
        return dataSource;
    }

    /// <inheritdoc/>  
    public async Task<TestDataSource> UpdateTestDataSourceAsync(TestDataSource dataSource)
    {
        this.projectFileSystem.SaveToFile<TestDataSource>(dataSource, projectFileSystem.TestDataRepository, $"{dataSource.DataSourceId}.dat");
        if (IsOnlineMode)
        {
            await this.testDataRepositoryClient.UpdateDataSourceAsync(this.automationProject.ProjectId, this.projectVersion.ToString(), dataSource);
        }
        return dataSource;
    }

    /// <inheritdoc/>  
    public async Task DeleteTestDataSourceAsync(TestDataSource dataSource)
    {
        File.Delete(Path.Combine(this.projectFileSystem.TestDataRepository, $"{dataSource.DataSourceId}.dat"));
        File.Delete(Path.Combine(this.projectFileSystem.TestDataRepository, dataSource.ScriptFile));
        if (IsOnlineMode)
        {
            await this.testDataRepositoryClient.DeleteDataSourceAsync(this.automationProject.ProjectId, this.projectVersion.ToString(), dataSource.DataSourceId);
        }
    }

    /// <inheritdoc/>  
    public async Task SaveTestDataSourceDataAsync(TestDataSource dataSource)
    {
        if (IsOnlineMode)
        {
            await AddDataFileAsync(Path.Combine(this.projectFileSystem.TestDataRepository, Path.GetFileName(dataSource.ScriptFile)), dataSource.DataSourceId);
            if (dataSource.DataSource.Equals(DataSource.CsvFile) && dataSource.MetaData is CsvDataSourceConfiguration csvMetaData)
            {
                await AddDataFileAsync(Path.Combine(this.projectFileSystem.TestDataRepository, csvMetaData.TargetFile), dataSource.DataSourceId);
            }
        }
    }

    /// <inheritdoc/>  
    public async Task DownloadAllTestDataSourcesAsync()
    {
        if (IsOnlineMode)
        {
            var dataSources = await this.testDataRepositoryClient.GetAllForProjectAsync(automationProject.ProjectId, projectVersion.ToString(), lastUpdated);
            foreach (var dataSource in dataSources)
            {
                var dataSourceFile = Path.Combine(this.projectFileSystem.TestDataRepository, $"{dataSource.DataSourceId}.dat");
                this.projectFileSystem.SaveToFile(dataSource, Path.GetDirectoryName(dataSourceFile), Path.GetFileName(dataSourceFile));
            }
            var tags = dataSources.Select(d => d.DataSourceId).ToArray();
            if (tags.Any())
            {
                await DownloadFilesWithTagsAsync(tags);
            }
        }
    }

    #endregion Test Data Source   
}
