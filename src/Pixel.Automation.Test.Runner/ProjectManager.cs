using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Core.TestData;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Services.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pixel.Automation.Test.Runner
{
    public class ProjectManager
    {
        private readonly ILogger logger = Log.ForContext<ProjectManager>();

        private readonly ISerializer serializer;
        private readonly IProjectFileSystem fileSystem;
        private readonly ITypeProvider typeProvider;
        private readonly IEntityManager entityManager;
        private readonly ITestSessionClient sessionClient;
        private readonly ITestRunner testRunner;
        private readonly ITestSelector testSelector;
        private readonly IApplicationDataManager applicationDataManager;       
        private AutomationProject automationProject;
        private VersionInfo targetVersion;
        private List<TestFixture> availableFixtures = new List<TestFixture>();
        private SessionTemplate sessionTemplate;

        public ProjectManager(IEntityManager entityManager, ISerializer serializer, IProjectFileSystem fileSystem, ITypeProvider typeProvider,
            IApplicationDataManager applicationDataManager, ITestRunner testRunner,
            ITestSelector testSelector, ITestSessionClient sessionClient)
        {
            this.entityManager = Guard.Argument(entityManager).NotNull().Value;
            this.serializer = Guard.Argument(serializer).NotNull().Value;
            this.fileSystem = Guard.Argument(fileSystem).NotNull().Value;
            this.typeProvider = Guard.Argument(typeProvider).NotNull().Value;     
            this.sessionClient = Guard.Argument(sessionClient).NotNull().Value;
            this.testRunner = Guard.Argument(testRunner).NotNull().Value;
            this.testSelector = Guard.Argument(testSelector).NotNull().Value;
            this.applicationDataManager = Guard.Argument(applicationDataManager).NotNull().Value;
        }

        public async Task<string> LoadProjectAsync(SessionTemplate template)
        {
            Guard.Argument(template).NotNull();
            this.sessionTemplate = template;
            await LoadProjectAsync(template.ProjectName, template.ProjectVersion, template.InitializeScript);
            return this.automationProject.ProjectId;
        }

        public async Task LoadProjectAsync(string projectName, string projectVersion, string initializationScript)
        {
            Guard.Argument(projectName).NotNull().NotEmpty();
            Guard.Argument(projectVersion).NotNull().NotEmpty();

            logger.Information($"Trying to load version {projectVersion} for project : {projectName}");

            this.automationProject = serializer.Deserialize<AutomationProject>(Path.Combine(this.applicationDataManager.GetProjectsRootDirectory(), projectName, $"{projectName}.atm"), null);
            if (!Version.TryParse(projectVersion, out Version version))
            {
                throw new ArgumentException($"{nameof(projectVersion)} : {projectVersion} doesn't have a valid format");
            }
            if(!automationProject.DeployedVersions.Any(a => a.Version.Equals(version)))
            {
                logger.Error($"Version : {version} doesn't exist for project {automationProject.Name}");               
            }
            this.targetVersion = automationProject.DeployedVersions.Where(a => a.Version.Equals(version)).Single();

            await this.applicationDataManager.DownloadProjectDataAsync(this.automationProject, targetVersion);
            this.fileSystem.Initialize(automationProject.Name, targetVersion);

            if (!string.IsNullOrEmpty(initializationScript) && !File.Exists(Path.Combine(fileSystem.ScriptsDirectory, initializationScript)))
            {
                throw new FileNotFoundException($"Script file {initializationScript} doesn't exist.");
            }

            //Load the setup data model for the project.
            Assembly dataModelAssembly = Assembly.LoadFrom(Path.Combine(fileSystem.ReferencesDirectory, targetVersion.DataModelAssembly));
            Type setupDataModel = dataModelAssembly.GetTypes().FirstOrDefault(t => t.Name.Equals(Constants.ProcessDataModelName)) ?? throw new Exception($"Data model assembly {dataModelAssembly.GetName().Name} doesn't contain  type : {Constants.ProcessDataModelName}");

            this.entityManager.SetCurrentFileSystem(this.fileSystem);
            this.entityManager.RegisterDefault<IProjectFileSystem>(this.fileSystem);
            this.entityManager.Arguments = Activator.CreateInstance(setupDataModel);

            var processEntity = serializer.Deserialize<Entity>(this.fileSystem.ProcessFile, typeProvider.GetAllTypes());
            this.entityManager.RootEntity = processEntity;
            this.entityManager.RestoreParentChildRelation(this.entityManager.RootEntity);


            var scriptEngine = this.entityManager.GetScriptEngine();
            initializationScript = string.IsNullOrEmpty(initializationScript) ? Constants.InitializeEnvironmentScript : initializationScript;
            string scriptFile = fileSystem.GetRelativePath(Path.Combine(fileSystem.ScriptsDirectory, initializationScript));
            await scriptEngine.ExecuteFileAsync(scriptFile);


            logger.Information($"Project load completed.");
        }

        public void LoadTestCases()
        {
            foreach (var testFixtureDirectory in Directory.GetDirectories(this.fileSystem.TestCaseRepository))
            {
                var testFixture = this.fileSystem.LoadFiles<TestFixture>(testFixtureDirectory).Single();            
                this.availableFixtures.Add(testFixture);

                foreach (var testCase in this.fileSystem.LoadFiles<TestCase>(testFixtureDirectory))
                {
                    testFixture.Tests.Add(testCase);
                }
            }
            logger.Information($"Found {this.availableFixtures.Select(s => s.Tests).Count()} test cases.");
        }

        public async Task ListAllAsync(string selector)
        {          
            await this.testSelector.Initialize(selector);
            logger.Information($"Listing tests that matches selection condition now.");
            try
            {
              
                foreach (var testFixture in this.availableFixtures)
                {
                    if (testFixture.IsMuted)
                    {
                        continue;
                    }        
                    Console.WriteLine($"{testFixture.DisplayName}");
                    foreach (var testCase in testFixture.Tests)
                    {
                        if (!testSelector.CanRunTest(testFixture, testCase))
                        {
                            continue;
                        }
                        Console.WriteLine($"    -- {testCase.DisplayName}");
                    }
                }             
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }
        }

        public async Task RunAllAsync(string selector)
        {
            TestSession testSession = new TestSession(this.sessionTemplate);
            List<Persistence.Core.Models.TestResult> testResults = new List<Persistence.Core.Models.TestResult>();
            testSession.SessionId = await sessionClient.AddSessionAsync(testSession);

            try
            {
                int counter = 0;
                await this.testSelector.Initialize(selector);
                await this.testRunner.SetUpEnvironment();

                foreach (var testFixture in this.availableFixtures)
                {
                    if(testFixture.IsMuted)
                    {
                        continue;
                    }

                    ITestCaseFileSystem testCaseFileSystem = this.fileSystem.CreateTestCaseFileSystemFor(testFixture.Id);
                    testFixture.TestFixtureEntity = this.Load<Entity>(testCaseFileSystem.FixtureProcessFile);
                    testFixture.TestFixtureEntity.Name = testFixture.DisplayName;
                    testFixture.TestFixtureEntity.Tag = testFixture.Id;
                    await this.testRunner.TryOpenTestFixture(testFixture);
                    foreach (var testCase in testFixture.Tests)
                    {                      
                        if (!testSelector.CanRunTest(testFixture, testCase))
                        {
                            continue;
                        }
                       
                        await foreach(var testResult in  this.RunTestCaseAsync(testFixture, testCase))
                        {
                            testResult.SessionId = testSession.SessionId;                          
                            testResult.ExecutionOrder = ++counter;
                            await sessionClient.AddResultAsync(testResult);
                            testResults.Add(testResult);
                        }
                                
                    }
                    await this.testRunner.TryCloseTestFixture(testFixture);
                }

                await this.testRunner.TearDownEnvironment();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }
           
            try
            {
                testSession.OnFinished(testResults);
                await sessionClient.UpdateSessionAsync(testSession.SessionId, testSession);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }
        }
       
        async IAsyncEnumerable<Pixel.Persistence.Core.Models.TestResult> RunTestCaseAsync(TestFixture fixture, TestCase testCase)
        {           
            logger.Information($"Start execution of test case : {testCase.DisplayName}");

            ITestCaseFileSystem testCaseFileSystem = this.fileSystem.CreateTestCaseFileSystemFor(fixture.Id);
            string testCaseProcessFile = testCaseFileSystem.GetTestProcessFile(testCase.Id);
            testCase.TestCaseEntity = this.Load<Entity>(testCaseProcessFile);
            testCase.TestCaseEntity.Name = testCase.DisplayName;
            testCase.TestCaseEntity.Tag = testCase.Id;

            if (await this.testRunner.TryOpenTestCase(fixture, testCase))
            {
                await foreach (var result in this.testRunner.RunTestAsync(fixture, testCase))
                {
                    var testResult = new Persistence.Core.Models.TestResult()
                    {
                        ProjectId = automationProject.ProjectId,
                        ProjectName = automationProject.Name,
                        TestId = testCase.Id,
                        TestName = testCase.DisplayName,
                        FixtureId = fixture.Id,
                        FixtureName = fixture.DisplayName,
                        Result = (Persistence.Core.Enums.TestStatus)((int)result.Result),
                        ExecutedOn = DateTime.Today.ToUniversalTime(),
                        ExecutionTime = result.ExecutionTime.TotalSeconds                        
                    };
                    if(result.Result == Core.TestData.TestStatus.Failed)
                    {
                        testResult.FailureDetails = new FailureDetails(result.Error);
                        logger.Warning($"Test case failed with error : {testResult.FailureDetails.Message}");
                    }
                    logger.Information($"Test case : {testCase.DisplayName} completed with result {testResult.Result} in time {testResult.ExecutionTime}");                 
                    yield return testResult;
                }
                await this.testRunner.TryCloseTestCase(fixture, testCase);
                yield break;
            }
            throw new Exception($"Failed to open test case : {testCase}");
        }

        public T Load<T>(string fileName) where T : new()
        {            
            string fileContents = File.ReadAllText(fileName);

            //TODO : Check if deployed files have _n with assembly name that needs to be replaced
            string dataModelAssemblyName = this.entityManager.Arguments.GetType().Assembly.GetName().Name;
            int indexOfUnderScoreInAssemblyName = dataModelAssemblyName.LastIndexOf('_');

            string assemblyNameWithoutIteration =  indexOfUnderScoreInAssemblyName > 0 ? dataModelAssemblyName.Substring(0, indexOfUnderScoreInAssemblyName) : dataModelAssemblyName;

            Regex regex = new Regex($"({assemblyNameWithoutIteration})(_\\d+)");
            fileContents = regex.Replace(fileContents, (m) =>
            {
                return m.Value.Replace($"{m.Groups[0].Value}", $"{dataModelAssemblyName}");
            });

            File.WriteAllText(fileName, fileContents);
            T entity = serializer.Deserialize<T>(fileName, typeProvider.GetAllTypes());
            return entity;
        }
    }
}
