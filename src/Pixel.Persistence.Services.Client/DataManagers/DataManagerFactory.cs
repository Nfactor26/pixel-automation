using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Persistence.Services.Client.Interfaces;

namespace Pixel.Persistence.Services.Client;

public class DataManagerFactory : IDataManagerFactory
{
    private readonly ISerializer serializer;
    private readonly ApplicationSettings applicationSettings;
    private readonly IApplicationFileSystem applicationFileSystem;
    private readonly IProjectFilesRepositoryClient projectFilesClient;
    private readonly IFixturesRepositoryClient fixturesClient;
    private readonly ITestsRepositoryClient testsClient;
    private readonly ITestDataRepositoryClient testsDataClient;

    public DataManagerFactory(ApplicationSettings applicationSettings, IApplicationFileSystem applicationFileSystem, ISerializer serializer,
        IProjectFilesRepositoryClient projectFilesClient, IFixturesRepositoryClient fixturesClient,
        ITestsRepositoryClient testsClient, ITestDataRepositoryClient testsDataClient)
    {
        this.applicationSettings = Guard.Argument(applicationSettings, nameof(applicationSettings)).NotNull();
        this.applicationFileSystem = Guard.Argument(applicationFileSystem, nameof(applicationFileSystem)).NotNull().Value;
        this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
        this.projectFilesClient = Guard.Argument(projectFilesClient, nameof(projectFilesClient)).NotNull().Value;
        this.fixturesClient = Guard.Argument(fixturesClient, nameof(fixturesClient)).NotNull().Value;
        this.testsClient = Guard.Argument(testsClient, nameof(testsClient)).NotNull().Value;
        this.testsDataClient = Guard.Argument(testsDataClient, nameof(testsDataClient)).NotNull().Value;
    }

    ///<inheritdoc/>
    public IProjectAssetsDataManager CreateProjectAssetsDataManager(AutomationProject automationProject, VersionInfo versionInfo, IProjectFileSystem fileSystem)
    {
        var assetsDataManager = new TestAndFixtureAndTestDataManager(serializer, projectFilesClient, fixturesClient, testsClient, applicationFileSystem,
            fileSystem, applicationSettings, testsDataClient);
        assetsDataManager.Initialize(automationProject, versionInfo);
        return assetsDataManager;
    }
}
