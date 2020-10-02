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
        private ITestRunner testRunner;

        private AutomationProject automationProject;
        private List<TestFixture> availableFixtures = new List<TestFixture>();

        public ProjectManager(IEntityManager entityManager, ISerializer serializer, IProjectFileSystem fileSystem, ITypeProvider typeProvider, ITestSessionClient sessionClient)
        {
            Guard.Argument(entityManager).NotNull();
            Guard.Argument(serializer).NotNull();
            Guard.Argument(fileSystem).NotNull();
            Guard.Argument(typeProvider).NotNull();
            Guard.Argument(sessionClient).NotNull();

            this.entityManager = entityManager;
            this.serializer = serializer;
            this.fileSystem = fileSystem;
            this.typeProvider = typeProvider;
            this.sessionClient = sessionClient;
        }



        public void LoadProject(string projectName, string projectVersion)
        {
            Guard.Argument(projectName).NotNull().NotEmpty();
            Guard.Argument(projectVersion).NotNull().NotEmpty();

            logger.Information($"Trying to load version {projectVersion} for project : {projectName}");

            this.automationProject = serializer.Deserialize<AutomationProject>(this.fileSystem.ProjectFile, null);
            if (!Version.TryParse(projectVersion, out Version version))
            {
                throw new ArgumentException($"{nameof(projectVersion)} : {projectVersion} doesn't have a valid format");
            }
            var versionInfo = automationProject.DeployedVersions.Where(a => a.Version.Equals(version)).Single();

            this.fileSystem.Initialize(automationProject.Name, versionInfo);

            Assembly dataModelAssembly = Assembly.LoadFrom(Path.Combine(fileSystem.ReferencesDirectory, versionInfo.DataModelAssembly));
            Type processDataModelType = dataModelAssembly.GetTypes().FirstOrDefault(t => t.Name.Equals("DataModel")) ?? throw new Exception($"Data model assembly {dataModelAssembly.GetName().Name} doesn't contain  type : DataModel");

            this.entityManager.SetCurrentFileSystem(this.fileSystem);
            this.entityManager.RegisterDefault<IProjectFileSystem>(this.fileSystem);
            this.entityManager.Arguments = Activator.CreateInstance(processDataModelType);

            var processEntity = serializer.Deserialize<Entity>(this.fileSystem.ProcessFile, typeProvider.GetAllTypes());
            processEntity.EntityManager = this.entityManager;
            this.entityManager.RootEntity = processEntity;
            this.entityManager.RestoreParentChildRelation(this.entityManager.RootEntity);

            this.testRunner = entityManager.GetServiceOfType<ITestRunner>();

            logger.Information($"Project load completed.");
        }

        public void LoadTestCases()
        {
            //List<TestCategory> testCategories = new List<TestCategory>();
            //foreach (var testCategory in this.fileSystem.LoadFiles<TestCategory>(this.fileSystem.TestCaseRepository))
            //{
            //    testCategories.Add(testCategory);
            //}

            //foreach (var testDirectory in Directory.GetDirectories(this.fileSystem.TestCaseRepository))
            //{
            //    foreach (var testCase in this.fileSystem.LoadFiles<TestCase>(testDirectory))
            //    {
            //        var ownerCategory = testCategories.Single(a => a.Id.Equals(testCase.CategoryId));
            //        ownerCategory.Tests.Add(testCase);
            //    }
            //}
            //this.availableFixtures.AddRange(testCategories);

        }

        public async Task Setup()
        {
            await this.testRunner.SetUp();
        }

        public async Task TearDown()
        {
            await this.testRunner.TearDown();
        }


        public async Task RunAll(ITestSelector testSelector)
        {
            TestSession testSession = new TestSession(automationProject.Name);

            try
            {
                await this.Setup();

                foreach (var testCategory in this.availableFixtures)
                {
                    //if(!testSelector.CanRunCategory(testCategory))
                    //{
                    //    continue;
                    //}

                    foreach (var testCase in testCategory.Tests)
                    {                      
                        if (!testSelector.CanRunTest(testCase))
                        {
                            continue;
                        }

                        var testResult = await this.RunTestCaseAsync(testCategory, testCase);
                        testSession.TestResultCollection.Add(testResult);
                        logger.Information($"Test case : {testCase.DisplayName} completed with result {testResult.Result} in time {testResult.ExecutionTime}");
                        if (testResult.Result == Persistence.Core.Models.TestState.Failed)
                        {
                            logger.Warning($"Test case failed with error : {testResult.ErrorMessage ?? "unknown" }");
                        }

                    }
                }             
                
                await this.TearDown();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }

            try
            {
                await sessionClient.AddSession(testSession);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }
        }
       
        public async Task<Persistence.Core.Models.TestResult> RunTestCaseAsync(TestFixture fixture, TestCase testCase)
        {
            logger.Information($"Start execution of test case : {testCase.DisplayName}");

            string testCaseProcessFile = Path.Combine(this.fileSystem.TestCaseRepository, testCase.Id, "TestAutomation.proc");
            testCase.TestCaseEntity = serializer.Deserialize<Entity>(testCaseProcessFile, typeProvider.GetAllTypes());

            if (await this.testRunner.TryOpenTestCase(fixture, testCase))
            {
                await foreach (var result in this.testRunner.RunTestAsync(fixture, testCase))
                {
                    var testResult = new Persistence.Core.Models.TestResult()
                    {
                        TestId = testCase.Id,
                        TestName = testCase.DisplayName,
                        CategoryId = fixture.Id,
                        CategoryName = fixture.DisplayName,
                        Result = (Persistence.Core.Models.TestState)((int)result.Result),
                        ExecutionTime = result.ExecutionTime.TotalSeconds,
                        ErrorMessage = result.ErrorMessage
                    };
                    return testResult;
                }
            }
            throw new Exception($"Failed to open test case : {testCase}");
        }
    }
}
