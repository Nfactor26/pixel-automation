using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Reference.Manager.Contracts;
using Pixel.Persistence.Core.Models;
using Pixel.Persistence.Services.Client;
using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Scripting.Common.CSharp.WorkspaceManagers;
using Serilog;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TestCase = Pixel.Automation.Core.TestData.TestCase;
using TestFixture = Pixel.Automation.Core.TestData.TestFixture;

namespace Pixel.Automation.Test.Runner
{
    public class ProjectManager
    {
        private readonly ILogger logger = Log.ForContext<ProjectManager>();

        private readonly ISerializer serializer;
        private readonly IApplicationFileSystem applicationFileSystem;
        private readonly IProjectFileSystem projectFileSystem;
        private readonly ITypeProvider typeProvider;
        private readonly IEntityManager entityManager;
        private readonly ITestSessionClient sessionClient;
        private readonly ITestRunner testRunner;
        private readonly ITestSelector testSelector;
        private readonly IProjectDataManager projectDataManager;
        private readonly IProjectAssetsDataManager projectAssetsDataManager;
        private readonly IPrefabDataManager prefabDataManager;
        private readonly IScriptEngineFactory scriptEngineFactory;
        private readonly IReferenceManagerFactory referenceManagerFactory;
        private IReferenceManager referenceManager;
        private Core.Models.AutomationProject automationProject;
        private Core.Models.VersionInfo targetVersion;
        private List<TestFixture> availableFixtures = new List<TestFixture>();
        private SessionTemplate sessionTemplate;       

        public ProjectManager(IEntityManager entityManager, ISerializer serializer, IApplicationFileSystem applicationFileSystem,
            IProjectFileSystem projectFileSystem, ITypeProvider typeProvider,
            IProjectDataManager projectDataManager, IProjectAssetsDataManager projectAssetsDataManager, IPrefabDataManager prefabDataManager,
            ITestRunner testRunner, ITestSelector testSelector, ITestSessionClient sessionClient, IScriptEngineFactory scriptEngineFactory,
            IReferenceManagerFactory referenceManagerFactory)
        {
            this.entityManager = Guard.Argument(entityManager).NotNull().Value;
            this.serializer = Guard.Argument(serializer).NotNull().Value;
            this.applicationFileSystem = Guard.Argument(applicationFileSystem).NotNull().Value;
            this.projectFileSystem = Guard.Argument(projectFileSystem).NotNull().Value;
            this.typeProvider = Guard.Argument(typeProvider).NotNull().Value;     
            this.sessionClient = Guard.Argument(sessionClient).NotNull().Value;
            this.testRunner = Guard.Argument(testRunner).NotNull().Value;
            this.testSelector = Guard.Argument(testSelector).NotNull().Value;
            this.projectDataManager = Guard.Argument(projectDataManager).NotNull().Value;
            this.projectAssetsDataManager = Guard.Argument(projectAssetsDataManager).NotNull().Value;
            this.prefabDataManager = Guard.Argument(prefabDataManager).NotNull().Value;
            this.scriptEngineFactory = Guard.Argument(scriptEngineFactory).NotNull().Value;
            this.referenceManagerFactory = Guard.Argument(referenceManagerFactory).NotNull().Value;
        }

        public async Task<string> LoadProjectAsync(SessionTemplate template, string projectVersion)
        {
            Guard.Argument(template).NotNull();
            this.sessionTemplate = template;
            await LoadProjectAsync(template.ProjectId, projectVersion, template.InitFunction ?? Constants.DefaultInitFunction);
            return this.automationProject.ProjectId;
        }

        public async Task LoadProjectAsync(string projectId, string projectVersion, string initializerFunction)
        {
            Guard.Argument(projectId, nameof(projectId)).NotNull().NotEmpty();
           
            this.automationProject = serializer.Deserialize<Core.Models.AutomationProject>(this.applicationFileSystem.GetAutomationProjectFile(projectId), null);

            if(string.IsNullOrEmpty(projectVersion) || string.IsNullOrWhiteSpace(projectVersion))
            {
                projectVersion = this.automationProject.AvailableVersions.OrderBy(a => a.Version).Last().Version.ToString();
            }           
            logger.Information($"Trying to load version {projectVersion} for project : {projectId}");
            if (!Version.TryParse(projectVersion, out Version version))
            {
                throw new ArgumentException($"{nameof(projectVersion)} : {projectVersion} doesn't have a valid format");
            }
            if(!automationProject.AvailableVersions.Any(a => a.Version.Equals(version)))
            {
                logger.Error($"Version : {version} doesn't exist for project {automationProject.Name}");               
            }
            this.targetVersion = automationProject.AvailableVersions.Where(a => a.Version.Equals(version)).Single();

            this.projectFileSystem.Initialize(this.automationProject, this.targetVersion);
            this.projectAssetsDataManager.Initialize(this.automationProject, this.targetVersion);
            logger.Information("Working directory is {0}", this.projectFileSystem.WorkingDirectory);

            await this.projectDataManager.DownloadProjectDataFilesAsync(this.automationProject, (this.targetVersion as Core.Models.ProjectVersion));
            await this.projectAssetsDataManager.DownloadAllFixturesAsync();
            await this.projectAssetsDataManager.DownloadAllTestsAsync();
            await this.projectAssetsDataManager.DownloadAllTestDataSourcesAsync();

            this.referenceManager = this.referenceManagerFactory.CreateReferenceManager(this.automationProject.ProjectId, this.targetVersion.ToString(), this.projectFileSystem);
            this.entityManager.RegisterDefault<IReferenceManager>(this.referenceManager);

            foreach (var prefabReference in this.referenceManager.GetPrefabReferences().References)
            {
                await this.prefabDataManager.DownloadPrefabDataAsync( prefabReference.ApplicationId, prefabReference.PrefabId, prefabReference.Version.ToString());
            }
         
            this.entityManager.SetCurrentFileSystem(this.projectFileSystem);
            this.entityManager.RegisterDefault<IFileSystem>(this.projectFileSystem as IFileSystem);
                    
            object dataModel;
            //Load the setup data model for the project.
            if(this.targetVersion.IsPublished)
            {                
                string dataModelAssemblyFile = Path.Combine(projectFileSystem.ReferencesDirectory, targetVersion.DataModelAssembly);
                if(!File.Exists(dataModelAssemblyFile))
                {
                    throw new FileNotFoundException($"Data model assembly file {dataModelAssemblyFile} doesn't exist.");
                }
                Assembly dataModelAssembly = Assembly.LoadFrom(dataModelAssemblyFile);
                Type setupDataModel = dataModelAssembly.GetTypes().FirstOrDefault(t => t.Name.Equals(Constants.AutomationProcessDataModelName)) ?? throw new Exception($"Data model assembly {dataModelAssembly.GetName().Name} doesn't contain  type : {Constants.AutomationProcessDataModelName}");
                dataModel = Activator.CreateInstance(setupDataModel);
                logger.Information("Data model assembly loaded from {0}", dataModelAssemblyFile);
            }
            else
            {
                dataModel = CompileAndCreateDataModel();
            }
         
            ConfigureScriptEngineFactory(referenceManager, dataModel);

            var processEntity = serializer.Deserialize<Entity>(this.projectFileSystem.ProcessFile, typeProvider.GetKnownTypes());
            this.entityManager.RootEntity = processEntity;
            this.entityManager.RestoreParentChildRelation(this.entityManager.RootEntity);
            this.entityManager.Arguments = dataModel;
          
            await ExecuteInitializationScript(Constants.InitializeEnvironmentScript, string.IsNullOrEmpty(initializerFunction) ? Constants.DefaultInitFunction : initializerFunction);

            logger.Information($"Project {0} with version {1}  was loaded.", this.automationProject.Name, this.targetVersion.Version);
        }

        /// <summary>
        /// Create a workspace manager. Add all the documents from data model directory to this workspace , compile the workspace and
        /// create an instnace of data model from generated assembly.
        /// </summary>
        /// <param name="dataModelName"></param>
        /// <returns>Instance of dataModel</returns>
        protected object CompileAndCreateDataModel()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(this.projectFileSystem.ReferencesDirectory);
            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                file.Delete();
            }
            string dataModelAssembly = Path.Combine(this.projectFileSystem.ReferencesDirectory, $"{this.automationProject.Namespace}.dll");
            if(!File.Exists(dataModelAssembly))
            {              
                var workspace = new CodeWorkspaceManager(this.projectFileSystem.DataModelDirectory);
                workspace.WithAssemblyReferences(referenceManager.GetCodeEditorReferences() ?? Array.Empty<string>());
                workspace.AddProject(this.automationProject.Name, this.automationProject.Namespace, Array.Empty<string>());

                string[] dataModelFiles = Directory.GetFiles(this.projectFileSystem.DataModelDirectory, "*.cs");
                foreach (var dataModelFile in dataModelFiles)
                {
                    string documentName = Path.GetFileName(dataModelFile);
                    workspace.AddDocument(documentName, this.automationProject.Name, File.ReadAllText(dataModelFile));
                }
                using (var compilationResult = workspace.CompileProject(this.automationProject.Name, this.automationProject.Namespace))
                {
                    compilationResult.SaveAssemblyToDisk(this.projectFileSystem.ReferencesDirectory);
                }
            }

            Assembly assembly = Assembly.LoadFrom(dataModelAssembly);
            logger.Information($"Data model assembly compiled and assembly loaded from {dataModelAssembly}");
            Type typeofDataModel = assembly.GetTypes().FirstOrDefault(t => t.Name.Equals(Constants.AutomationProcessDataModelName))
                ?? throw new Exception($"Data model assembly {assembly.GetName().Name} doesn't contain  type : {Constants.AutomationProcessDataModelName}");
            return Activator.CreateInstance(typeofDataModel);
            throw new Exception($"Failed to compile data model project");
        }

        /// <summary>
        /// Setup ScriptEngineFactory with search path and assembly references
        /// </summary>
        /// <param name="referenceManager"></param>
        protected virtual void ConfigureScriptEngineFactory(IReferenceManager referenceManager, object dataModel)
        {
            this.scriptEngineFactory.WithSearchPaths(Environment.CurrentDirectory, Environment.CurrentDirectory, projectFileSystem.ReferencesDirectory)
                .WithAdditionalSearchPaths(Directory.GetDirectories(Path.Combine(AppContext.BaseDirectory, "Plugins")))
                .WithWhiteListedReferences(referenceManager.GetWhiteListedReferences())
                .WithAdditionalAssemblyReferences(referenceManager.GetScriptEngineReferences())
                .WithAdditionalAssemblyReferences(dataModel.GetType().Assembly)
                .WithAdditionalNamespaces(referenceManager.GetImportsForScripts().ToArray());
        }

        /// <summary>
        /// Execute the Initialization script for Automation process.
        /// Empty Initialization script is created if script file doesn't exist already.
        /// </summary>
        private async Task ExecuteInitializationScript(string fileName, string initializerFunction)
        {
            try
            {               
                var scriptFile = Path.Combine(this.projectFileSystem.ScriptsDirectory, fileName);
                if (!File.Exists(scriptFile))
                {
                    throw new FileNotFoundException("Script file doesn't exist.", scriptFile);
                }
                var scriptEngine = this.entityManager.GetScriptEngine();
                await scriptEngine.ExecuteFileAsync(scriptFile);
                logger.Information("Executed initialize environemnt script : {scriptFile}", scriptFile);
                await scriptEngine.ExecuteScriptAsync(initializerFunction);
                logger.Information("Executed initializer function : {initializerFunction}", initializerFunction);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to execute Initialization script {fileName}");
            }
        }
       
        public async Task LoadTestCasesAsync()
        {
            foreach (var testFixtureDirectory in Directory.GetDirectories(this.projectFileSystem.TestCaseRepository))
            {
                var testFixture = this.projectFileSystem.LoadFiles<TestFixture>(testFixtureDirectory).Single();            
                this.availableFixtures.Add(testFixture);
                await this.projectAssetsDataManager.DownloadFixtureDataAsync(testFixture);
                foreach (var testCaseDirectory in Directory.GetDirectories(testFixtureDirectory))
                {
                    var testCase = this.projectFileSystem.LoadFiles<TestCase>(testCaseDirectory).Single();
                    testFixture.Tests.Add(testCase);
                    await this.projectAssetsDataManager.DownloadTestDataAsync(testCase);
                }
            }
            logger.Information($"Found {this.availableFixtures.Select(s => s.Tests).Count()} test cases.");
        }

        public async Task ListAllAsync(string selector, IAnsiConsole console)
        {          
            await this.testSelector.Initialize(selector);
            logger.Information($"Listing tests that matches selection condition now.");
            try
            {
                var tree = new Tree($"[blue]{this.automationProject.Name} - {this.targetVersion}[/]");
                foreach (var testFixture in this.availableFixtures)
                {                   
                    if (testFixture.IsMuted)
                    {
                        tree.AddNode($"[red]{testFixture.DisplayName}[/]");
                        continue;
                    }
                    var fixtureNode = tree.AddNode($"[green]{testFixture.DisplayName}[/]");
                    foreach (var testCase in testFixture.Tests)
                    {
                        if (!testSelector.CanRunTest(testFixture, testCase))
                        {
                            fixtureNode.AddNode($"[red]{testCase.DisplayName}[/]");
                            continue;
                        }
                        fixtureNode.AddNode($"[green]{testCase.DisplayName}[/]");
                    }
                }
                console.Write(tree);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }           
        }

        public async Task RunAllAsync(string selector, IAnsiConsole console)
        {
            TestSession testSession = new TestSession(this.sessionTemplate, this.targetVersion.Version.ToString());
            List<Persistence.Core.Models.TestResult> testResults = new List<Persistence.Core.Models.TestResult>();
            testSession.Id = await sessionClient.AddSessionAsync(testSession);

            try
            {
                int counter = 0;
                await this.testSelector.Initialize(selector);
                await this.testRunner.SetUpEnvironment();

                console.MarkupLine($"[blue]{this.automationProject.Name} - {this.targetVersion}[/]");
                foreach (var testFixture in this.availableFixtures)
                {
                    console.MarkupLine($" - [cyan3]{testFixture.DisplayName}[/]");
                   
                    if (testFixture.IsMuted)
                    {                        
                        continue;
                    }
                    
                    var fixtureFiles = this.projectFileSystem.GetTestFixtureFiles(testFixture);
                    testFixture.TestFixtureEntity = this.Load<Entity>(fixtureFiles.ProcessFile);
                    testFixture.TestFixtureEntity.Name = testFixture.DisplayName;
                    testFixture.TestFixtureEntity.Tag = testFixture.FixtureId;
                    await this.testRunner.TryOpenTestFixture(testFixture);
                    foreach (var testCase in testFixture.Tests)
                    {                      
                        if (!testSelector.CanRunTest(testFixture, testCase))
                        {
                            console.MarkupLine($"  - [yellow]{testCase.DisplayName}[/]");
                            continue;
                        }
                       
                        await foreach(var testResult in  this.RunTestCaseAsync(testFixture, testCase))
                        {
                            testResult.SessionId = testSession.Id;                          
                            testResult.ExecutionOrder = ++counter;
                            await sessionClient.AddResultAsync(testResult);
                            testResults.Add(testResult);
                            if(testResult.Result == Persistence.Core.Enums.TestStatus.Success)
                            {
                                console.MarkupLine($"  - [green]{testCase.DisplayName}[/]");
                            }
                            else
                            {
                                console.MarkupLine($"  - [red]{testCase.DisplayName}[/]");
                                console.MarkupLine($"    [red]{testResult.FailureDetails.Exception} - {testResult.FailureDetails.Message}[/]");
                            }
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
                await sessionClient.UpdateSessionAsync(testSession.Id, testSession);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }
        }
       
        async IAsyncEnumerable<Pixel.Persistence.Core.Models.TestResult> RunTestCaseAsync(TestFixture fixture, TestCase testCase)
        {           
            logger.Information($"Start execution of test case : {testCase.DisplayName}");

            var testCaseFiles = this.projectFileSystem.GetTestCaseFiles(testCase);          
            testCase.TestCaseEntity = this.Load<Entity>(testCaseFiles.ProcessFile);
            testCase.TestCaseEntity.Name = testCase.DisplayName;
            testCase.TestCaseEntity.Tag = testCase.TestCaseId;

            if (await this.testRunner.TryOpenTestCase(fixture, testCase))
            {
                await foreach (var result in this.testRunner.RunTestAsync(fixture, testCase))
                {
                    var testResult = new Persistence.Core.Models.TestResult()
                    {
                        ProjectId = automationProject.ProjectId,
                        ProjectName = automationProject.Name,
                        TestId = testCase.TestCaseId,
                        TestName = testCase.DisplayName,
                        FixtureId = fixture.FixtureId,
                        FixtureName = fixture.DisplayName,
                        Result = (Persistence.Core.Enums.TestStatus)((int)result.Result),
                        ExecutedOn = DateTime.Today.ToUniversalTime(),
                        ExecutionTime = result.ExecutionTime.TotalSeconds                        
                    };
                    if(result.Result == TestStatus.Failed)
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
            T entity = serializer.Deserialize<T>(fileName, typeProvider.GetKnownTypes());
            return entity;
        }
    }
}
