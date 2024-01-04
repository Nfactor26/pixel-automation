using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Core.TestData;
using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Persistence.Services.Client.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client;

public class TestAndFixtureAndTestDataManager : IProjectAssetsDataManager
{
    private readonly ILogger logger = Log.ForContext<TestAndFixtureAndTestDataManager>();

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
            await this.filesClient.AddProjectDataFile(new ProjectDataFile()
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
    public async Task DownloadFilesWithTagsAsync(string[] tags)
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
    public async Task<TestFixture> GetTestFixtureAsync(string fixtureId)
    {
        if (IsOnlineMode)
        {
            return await this.fixturesClient.GetByIdAsync(automationProject.ProjectId, projectVersion.ToString(), fixtureId);
        }
        return null;
    }

    /// <inheritdoc/>  
    public async Task<TestFixture> AddTestFixtureAsync(TestFixture testFixture)
    {
        var fixtureFiles = this.projectFileSystem.GetTestFixtureFiles(testFixture);
        if (string.IsNullOrEmpty(testFixture.ScriptFile))
        {
            testFixture.ScriptFile = this.projectFileSystem.GetRelativePath(fixtureFiles.ScriptFile).Replace("\\", "/");
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
        logger.Information("Added test fixture : '{0}' to version : '{1}' of automation project : '{2}'.", testFixture.DisplayName, projectVersion, automationProject.Name);

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
        logger.Information("Updated test fixture : '{0}' for version : '{1}' of automation project : '{2}'.", testFixture.DisplayName, projectVersion, automationProject.Name);
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
        logger.Information("Deleted test fixture : '{0}' from version : '{1}' of automation project : '{2}'.", testFixture.DisplayName, projectVersion, automationProject.Name);
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
        logger.Information("Saved data file for test fixture : '{0}' for version : '{1}' of automation project : '{2}'.", testFixture.DisplayName, projectVersion, automationProject.Name);
    }

    /// <inheritdoc/>  
    public async Task DownloadFixtureDataAsync(TestFixture testFixture)
    {
        if (IsOnlineMode)
        {
            //There can be abandoned script files locally if some other user delted a component from test fixture along with it's script files.
            //We don't want this script file to be saved back from this user after he edits and saves the test fixture. Hence, we delete all script files
            //before retrieving latest data.
            var fixtureFiles = this.projectFileSystem.GetTestFixtureFiles(testFixture);
            foreach (var file in Directory.GetFiles(fixtureFiles.FixtureDirectory, "*.csx"))
            {
                File.Delete(file);
            }
            await DownloadFilesWithTagsAsync(new[] { testFixture.FixtureId });
            logger.Information("Downloaded data for test fixture : '{0}' for version : '{1}' of automation project : '{2}'.", testFixture.DisplayName, projectVersion, automationProject.Name);
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
                logger.Information("Updated local copy of Fixture : {0}", fixture.DisplayName);
            }
        }
    }

    #endregion Fixtures

    #region Tests

    /// <inheritdoc/> 
    public async Task<TestCase> GetTestCaseAsync(string testCaseId)
    {
        if (IsOnlineMode)
        {
            return await this.testsClient.GetByIdAsync(automationProject.ProjectId, projectVersion.ToString(), testCaseId);
        }
        return null;
    }

    /// <inheritdoc/>  
    public async Task<TestCase> AddTestCaseAsync(TestCase testCase)
    {
        var testCaseFiles = this.projectFileSystem.GetTestCaseFiles(testCase);
        if (string.IsNullOrEmpty(testCase.ScriptFile))
        {
            testCase.ScriptFile = this.projectFileSystem.GetRelativePath(testCaseFiles.ScriptFile).Replace("\\", "/");
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
        logger.Information("Added test case : '{0}' to version : '{1}' of automation project : '{2}'.", testCase.DisplayName, projectVersion, automationProject.Name);
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
        logger.Information("Updated test case : '{0}' for version : '{1}' of automation project : '{2}'.", testCase.DisplayName, projectVersion, automationProject.Name);
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
        logger.Information("Deleted test case : '{0}' from version : '{1}' of automation project : '{2}'.", testCase.DisplayName, projectVersion, automationProject.Name);
    }

    /// <inheritdoc/>  
    public async Task SaveTestDataAsync(TestCase testCase)
    {
        var testCaseFiles = this.projectFileSystem.GetTestCaseFiles(testCase);
        this.projectFileSystem.SaveToFile<Entity>(testCase.TestCaseEntity, testCaseFiles.TestDirectory, testCaseFiles.ProcessFile);
        if (IsOnlineMode)
        {
            await AddDataFileAsync(testCaseFiles.ProcessFile, testCase.TestCaseId);
            foreach (var scriptFile in Directory.EnumerateFiles(testCaseFiles.TestDirectory, "*.csx"))
            {
                await AddDataFileAsync(scriptFile, testCase.TestCaseId);
            }
        }
        logger.Information("Saved data files for test case : '{0}' for version : '{1}' of automation project : '{2}'.", testCase.DisplayName, projectVersion, automationProject.Name);
    }

    /// <inheritdoc/>  
    public async Task DownloadTestDataAsync(TestCase testCase)
    {
        if (IsOnlineMode)
        {
            //There can be abandoned script files locally if some other user delted a component from test cases along with it's script files.
            //We don't want this script file to be saved back from this user after he edits and saves the test case. Hence, we delete all script files
            //before retrieving latest data.
            var testCaseFiles = this.projectFileSystem.GetTestCaseFiles(testCase);
            foreach (var file in Directory.GetFiles(testCaseFiles.TestDirectory, "*.csx"))
            {
                File.Delete(file);
            }
            await DownloadFilesWithTagsAsync(new[] { testCase.TestCaseId });
            logger.Information("Downloaded data files for test case : '{0}' for version : '{1}' of automation project : '{2}'.", testCase.DisplayName, projectVersion, automationProject.Name);
        }
    }

    /// <inheritdoc/>  
    public async Task DownloadAllTestsAsync()
    {
        if (IsOnlineMode)
        {
            var tests = await this.testsClient.GetAllForProjectAsync(automationProject.ProjectId, projectVersion.ToString(), lastUpdated);
            SaveTestCasesLocally(tests);
        }
    }

    /// <inheritdoc/>  
    public async Task DownloadTestCasesForFixtureAsync(string fixtureId)
    {
        if (IsOnlineMode)
        {
            var tests = await this.testsClient.GetAllForFixtureAsync(automationProject.ProjectId, projectVersion.ToString(), fixtureId, lastUpdated);
            SaveTestCasesLocally(tests);
        }
    }

    private void SaveTestCasesLocally(IEnumerable<TestCase> tests)
    {
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
            logger.Information("Updated local copy of Test Case : {0}", test.DisplayName);
        }
    }

    #endregion Tests

    #region Test Data Source

    /// <inheritdoc/>  
    public async Task<TestDataSource> AddTestDataSourceAsync(string groupName, TestDataSource dataSource)
    {
        this.projectFileSystem.SaveToFile<TestDataSource>(dataSource, projectFileSystem.TestDataRepository, $"{dataSource.DataSourceId}.dat");
        if (IsOnlineMode)
        {
            await this.testDataRepositoryClient.AddDataSourceAsync(this.automationProject.ProjectId, this.projectVersion.ToString(), groupName, dataSource);
            await SaveTestDataSourceDataAsync(dataSource);
        }
        logger.Information("Added test data source : '{0}' to version : '{1}' of automation project : '{2}'.", dataSource.Name, projectVersion, automationProject.Name);
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
        logger.Information("Updated test data source : '{0}' for version : '{1}' of automation project : '{2}'.", dataSource.Name, projectVersion, automationProject.Name);
        return dataSource;
    }

    /// <inheritdoc/>  
    public async Task DeleteTestDataSourceAsync(TestDataSource dataSource)
    {
        if (IsOnlineMode)
        {
            await this.testDataRepositoryClient.DeleteDataSourceAsync(this.automationProject.ProjectId, this.projectVersion.ToString(), dataSource.DataSourceId);
        }
        dataSource.IsDeleted = true;
        this.projectFileSystem.SaveToFile<TestDataSource>(dataSource, projectFileSystem.TestDataRepository, $"{dataSource.DataSourceId}.dat");
        logger.Information("Deleteds test data source : '{0}' from version : '{1}' of automation project : '{2}'.", dataSource.Name, projectVersion, automationProject.Name);
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
            logger.Information("Saved test data source : '{0}' for version : '{1}' of automation project : '{2}'.", dataSource.Name, projectVersion, automationProject.Name);
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
                logger.Information("Updated local copy of Test Data Source : {0}", dataSource.Name);
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
