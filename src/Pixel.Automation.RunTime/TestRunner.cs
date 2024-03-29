﻿using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using Serilog;
using Serilog.Context;
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
       
        protected int postDelayAmount = 0;
        
        public TestRunner(IEntityManager entityManager, IDataSourceReader testDataLoader)
        {           
            this.entityManager = Guard.Argument(entityManager).NotNull().Value; 
            this.testDataLoader = Guard.Argument(testDataLoader).NotNull().Value;           
        }   

        #region ITestRunner

        bool isSetupComplete;
        bool IsSetupComplete
        {
            get => isSetupComplete;
            set
            {
                isSetupComplete = value;               
            }
        }

        public bool CanRunTests
        {
            get => IsSetupComplete;
        }

        public async Task SetUpEnvironment()
        {
            try
            {
                logger.Information("Performing environment setup");               
       
                var oneTimeSetUpEntity = this.entityManager.RootEntity.GetFirstComponentOfType<OneTimeSetUpEntity>(Core.Enums.SearchScope.Descendants);
                oneTimeSetUpEntity.ResetHierarchy();
                await ProcessEntity(oneTimeSetUpEntity);
                IsSetupComplete = true;

                //set the data source suffix if any 
                var scriptEngine = this.entityManager.GetScriptEngine();
                if(scriptEngine.HasScriptVariable("dataSourceSuffix"))
                {
                    string dataSourceSuffix = scriptEngine.GetVariableValue<string>("dataSourceSuffix");
                    this.testDataLoader.SetDataSourceSuffix(dataSourceSuffix);
                }

                logger.Information("Environment setup completed");

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }
        }    

        public async Task TearDownEnvironment()
        {
            try
            {

                logger.Information("Performing environment teardown");

                var oneTimeTearDownEntity = this.entityManager.RootEntity.GetFirstComponentOfType<OneTimeTearDownEntity>(Core.Enums.SearchScope.Descendants);
                oneTimeTearDownEntity.ResetHierarchy();
                await ProcessEntity(oneTimeTearDownEntity);

                //clear the data source suffix
                this.testDataLoader.SetDataSourceSuffix(string.Empty);

                logger.Information("Environment teardown completed");

            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
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

                var oneTimeSetupEntity = fixture.TestFixtureEntity.GetFirstComponentOfType<OneTimeSetUpEntity>(Core.Enums.SearchScope.Children, false);
                if (oneTimeSetupEntity != null)
                {
                    oneTimeSetupEntity.ResetHierarchy();
                    await ProcessEntity(oneTimeSetupEntity);                   
                }

                logger.Information("One time setup completed for fixture : {0}", fixture);

            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }


        public async Task OneTimeTearDown(TestFixture fixture)
        {
            try
            {
                logger.Information("Performing one time teardown for fixture : {0}", fixture);

                var oneTimeTearDownEntity = fixture.TestFixtureEntity.GetFirstComponentOfType<OneTimeTearDownEntity>(Core.Enums.SearchScope.Children, false);
                if(oneTimeTearDownEntity != null)
                {
                    oneTimeTearDownEntity.ResetHierarchy();
                    await ProcessEntity(oneTimeTearDownEntity);
                }              
             
                IScriptEngine scriptEngine = fixture.TestFixtureEntity.EntityManager.GetScriptEngine();
                scriptEngine.ClearState();

                //Execute the script file again. This is required at design time so that any arguments bound to variables in this script file
                //still have access to them.
                await scriptEngine.ExecuteFileAsync(fixture.ScriptFile);

                logger.Information("One time teardown completed for fixture : {0}", fixture);

            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }


        [NonSerialized]
        private Stopwatch stopWatch = new Stopwatch();
   
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

                    ConfigureDelays(testCase);

                    stopWatch.Reset();
                    stopWatch.Start();
                    TestResult result = new TestResult() { TestData = item.ToString() };
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

                                await testScriptEngine.ExecuteFileAsync(testCase.ScriptFile);

                                var setupEntity = fixtureEntity.GetFirstComponentOfType<SetUpEntity>(Core.Enums.SearchScope.Children, false);
                                if(setupEntity != null)
                                {
                                    setupEntity.ResetHierarchy();
                                    await ProcessEntity(setupEntity);
                                }                             
                            }
                            catch
                            {
                                logger.Error("There was an error while executing setup stage.");
                                throw;
                            }

                            try
                            {
                               testCaseEntity.ResetHierarchy();
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
                                var teartDownEntity = fixtureEntity.GetFirstComponentOfType<TearDownEntity>(Core.Enums.SearchScope.Children, false);
                                if(teartDownEntity != null)
                                {
                                    teartDownEntity.ResetHierarchy();
                                    await ProcessEntity(teartDownEntity);
                                }                              

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

                        result.Result = TestStatus.Success;

                    }
                    catch (Exception ex)
                    {
                        result.Result = TestStatus.Failed;
                        result.Error = ex;
                    }
                    finally
                    {
                        stopWatch.Stop();
                        result.ExecutionTime = TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds);
                        ResetDelays();
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
                if (!allFixtures.Any(f => f.Tag.Equals(fixture.FixtureId)))
                {
                    TestFixtureEntity fixtureEntity = fixture.TestFixtureEntity as TestFixtureEntity;
                    EntityManager fixtureEntityManager = new EntityManager(this.entityManager);
                    fixtureEntityManager.SetIdentifier($"Fixture - {fixture.DisplayName}");
                    fixtureEntityManager.Arguments = new EmptyModel();
                    fixtureEntity.EntityManager = fixtureEntityManager;
                    this.entityManager.RootEntity.AddComponent(fixture.TestFixtureEntity);                 
                    
                    logger.Information("Added test fixture : {0} to process.", fixture);

                    IScriptEngine scriptEngine = fixtureEntityManager.GetScriptEngine();
                    await scriptEngine.ExecuteFileAsync(fixture.ScriptFile);

                    logger.Information("Executed script file : {0} for fixture.", fixture.ScriptFile);
                    return true;

                }
                logger.Information("Fixture : {0} is already open.", fixture);
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "{0}. Failed to open test fixture {1}", ex.Message, fixture);
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
                if (allFixtureEntities.Any(f => f.Tag.Equals(fixture.FixtureId)))
                {
                    var fixtureEntity = allFixtureEntities.First(a => a.Tag.Equals(fixture.FixtureId));
                    foreach (var testCase in fixture.Tests)
                    {
                        if (fixtureEntity.GetComponentsByTag(testCase.TestCaseId, Core.Enums.SearchScope.Children).Any())
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
                logger.Error(ex, "{0}. Failed to close test fixture {1}", ex.Message, fixture);
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
                    testEntityManager.SetIdentifier($"Test - {testCase.DisplayName}");
                    testCaseEntity.EntityManager = testEntityManager;                  

                    var dataSource = testDataLoader.LoadData(testCase.TestDataId);
                    if(!dataSource.Any())
                    {                       
                        throw new InvalidOperationException("Data source doesn't have any data records");
                    }
                    testEntityManager.Arguments = dataSource.FirstOrDefault();
                                      
                    //if fixture entity is not already added to process tree, add it.
                    var allFixtures = this.entityManager.RootEntity.GetComponentsOfType<TestFixtureEntity>(Core.Enums.SearchScope.Children);
                    var parentFixture = allFixtures.FirstOrDefault(f => f.Tag.Equals(fixture.FixtureId));

                    if(parentFixture == null)
                    {
                        throw new InvalidOperationException($"Failed to open test case : {testCase.DisplayName}. Parent fixture : {fixture.DisplayName} is not open.");
                    }

                    parentFixture.AddComponent(testCaseEntity);

                    //This is required so that any argument that was configured in previous session should have access to variables.
                    var scriptEngine = testEntityManager.GetScriptEngine();
                    await scriptEngine.ExecuteFileAsync(fixture.ScriptFile);
                    await scriptEngine.ExecuteFileAsync(testCase.ScriptFile);

                    logger.Information("Added test case : {testCase} to Fixture.", testCase);

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "{0}. Failed to open test case {1}", ex.Message, testCase);
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

                testCase.TestCaseEntity.EntityManager.Dispose();               
                fixture.TestFixtureEntity.RemoveComponent(testCase.TestCaseEntity);              

                logger.Information("Removed test case : {0} from Fixture.", testCase);
                
                return await Task.FromResult<bool>(true);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "{0}. Failed to close test case {0}", ex.Message, testCase);
                return false;
            }    
        }

        #endregion ITestRunner

        protected void ConfigureDelays(TestCase testCase)
        {           
            this.postDelayAmount = testCase.DelayFactor * testCase.PostDelay;
        }

        protected void ResetDelays()
        {
            this.postDelayAmount = 0;
        }

        protected async Task AddDelay(int delayAmount)
        {
            if (delayAmount > 0)
            {
                logger.Debug($"Wait for {delayAmount / 1000.0} seconds");
                await Task.Delay(delayAmount);
            }
        }

        protected virtual async Task<bool> ProcessEntity(Entity targetEntity)
        {
            var entitiesBeingProcessed = new Stack<Entity>();
            IComponent actorBeingProcessed = null;
            try
            {
                await targetEntity.BeforeProcessAsync();
                foreach (IComponent component in targetEntity.GetNextComponentToProcess())
                {
                    if (component.IsEnabled)
                    {
                        logger.Information("Component : [{$Id},{$Name}] will be processed next.", component.Id, component.Name);

                        switch (component)
                        {                          
                            case ActorComponent actor:
                                using (LogContext.PushProperty(TraceManager.CaptureTrace, true))
                                {
                                    try
                                    {
                                        actorBeingProcessed = actor;
                                        actor.IsExecuting = true;
                                        await actor.ActAsync();
                                        await AddDelay(this.postDelayAmount);
                                        await actor.OnCompletionAsync();
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
                                            logger.Warning(ex, ex.Message);
                                        }
                                    }

                                    actor.IsExecuting = false;                                   
                                }
                                break;
                            case IEntityProcessor processor:
                                processor.ConfigureDelay(this.postDelayAmount);
                                await processor.BeginProcessAsync();
                                processor.ResetDelay();
                                break;

                            case Entity entity:
                                //Entity -> GetNextComponentToProcess yields child entity two times . Before processing its children and after it's children are processed
                                if (entitiesBeingProcessed.Count() > 0 && entitiesBeingProcessed.Peek().Equals(entity))
                                {
                                    var processedEntity = entitiesBeingProcessed.Pop();
                                    await processedEntity.OnCompletionAsync();
                                }
                                else
                                {
                                    entitiesBeingProcessed.Push(entity);
                                    await entity.BeforeProcessAsync();
                                }
                                break;
                        }
                    }
                }

                await targetEntity.OnCompletionAsync();

                logger.Information("All components have been processed for Entity : {Id}, {Name}", targetEntity.Id, targetEntity.Name);

            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                if (actorBeingProcessed is ActorComponent)
                {
                    ActorComponent currentActor = actorBeingProcessed as ActorComponent;
                    currentActor.IsExecuting = false;
                    currentActor.IsFaulted = true;
                }

                while (entitiesBeingProcessed.Count() > 0)
                {
                    try
                    {
                        var entity = entitiesBeingProcessed.Pop();
                        await entity.OnFaultAsync(actorBeingProcessed);
                    }
                    catch (Exception faultHandlingExcpetion)
                    {
                        logger.Error(faultHandlingExcpetion.Message, faultHandlingExcpetion);
                    }
                }

                await targetEntity.OnFaultAsync(actorBeingProcessed);
                throw;
            }

            return true;
        }

    }
}
