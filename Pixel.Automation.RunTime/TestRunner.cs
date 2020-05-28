using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.RunTime
{
    /// <inheritdoc/>
    public class TestRunner : NotifyPropertyChanged, ITestRunner
    {
        protected readonly ILogger logger = Log.ForContext<TestRunner>();
        protected readonly IEntityManager entityManager;
        protected readonly IDataSourceReader testDataLoader;
     

        public TestRunner(IEntityManager entityManager, IDataSourceReader testDataLoader)
        {
            this.entityManager = entityManager;
            this.testDataLoader = testDataLoader;
        }   

        #region ITestRunner

        bool isSetupComplete;
        bool IsSetupComplete
        {
            get => isSetupComplete;
            set
            {
                isSetupComplete = value;
                OnPropertyChanged(nameof(CanSetUp));
                OnPropertyChanged(nameof(CanTearDown));
                OnPropertyChanged(nameof(CanRunTests));
            }
        }

        public bool CanRunTests
        {
            get => IsSetupComplete;
        }

        public bool CanSetUp
        {
            get => !isSetupComplete;
        }

        public async Task SetUp()
        {
            try
            {
                logger.Information("Performing one time setup");

                var oneTimeSetUp = this.entityManager.RootEntity.GetFirstComponentOfType<OneTimeSetUpEntity>(Core.Enums.SearchScope.Descendants);
                await ProcessEntity(oneTimeSetUp);
                IsSetupComplete = true;

                Log.Information("One time setup completed");

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }
     
        public bool CanTearDown
        {
            get => isSetupComplete;
        }

        public async Task TearDown()
        {
            try
            {

                logger.Information("Performing One time teardown");

                var oneTimeTearDown = this.entityManager.RootEntity.GetFirstComponentOfType<OneTimeTearDownEntity>(Core.Enums.SearchScope.Descendants);
                await ProcessEntity(oneTimeTearDown);

                logger.Information("One time teardown completed");

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
            finally
            {
                IsSetupComplete = false; //even if something breaks during teardown , consider teardown complete as false               
            }

        }

        [NonSerialized]
        Stopwatch stopWatch = new Stopwatch();

        public async IAsyncEnumerable<TestResult> RunTestAsync(TestCase testCase)
        {
            Guard.Argument(testCase).NotNull();

            var testCaseEntity = testCase.TestCaseEntity;
            var dataSource = testDataLoader.LoadData(testCase.TestDataId);
            if (dataSource is IEnumerable dataSourceCollection)
            {
                foreach (var item in dataSourceCollection)
                {
                    testCaseEntity.EntityManager.Arguments = item;
                    
                    IScriptEngine scriptEngine = testCaseEntity.EntityManager.GetServiceOfType<IScriptEngine>();
                    scriptEngine.ClearState();
                    var state = await scriptEngine.ExecuteFileAsync(testCase.ScriptFile);
                                       
                    stopWatch.Reset();
                    stopWatch.Start();
                    TestResult result = default;
                    try
                    {

                        logger.Information("Starting execution of test case  : {testCase} now.", testCase);

                        try
                        {
                            try
                            {
                                var setupEntity = testCaseEntity.GetFirstComponentOfType<SetUpEntity>();
                                await ProcessEntity(setupEntity);
                            }
                            catch
                            {
                                logger.Error("There was an error while executing setup stage.");
                                throw;
                            }

                            try
                            {
                                var testSequence = testCaseEntity.GetFirstComponentOfType<TestSequenceEntity>();
                                await ProcessEntity(testSequence);
                            }
                            catch
                            {
                                logger.Error("There was an error while executing test case.");
                                throw;
                            }
                        }
                        finally
                        {
                            try
                            {
                                var teartDownEntity = testCaseEntity.GetFirstComponentOfType<TearDownEntity>();
                                await ProcessEntity(teartDownEntity);
                            }
                            catch
                            {
                                logger.Error("There was an error while executing teardown stage.");
                                throw;
                            }
                        }

                        logger.Information("Finished execution of test case  : {testCase}.", testCase);

                        result = new TestResult() { Result = TestState.Success };

                    }
                    catch (Exception ex)
                    {
                        result = new TestResult() { Result = TestState.Failed, Error = ex };
                    }
                    finally
                    {
                        stopWatch.Stop();
                        result.ExecutionTime = TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds);
                    }

                   yield return result;
                }
            }
          
        }

        public async Task<bool> TryOpenTestCase(TestCase testCase)
        {
            try
            {
                Guard.Argument(testCase).NotNull();

                var testCaseEntity = testCase.TestCaseEntity;
                if (!string.IsNullOrEmpty(testCase.TestDataId))
                {
                    //we create a new EntityManager for TestCaseEntity that can use ArgumentProcessor with different globals object               
                    EntityManager testEntityManager = new EntityManager(this.entityManager, null);

                    ITestCaseFileSystem testCaseFileSystem = testEntityManager.GetServiceOfType<ITestCaseFileSystem>();
                    testCaseFileSystem.Initialize(this.entityManager.GetCurrentFileSystem().WorkingDirectory, testCase.Id);
                    testEntityManager.RegisterDefault<IFileSystem>(testCaseFileSystem);
                    testEntityManager.SetCurrentFileSystem(testCaseFileSystem);                 

                    var dataSource = testDataLoader.LoadData(testCase.TestDataId);
                    if (dataSource is IEnumerable dataSourceCollection)
                    {
                        testEntityManager.Arguments = dataSource.FirstOrDefault();
                    }

                    string scriptFileContent = File.ReadAllText(Path.Combine(entityManager.GetCurrentFileSystem().WorkingDirectory, testCase.ScriptFile));
                  
                    //Execute script file to set up initial state of script engine
                    IScriptEngine scriptEngine = testEntityManager.GetServiceOfType<IScriptEngine>();                 
                    await scriptEngine.ExecuteScriptAsync(scriptFileContent);

                    testCaseEntity.EntityManager = testEntityManager;
                    var testFixtureEntity = this.entityManager.RootEntity.GetFirstComponentOfType<TestFixtureEntity>();
                    testFixtureEntity.AddComponent(testCaseEntity);

                    logger.Information("Added test case : {testCase} to Fixture.", testCase);

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to open test case {0}", testCase);
                return false;
            }
        }

        public async Task CloseTestCase(TestCase testCase)
        {
            Guard.Argument(testCase).NotNull();
            Guard.Argument(testCase).Require(testCase.TestCaseEntity != null, (t) => {
                return $"TestCaseEntity is not set for test case : {testCase}"; });

            var testFixtureEntity = this.entityManager.RootEntity.GetFirstComponentOfType<TestFixtureEntity>();
            testFixtureEntity.RemoveComponent(testCase.TestCaseEntity);

            var entityManager = testCase.TestCaseEntity.EntityManager;
            entityManager?.Dispose();
            testCase.TestCaseEntity.EntityManager = null;

            logger.Information("Removed test case : {testCase} from Fixture.", testCase);

            await Task.CompletedTask;            
        }

        #endregion ITestRunner

        protected Stack<Entity> entitiesBeingProcessed = new Stack<Entity>();
        protected virtual async Task<bool> ProcessEntity(Entity targetEntity)
        {
            IComponent actorBeingProcessed = null;
            try
            {
                targetEntity.BeforeProcess();
                foreach (IComponent component in targetEntity.GetNextComponentToProcess())
                {
                    if (component.IsEnabled)
                    {
                        logger.Information("Component : [{$Id},{$Name}] will be processed next.", component.Id, component.Name);


                        switch (component)
                        {
                            case ActorComponent actor:
                                try
                                {
                                    actorBeingProcessed = actor;
                                    actor.BeforeProcess();
                                    actor.IsExecuting = true;                                  
                                    actor.Act();
                                    actor.OnCompletion();
                                }
                                catch (Exception ex)
                                {
                                    if (!actor.ContinueOnError)
                                    {
                                        throw;
                                    }
                                    else
                                    {
                                        actor.IsFaulted = true;
                                        Log.Warning(ex, ex.Message);
                                    }
                                }

                                //Thread.Sleep(TimeSpan.FromSeconds(this.ProcessingDelay));
                                actor.IsExecuting = false;
                                break;

                            case AsyncActorComponent actor:
                                try
                                {
                                    actorBeingProcessed = actor;
                                    actor.BeforeProcess();
                                    actor.IsExecuting = true;
                                    await actor.ActAsync();
                                    actor.OnCompletion();
                                }
                                catch (Exception ex)
                                {
                                    if (!actor.ContinueOnError)
                                    {
                                        throw;
                                    }
                                    else
                                    {
                                        actor.IsFaulted = true;
                                        Log.Warning(ex, ex.Message);
                                    }
                                }

                                //Thread.Sleep(TimeSpan.FromSeconds(this.ProcessingDelay));
                                actor.IsExecuting = false;
                                break;

                            case IEntityProcessor processor:
                                await processor.BeginProcess();
                                break;

                            case Entity entity:
                                //Entity -> GetNextComponentToProcess yields child entity two times . Before processing its children and after it's children are processed
                                if (this.entitiesBeingProcessed.Count() > 0 && this.entitiesBeingProcessed.Peek().Equals(entity))
                                {                                   
                                    var processedEntity = this.entitiesBeingProcessed.Pop();
                                    processedEntity.OnCompletion();
                                }
                                else
                                {
                                    entity.BeforeProcess();
                                    this.entitiesBeingProcessed.Push(entity);
                                }
                                break;
                        }
                    }
                }
                targetEntity.OnCompletion();

                Log.Information("All components have been processed for Entity : {Id},{Name}", targetEntity.Id, targetEntity.Name);

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                if (actorBeingProcessed is ActorComponent)
                {
                    ActorComponent currentActor = actorBeingProcessed as ActorComponent;
                    currentActor.IsExecuting = false;
                    currentActor.IsFaulted = true;
                }

                while (this.entitiesBeingProcessed.Count() > 0)
                {
                    try
                    {
                        var entity = this.entitiesBeingProcessed.Pop();
                        entity.OnFault(actorBeingProcessed);
                    }
                    catch (Exception faultHandlingExcpetion)
                    {
                        Log.Error(faultHandlingExcpetion.Message, faultHandlingExcpetion);
                    }
                }

                targetEntity.OnFault(actorBeingProcessed);
                throw;
            }

            return true;
        }

    }
}
