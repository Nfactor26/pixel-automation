using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
                OnPropertyChanged(nameof(CanSetUpEnvironment));
                OnPropertyChanged(nameof(CanTearDownEnvironment));
                OnPropertyChanged(nameof(CanRunTests));
            }
        }

        public bool CanRunTests
        {
            get => IsSetupComplete;
        }

        public bool CanSetUpEnvironment
        {
            get => !isSetupComplete;
        }

        public async Task SetUpEnvironment()
        {
            try
            {
                logger.Information("Performing environment setup");

                var oneTimeSetUp = this.entityManager.RootEntity.GetFirstComponentOfType<OneTimeSetUpEntity>(Core.Enums.SearchScope.Descendants);
                await ProcessEntity(oneTimeSetUp);
                IsSetupComplete = true;

                Log.Information("Environment setup completed");

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }
     
        public bool CanTearDownEnvironment
        {
            get => isSetupComplete;
        }

        public async Task TearDownEnvironment()
        {
            try
            {

                logger.Information("Performing environment teardown");

                var oneTimeTearDown = this.entityManager.RootEntity.GetFirstComponentOfType<OneTimeTearDownEntity>(Core.Enums.SearchScope.Descendants);
                await ProcessEntity(oneTimeTearDown);

                logger.Information("Environment teardown completed");

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

        public async Task OneTimeSetUp(TestFixture fixture)
        {
            try
            {
                logger.Information("Performing one time setup for fixture : {0}", fixture);

                IScriptEngine scriptEngine = fixture.TestFixtureEntity.EntityManager.GetScriptEngine();
                scriptEngine.ClearState();
                await scriptEngine.ExecuteFileAsync(fixture.ScriptFile);

                await ProcessEntity(fixture.TestFixtureEntity.GetFirstComponentOfType<OneTimeSetUpEntity>(Core.Enums.SearchScope.Children));
              
                Log.Information("One time setup completed for fixture : {0}", fixture);

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }


        public async Task OneTimeTearDown(TestFixture fixture)
        {
            try
            {
                logger.Information("Performing one time teardown for fixture : {0}", fixture);

                await ProcessEntity(fixture.TestFixtureEntity.GetFirstComponentOfType<OneTimeTearDownEntity>(Core.Enums.SearchScope.Children));
                IScriptEngine scriptEngine = fixture.TestFixtureEntity.EntityManager.GetScriptEngine();
                scriptEngine.ClearState();
             
                Log.Information("One time teardown completed for fixture : {0}", fixture);

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }


        [NonSerialized]
        Stopwatch stopWatch = new Stopwatch();

        public async IAsyncEnumerable<TestResult> RunTestAsync(TestFixture fixture, TestCase testCase)
        {
            Guard.Argument(testCase).NotNull();

            var testCaseEntity = testCase.TestCaseEntity;
            var dataSource = testDataLoader.LoadData(testCase.TestDataId);
            if (dataSource is IEnumerable dataSourceCollection)
            {
                foreach (var item in dataSourceCollection)
                {
                    testCaseEntity.EntityManager.Arguments = item;
                    var fixtureEntity = testCase.TestCaseEntity.Parent as TestFixtureEntity;
                    var fixtureScriptEngine = fixtureEntity.EntityManager.GetScriptEngine();
                    var testScriptEngine = testCaseEntity.EntityManager.GetScriptEngine();


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
                                testScriptEngine.ClearState();                              
                                await testScriptEngine.ExecuteFileAsync(fixture.ScriptFile);

                                //copy the variable values from fixture script engine to test script engine
                                foreach(var variable in fixtureScriptEngine.GetScriptVariables())
                                {
                                    testScriptEngine.SetVariableValue(variable.PropertyName, fixtureScriptEngine.GetVariableValue<object>(variable.PropertyName));
                                }

                                //foreach (var sharedVariable in fixtureEntity.SharedData)
                                //{
                                //    testScriptEngine.SetVariableValue(sharedVariable.Key, sharedVariable.Value);
                                //}
                                await testScriptEngine.ExecuteFileAsync(testCase.ScriptFile);

                                var setupEntity = fixtureEntity.GetFirstComponentOfType<SetUpEntity>(Core.Enums.SearchScope.Children);
                                await ProcessEntity(setupEntity);
                            }
                            catch
                            {
                                logger.Error("There was an error while executing setup stage.");
                                throw;
                            }

                            try
                            {                                
                               await ProcessEntity(testCaseEntity);
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
                                var teartDownEntity = fixtureEntity.GetFirstComponentOfType<TearDownEntity>(Core.Enums.SearchScope.Children);
                                await ProcessEntity(teartDownEntity);

                                //copy the fixture variable values back to fixture script engine so that following tests see modified values of these variable
                                foreach (var variable in fixtureScriptEngine.GetScriptVariables())
                                {
                                    fixtureScriptEngine.SetVariableValue(variable.PropertyName, testScriptEngine.GetVariableValue<object>(variable.PropertyName));
                                }

                            }
                            catch
                            {
                                logger.Error("There was an error while executing teardown stage.");
                                throw;
                            }
                        }

                        logger.Information("Finished execution of test case  : {testCase}.", testCase);

                        result = new TestResult() { Result = TestState.Success, TestData = item.ToString() };

                    }
                    catch (Exception ex)
                    {
                        result = new TestResult() { Result = TestState.Failed, Error = ex, TestData = item.ToString() };
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
        
        public async Task<bool> TryOpenTestFixture(TestFixture fixture)
        {
            try
            {
                Guard.Argument(fixture).NotNull();
                
                var allFixtures = this.entityManager.RootEntity.GetComponentsOfType<TestFixtureEntity>(Core.Enums.SearchScope.Children);
                if (!allFixtures.Any(f => f.Tag.Equals(fixture.Id)))
                {
                    TestFixtureEntity fixtureEntity = fixture.TestFixtureEntity as TestFixtureEntity;
                    EntityManager fixtureEntityManager = new EntityManager(this.entityManager) { Arguments = new Empty() };
                    fixtureEntity.EntityManager = fixtureEntityManager;
                    this.entityManager.RootEntity.AddComponent(fixture.TestFixtureEntity);
                 
                    logger.Information("Added test fixture : {0} to process.", fixture);

                    IScriptEngine scriptEngine = fixtureEntityManager.GetScriptEngine();
                    await scriptEngine.ExecuteFileAsync(fixture.ScriptFile);

                    //foreach (var scriptVariable in scriptEngine.GetScriptVariables())
                    //{
                    //    if (!fixtureEntity.SharedData.ContainsKey(scriptVariable.PropertyName))
                    //    {
                    //        fixtureEntity.SharedData.Add(scriptVariable.PropertyName, scriptEngine.GetVariableValue<object>(scriptVariable.PropertyName));
                    //    }
                    //}

                    logger.Information("Executed script file : {0} for fixture.", fixture.ScriptFile);
                    return true;

                }
                logger.Information("Fixture : {0} is already open.", fixture);
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to open test fixture {0}", fixture);
                return false;
            }
        }

        public async Task<bool> TryCloseTestFixture(TestFixture fixture)
        {
            try
            {

                Guard.Argument(fixture).NotNull();
                Guard.Argument(fixture).Require(fixture.TestFixtureEntity != null, (t) =>
                {
                    return $"{nameof(TestFixtureEntity)} is null for fixture : {fixture}";
                });

                var allFixtureEntities = this.entityManager.RootEntity.GetComponentsOfType<TestFixtureEntity>(Core.Enums.SearchScope.Children);
                if (allFixtureEntities.Any(f => f.Tag.Equals(fixture.Id)))
                {
                    var fixtureEntity = allFixtureEntities.First(a => a.Tag.Equals(fixture.Id));
                    foreach (var testCase in fixture.Tests)
                    {
                        if (fixtureEntity.GetComponentsByTag(testCase.Id, Core.Enums.SearchScope.Children).Any())
                        {
                            throw new InvalidOperationException($"All tests belonging to a fixture : {fixture.DisplayName} must be closed before fixture can be closed");
                        }
                    }
                    //fixtureEntity.SharedData.Clear();
                    fixtureEntity.EntityManager.Dispose();
                    fixtureEntity.Parent.RemoveComponent(fixtureEntity);
                    await Task.CompletedTask;
                    logger.Information("Test fixture : {0} was closed.", fixture);
                    return true;
                }
                logger.Information("Fixture : {0} needs to be open before attempting to close it.", fixture);
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to close test fixture {0}", fixture);
                return false;
            }
        }

        public async Task<bool> TryOpenTestCase(TestFixture fixture, TestCase testCase)
        {
            try
            {
                Guard.Argument(testCase).NotNull();

                var testCaseEntity = testCase.TestCaseEntity;
                if (!string.IsNullOrEmpty(testCase.TestDataId))
                {
                    //Create a new EntityManager for TestCaseEntity that can use ArgumentProcessor and ScriptEngine with different globals object               
                    EntityManager testEntityManager = new EntityManager(this.entityManager);
                    testCaseEntity.EntityManager = testEntityManager;

                    var dataSource = testDataLoader.LoadData(testCase.TestDataId);
                    if (dataSource is IEnumerable dataSourceCollection)
                    {
                        testEntityManager.Arguments = dataSource.FirstOrDefault();
                    }
                   
                   
                    //if fixture entity is not already added to process tree, add it.
                    var allFixtures = this.entityManager.RootEntity.GetComponentsOfType<TestFixtureEntity>(Core.Enums.SearchScope.Children);
                    var requireFixture = allFixtures.FirstOrDefault(f => f.Tag.Equals(fixture.Id));

                    if(requireFixture == null)
                    {
                        throw new InvalidOperationException($"Failed to open test case : {testCase.DisplayName}. Parent fixture : {fixture.DisplayName} is not open.");
                    }

                    requireFixture.AddComponent(testCaseEntity);
                    await Task.CompletedTask;
                    //Execute fixture and test script file to set up initial state of script engine for test case
                    //IScriptEngine scriptEngine = testEntityManager.GetScriptEngine();
                    //await scriptEngine.ExecuteFileAsync(fixture.ScriptFile);
                    //await scriptEngine.ExecuteFileAsync(testCase.ScriptFile);

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

        public async Task<bool> TryCloseTestCase(TestFixture fixture, TestCase testCase)
        {
            try
            {
                Guard.Argument(testCase).NotNull();
                Guard.Argument(testCase).Require(testCase.TestCaseEntity != null, (t) =>
                {
                    return $"{nameof(TestCaseEntity)} is not set for test case : {testCase}";
                });

                fixture.TestFixtureEntity.RemoveComponent(testCase.TestCaseEntity);

                var entityManager = testCase.TestCaseEntity.EntityManager;
                entityManager?.Dispose();
                testCase.TestCaseEntity.EntityManager = null;

                logger.Information("Removed test case : {0} from Fixture.", testCase);
                
                return await Task.FromResult<bool>(true);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to close test case {0}", testCase);
                return false;
            }    
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
