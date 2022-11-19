using Pixel.Automation.Core.Models;
using Pixel.Automation.Core.TestData;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client.Interfaces;

public interface IProjectDataManager
{

    /// <summary>
    /// Load all the automatin project from disk and return loaded projects
    /// </summary>
    /// <returns></returns>
    IEnumerable<AutomationProject> GetAllProjects();

    /// <summary>
    /// Add a new project
    /// </summary>
    /// <param name="automationProject"></param>       
    /// <returns></returns>
    Task AddProjectAsync(AutomationProject automationProject);

    /// <summary>
    /// Add a new version to project by cloning data from an existing version
    /// </summary>
    /// <param name="automationProject"></param>
    /// <param name="newVersion"></param>
    /// <param name="cloneFrom"></param>
    /// <returns></returns>
    Task AddProjectVersionAsync(AutomationProject automationProject, ProjectVersion newVersion, ProjectVersion cloneFrom);

    /// <summary>
    /// Update details of an existing version of AutomationProject
    /// </summary>
    /// <param name=""></param>
    /// <param name="projectVersion"></param>
    /// <returns></returns>
    Task UpdateProjectVersionAsync(AutomationProject automationProject, ProjectVersion projectVersion);

    /// <summary>
    /// Add a data file to a given version of project
    /// </summary>
    /// <param name="automationProject"></param>
    /// <param name="projectVersion"></param>
    /// <param name="fileName"></param>
    /// <param name="filePath"></param>
    /// <param name="tag"></param>
    /// <returns></returns>
    Task AddOrUpdateDataFileAsync(AutomationProject automationProject, ProjectVersion projectVersion, string filePath, string tag);

    /// <summary>
    /// Delete a data file belonging to given version of project
    /// </summary>
    /// <param name="automationProject"></param>
    /// <param name="projectVersion"></param>
    /// <param name="filePath"></param>
    /// <returns></returns>
    Task DeleteDataFileAsync(AutomationProject automationProject, ProjectVersion projectVersion, string filePath);

    /// <summary>
    /// Download all the newer project files
    /// </summary>
    /// <returns></returns>
    Task DownloadProjectsAsync();


    /// <summary>
    /// Download all the data files belong to the version of AutomationProject being managed
    /// </summary>
    /// <returns></returns>
    Task DownloadProjectDataFilesAsync(AutomationProject automationProject, ProjectVersion projectVersion);

    /// <summary>
    /// Save the data files belonging to the version of AutomationProject being managed
    /// </summary>
    /// <returns></returns>
    Task SaveProjectDataAsync(AutomationProject automationProject, ProjectVersion projectVersion);

}

/// <summary>
/// Defines contract for managing test cases, test fixutres and test data asssociated with a specific version of Project 
/// </summary>
public interface IProjectAssetsDataManager : ITestCaseManager, ITestFixtureManager, ITestDataManager
{
    /// <summary>
    /// Initialize the ProjectDataManager with the AutomationProject and it's version to manage
    /// </summary>
    /// <param name="automationProject"></param>
    /// <param name="projectVersion"></param>
    void Initialize(AutomationProject automationProject, VersionInfo projectVersion);
}

public interface ITestCaseManager
{    

    /// <summary>
    /// Add a new TestCase
    /// </summary>
    /// <param name="testCase"></param>
    /// <returns></returns>
    Task<TestCase> AddTestCaseAsync(TestCase testCase);

    /// <summary>
    /// Update an existing TestCase
    /// </summary>
    /// <param name="testCase"></param>
    /// <returns></returns>
    Task<TestCase> UpdateTestCaseAsync(TestCase testCase);

    /// <summary>
    /// (soft) Delete an existing TestCase
    /// </summary>
    /// <param name="testCase"></param>
    /// <returns></returns>
    Task DeleteTestCaseAsync(TestCase testCase);

    /// <summary>
    /// Save data files belonging to a TestFixture
    /// </summary>
    /// <param name="testFixture"></param>
    /// <returns></returns>
    Task SaveTestDataAsync(TestCase testCase);

    /// <summary>
    /// Download data files belonging to a TestCase
    /// </summary>
    /// <param name="testCase"></param>
    /// <returns></returns>
    Task DownloadTestDataAsync(TestCase testCase);

    /// <summary>
    /// Download all the TestCases available for the version of AutomationProject being managed
    /// </summary>
    /// <returns></returns>
    Task DownloadAllTestsAsync();

}

public interface ITestFixtureManager
{    

    /// <summary>
    /// Add a new TestFixture
    /// </summary>
    /// <param name="testFixture"></param>
    /// <returns></returns>
    Task<TestFixture> AddTestFixtureAsync(TestFixture testFixture);

    /// <summary>
    /// Update an existing TestFixture
    /// </summary>
    /// <param name="testFixture"></param>
    /// <returns></returns>
    Task<TestFixture> UpdateTestFixtureAsync(TestFixture testFixture);

    /// <summary>
    /// (soft) Delete an existing TestFixture
    /// </summary>
    /// <param name="testFixture"></param>
    /// <returns></returns>
    Task DeleteTestFixtureAsync(TestFixture testFixture);

    /// <summary>
    /// Save data files belonging to a TestFixture
    /// </summary>
    /// <param name="testFixture"></param>
    /// <returns></returns>
    Task SaveFixtureDataAsync(TestFixture testFixture);

    /// <summary>
    /// Download data files belonging to a TestFixture
    /// </summary>
    /// <param name="testFixture"></param>
    /// <returns></returns>
    Task DownloadFixtureDataAsync(TestFixture testFixture);

    /// <summary>
    /// Download all the TestFixtures available for the version of AutomationProject being managed
    /// </summary>
    /// <returns></returns>
    Task DownloadAllFixturesAsync();
}

public interface ITestDataManager
{
    
    /// <summary>
    /// Add a new Test Data Source
    /// </summary>
    /// <param name="testCase"></param>
    /// <returns></returns>
    Task<TestDataSource> AddTestDataSourceAsync(TestDataSource dataSource);

    /// <summary>
    /// Update an existing Test Data Source
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    Task<TestDataSource> UpdateTestDataSourceAsync(TestDataSource dataSource);

    /// <summary>
    /// (soft) Delete an existing Test Data Source
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    Task DeleteTestDataSourceAsync(TestDataSource dataSource);

    /// <summary>
    /// Save data files belonging to a Test Data Source
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    Task SaveTestDataSourceDataAsync(TestDataSource dataSource);

    /// <summary>
    /// Download all the Test Data Source and associated data files
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    Task DownloadAllTestDataSourcesAsync();

}

