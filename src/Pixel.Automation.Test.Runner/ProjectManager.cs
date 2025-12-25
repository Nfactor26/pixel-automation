using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Reference.Manager.Contracts;
using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Persistence.Services.Client.Models;
using Pixel.Scripting.Common.CSharp.WorkspaceManagers;
using Serilog;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TestCase = Pixel.Automation.Core.TestData.TestCase;
using TestFixture = Pixel.Automation.Core.TestData.TestFixture;
using TestResult = Pixel.Persistence.Services.Client.Models.TestResult;

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
        private readonly ITestRunner testRunner;
        private readonly ITestSelector testSelector;
        private readonly IProjectDataManager projectDataManager;
        private readonly IProjectAssetsDataManager projectAssetsDataManager;
        private readonly IPrefabDataManager prefabDataManager;
        private readonly IScriptEngineFactory scriptEngineFactory;
        private readonly IReferenceManagerFactory referenceManagerFactory;
        private readonly TestSessionManager sessionManager;
        private IReferenceManager referenceManager;
        private Core.Models.AutomationProject automationProject;
        private Core.Models.VersionInfo targetVersion;
        private List<TestFixture> availableFixtures = new List<TestFixture>();
        private SessionTemplate sessionTemplate;       

        public ProjectManager(IEntityManager entityManager, ISerializer serializer, IApplicationFileSystem applicationFileSystem,
            IProjectFileSystem projectFileSystem, ITypeProvider typeProvider,
            IProjectDataManager projectDataManager, IProjectAssetsDataManager projectAssetsDataManager, IPrefabDataManager prefabDataManager,
            ITestRunner testRunner, ITestSelector testSelector, TestSessionManager sessionManager, IScriptEngineFactory scriptEngineFactory,
            IReferenceManagerFactory referenceManagerFactory)
        {
            this.entityManager = Guard.Argument(entityManager).NotNull().Value;
            this.serializer = Guard.Argument(serializer).NotNull().Value;
            this.applicationFileSystem = Guard.Argument(applicationFileSystem).NotNull().Value;
            this.projectFileSystem = Guard.Argument(projectFileSystem).NotNull().Value;
            this.typeProvider = Guard.Argument(typeProvider).NotNull().Value;     
            this.sessionManager = Guard.Argument(sessionManager).NotNull();
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
            Guard.Argument(template, nameof(template)).NotNull();
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(LoadProjectAsync), ActivityKind.Internal))
            {
                this.sessionTemplate = template;
                await LoadProjectAsync(template.ProjectId, projectVersion, template.InitFunction ?? Constants.DefaultInitFunction);
                return this.automationProject.ProjectId;
            }          
        }

        async Task LoadProjectAsync(string projectId, string projectVersion, string initializerFunction)
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

            await this.projectDataManager.DownloadProjectDataFilesAsync(this.automationProject, this.targetVersion);
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
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(CompileAndCreateDataModel), ActivityKind.Internal))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(this.projectFileSystem.ReferencesDirectory);
                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    file.Delete();
                }
                string dataModelAssembly = Path.Combine(this.projectFileSystem.ReferencesDirectory, $"{this.automationProject.Namespace}.dll");
                if (!File.Exists(dataModelAssembly))
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
        }

        /// <summary>
        /// Setup ScriptEngineFactory with search path and assembly references
        /// </summary>
        /// <param name="referenceManager"></param>
        protected virtual void ConfigureScriptEngineFactory(IReferenceManager referenceManager, object dataModel)
        {
            this.scriptEngineFactory.WithSearchPaths(Environment.CurrentDirectory, Environment.CurrentDirectory, projectFileSystem.ReferencesDirectory)
                .WithAdditionalSearchPaths(Directory.GetDirectories(Path.Combine(AppContext.BaseDirectory, "Plugins")))
                .WithAdditionalSearchPaths(Directory.GetDirectories(Path.Combine(AppContext.BaseDirectory, "Scripts")))
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
                if(testFixture.IsDeleted)
                {
                    continue;
                }
                this.availableFixtures.Add(testFixture);
                await this.projectAssetsDataManager.DownloadFixtureDataAsync(testFixture);
                foreach (var testCaseDirectory in Directory.GetDirectories(testFixtureDirectory))
                {
                    var testCase = this.projectFileSystem.LoadFiles<TestCase>(testCaseDirectory).Single();
                    if(testCase.IsDeleted || !testFixture.TestCases.Contains(testCase.TestCaseId))
                    {
                        continue;
                    }
                    testFixture.Tests.Add(testCase);
                    await this.projectAssetsDataManager.DownloadTestDataAsync(testCase);
                }
            }
            logger.Information($"Found {this.availableFixtures.Select(s => s.Tests).Count()} test cases.");
        }

        public async Task ListAllAsync(string selector, IAnsiConsole console)
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(ListAllAsync), ActivityKind.Internal))
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
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                }
            }                
        }

        public async Task RunAllAsync(string selector, IAnsiConsole console)
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(RunAllAsync), ActivityKind.Internal))
            {
                activity?.SetTag("Selector", selector);
                logger.Information("Selector is {0}", selector);
              
                TestSession testSession = new TestSession(this.sessionTemplate, this.targetVersion.Version.ToString());
                List<TestResult> testResults = new();
                testSession.Id = await sessionManager.AddSessionAsync(testSession);

                try
                {
                    int counter = 0;
                  
                    using (Telemetry.DefaultSource?.StartActivity("SetUpEnvironment", ActivityKind.Internal))
                    {                        
                        await this.testSelector.Initialize(selector);
                        await this.testRunner.SetUpEnvironment();
                    }

                    using (Telemetry.DefaultSource?.StartActivity("RunTestCases", ActivityKind.Internal))
                    {
                        console.MarkupLine($"[blue]{this.automationProject.Name} - {this.targetVersion}[/]");
                        foreach (var testFixture in this.availableFixtures)
                        {
                            console.MarkupLine($" - [cyan3]{testFixture.DisplayName}[/]");

                            if (testFixture.IsMuted)
                            {
                                continue;
                            }

                            using (var setupFixtureActivity = Telemetry.DefaultSource?.StartActivity("OpenAndSetupFixture", ActivityKind.Internal))
                            {
                                setupFixtureActivity?.AddTag("FixtureName", testFixture.DisplayName);
                                var fixtureFiles = this.projectFileSystem.GetTestFixtureFiles(testFixture);
                                testFixture.TestFixtureEntity = this.Load<Entity>(fixtureFiles.ProcessFile);
                                testFixture.TestFixtureEntity.Name = testFixture.DisplayName;
                                testFixture.TestFixtureEntity.Tag = testFixture.FixtureId;
                                await this.testRunner.TryOpenTestFixture(testFixture);
                                await this.testRunner.OneTimeSetUp(testFixture);
                            }                            
                        
                            foreach (var testCase in testFixture.Tests)
                            {
                                if (!testSelector.CanRunTest(testFixture, testCase))
                                {
                                    console.MarkupLine($"  - [yellow]{testCase.DisplayName}[/]");
                                    continue;
                                }

                                await foreach (var testResult in this.RunTestCaseAsync(testFixture, testCase))
                                {
                                    testResult.SessionId = testSession.Id;
                                    testResult.ExecutionOrder = ++counter;
                                    var result = await sessionManager.AddResultAsync(testResult);
                                    testResult.Id = result.Id;
                                    await UploadTraceImageFiles(result);
                                    testResults.Add(testResult);
                                    if (testResult.Result == TestStatus.Success)
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

                            using (var tearDownFixtureActivity = Telemetry.DefaultSource?.StartActivity("TearDownAndCloseFixture", ActivityKind.Internal))
                            {
                                tearDownFixtureActivity?.AddTag("FixtureName", testFixture.DisplayName);
                                await this.testRunner.OneTimeTearDown(testFixture);
                                await this.testRunner.TryCloseTestFixture(testFixture);
                            }                           
                        }
                    }                    

                    using (Telemetry.DefaultSource?.StartActivity("TearDownEnvironment", ActivityKind.Internal))
                    {
                        await this.testRunner.TearDownEnvironment();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message, ex);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                }

                using (Telemetry.DefaultSource?.StartActivity("UpdateSession", ActivityKind.Internal))
                {
                    try
                    {
                        testSession.OnFinished(testResults);
                        PrintSessionDetails(console, testSession);
                        await sessionManager.UpdateSessionAsync(testSession.Id, testSession);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message, ex);
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    }
                }                
            }            
        }
       
        async IAsyncEnumerable<TestResult> RunTestCaseAsync(TestFixture fixture, TestCase testCase)
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity($"Run Test Case - {testCase.DisplayName}", ActivityKind.Internal))
            {
                activity?.AddTag("TestName", testCase.DisplayName);
                TraceManager.StartCapture();
                logger.Information($"Start execution of test case : {testCase.DisplayName}");

                var testCaseFiles = this.projectFileSystem.GetTestCaseFiles(testCase);
                testCase.TestCaseEntity = this.Load<Entity>(testCaseFiles.ProcessFile);
                testCase.TestCaseEntity.Name = testCase.DisplayName;
                testCase.TestCaseEntity.Tag = testCase.TestCaseId;

                if (await this.testRunner.TryOpenTestCase(fixture, testCase))
                {
                    await foreach (var result in this.testRunner.RunTestAsync(fixture, testCase))
                    {
                        var testResult = new TestResult()
                        {
                            ProjectId = automationProject.ProjectId,
                            ProjectName = automationProject.Name,
                            ProjectVersion = targetVersion.ToString(),
                            TestId = testCase.TestCaseId,
                            TestName = testCase.DisplayName,
                            FixtureId = fixture.FixtureId,
                            FixtureName = fixture.DisplayName,
                            TestData = result.TestData,
                            Result = result.Result,
                            ExecutedOn = result.StartTime,
                            ExecutionTime = result.ExecutionTime.TotalSeconds
                        };
                        if (result.Result == TestStatus.Failed)
                        {
                            testResult.FailureDetails = new FailureDetails(result.Error);
                            logger.Warning($"Test case failed with error : {testResult.FailureDetails.Message}");
                            activity?.SetStatus(ActivityStatusCode.Error, result.Error.Message);
                        }
                        else
                        {
                            logger.Information($"Test case : {testCase.DisplayName} completed with result {testResult.Result} in time {testResult.ExecutionTime}");
                        }
                        foreach (var traceData in TraceManager.EndCapture())
                        {
                            switch(traceData.TraceType)
                            {
                                case Core.Enums.TraceType.Message:
                                    testResult.Traces.Add(new MessageTraceData(traceData.RecordedAt, traceData.TraceLevel, traceData.Content));
                                    break;
                                case Core.Enums.TraceType.Image:
                                    testResult.Traces.Add(new ImageTraceData(traceData.RecordedAt, Core.Enums.TraceLevel.Information, traceData.Content));
                                    break;
                            }                           
                        }
                     
                        yield return testResult;
                    }
                    await this.testRunner.TryCloseTestCase(fixture, testCase);
                    yield break;
                }
                throw new Exception($"Failed to open test case : {testCase}");
            }             
        }

        async Task UploadTraceImageFiles(TestResult testResult)
        {
            try
            {          
                List<string> filesToAdd = new();
                foreach (var trace in testResult.Traces)
                {
                    if (trace is ImageTraceData stepTraceImage)
                    {
                        string imageFile = Path.Combine(this.entityManager.GetCurrentFileSystem().TempDirectory, stepTraceImage.ImageFile);
                        if (File.Exists(imageFile))
                        {
                            filesToAdd.Add(imageFile);
                        }                                     
                    }
                }
                try
                {
                    await sessionManager.AddTraceImagesAsync(testResult, filesToAdd);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while trying to uploade trace image files");
                }
                foreach(var file in filesToAdd)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Failed to delete trace image file : '{0}'", file);
                    }
                }                
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to uploade trace image files");
            }
        }

        T Load<T>(string fileName) where T : new()
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

        void PrintSessionDetails(IAnsiConsole console, TestSession testSession)
        {
            console.WriteLine();
            console.Write(new BarChart()
                .Width(60)
                .Label($"[Blue bold underline]{testSession.ProjectName} - {testSession.ProjectVersion}[/]")
                .CenterLabel()
                .AddItem("Passed", testSession.NumberOfTestsPassed, Color.Green)
                .AddItem("Failed", testSession.NumberOfTestsFailed, Color.Red)
                .AddItem("Total", testSession.TotalNumberOfTests, Color.Purple));
            console.WriteLine();
            console.MarkupLine($"[cyan3]Total Session Time : {TimeSpan.FromMinutes(testSession.SessionTime)}[/]");
        }
    }
}
