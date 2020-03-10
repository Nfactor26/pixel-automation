using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using Serilog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using IComponent = Pixel.Automation.Core.Interfaces.IComponent;

namespace Pixel.Automation.Core.Processors
{

    [DataContract]
    [Serializable]
    [ToolBoxItem("TestCase Processor", "Entity Processor", iconSource: null, description: "Start Test execution", tags: new string[] { "Test" })]
    public class TestCaseProcessorEntity : BaseProcessorEntity , ITestRunner
    {      
        [DataMember(IsRequired = true)]
        [Description("Pick test fixtures to be processed in a random fashion??")]
        public bool RandomizeOrder { get; set; } = false;

        public TestCaseProcessorEntity()
        {
            this.Name = "TestCase Processor";           
        }

        public override Task BeginProcess()
        {
            //Task testExecutor = new Task(async () =>
            //{
            //    try
            //    {

            //        IsProcessing = true;
            //        OnPropertyChanged("CanResetHierarchy");

            //        Log.Information("Performing One time setup for processor : {Id},{Name}", this.Id, this.Name);
            //        var oneTimeSetUp = this.GetFirstComponentOfType<OneTimeSetUpEntity>();
            //        await ProcessEntity(oneTimeSetUp);
            //        Log.Information("One time setup completed for processor : {Id},{Name}", this.Id, this.Name);

            //        var testCases = this.GetComponentsOfType<TestCaseEntity>(SearchScope.Children);
            //        testCases = testCases.Where(t => t.IsEnabled).OrderBy(t => t.ProcessOrder);

            //        Log.Information("{Count} enabled test cases located for procdssor : {Id},{Name}", testCases.Count(), this.Id, this.Name);


            //        //process all the test cases
            //        foreach (TestCaseEntity testCase in testCases)
            //        {
            //            Log.Information("Running test case  : {Id},{Name} now.", testCase.Id, testCase.Name);

            //            var setupEntity = testCase.GetFirstComponentOfType<SetUpEntity>();
            //            await ProcessEntity(setupEntity);

            //            var testSequence = testCase.GetFirstComponentOfType<TestSequenceEntity>();
            //            await ProcessEntity(testSequence);

            //            var teartDownEntity = testCase.GetFirstComponentOfType<TearDownEntity>();
            //            await ProcessEntity(teartDownEntity);

            //            Log.Information("Completed test case  : {Id},{Name}.", testCase.Id, testCase.Name);

            //        }

            //        Log.Information("Performing one time teardown for processor : {Id},{Name}", this.Id, this.Name);

            //        var oneTimeTearDown = this.GetFirstComponentOfType<OneTimeTearDownEntity>();
            //        await ProcessEntity(oneTimeTearDown);

            //        Log.Information("One time teardown completed for processor : {Id},{Name}", this.Id, this.Name);
            //    }
            //    catch (Exception ex)
            //    {
            //        Log.Error(ex.Message, ex);
            //    }
            //    finally
            //    {
            //        IsProcessing = false;
            //        OnPropertyChanged("CanResetHierarchy");
            //    }

            //});
            //testExecutor.Start();
            //return testExecutor;

            return Task.CompletedTask;


        }

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
                        Log.Debug("Component : [{$Id},{$Name}] will be processed next.", component.Id, component.Name);


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
                                        Log.Warning(ex, ex.Message);
                                    }
                                }

                                Thread.Sleep(TimeSpan.FromSeconds(this.ProcessingDelay));
                                actor.IsExecuting = false;
                                break;

                            case IEntityProcessor processor:
                                await processor.BeginProcess();
                                break;

                            case Entity entity:
                                if (this.entitiesBeingProcessed.Count() > 0 && this.entitiesBeingProcessed.Peek().Equals(component as Entity))
                                {
                                    var processedEntity = this.entitiesBeingProcessed.Pop();
                                    processedEntity.OnCompletion();
                                }
                                else
                                {
                                    component.BeforeProcess();
                                    this.entitiesBeingProcessed.Push(component as Entity);
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

        public void SetUp()
        {
            Task setupTask = new Task(async () =>
            {
                try
                {

                    IsProcessing = true;
                    OnPropertyChanged("CanResetHierarchy");

                    Log.Information("Performing One time setup for processor : {Id},{Name}", this.Id, this.Name);

                    var oneTimeSetUp = this.GetFirstComponentOfType<OneTimeSetUpEntity>();
                    await ProcessEntity(oneTimeSetUp);
                    IsSetupComplete = true;

                    Log.Information("One time setup completed for processor : {Id},{Name}", this.Id, this.Name);               
                 
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex);
                }
                finally
                {
                    IsProcessing = false;
                    OnPropertyChanged("CanResetHierarchy");
                }

            });
            setupTask.Start();          
        }
        
        public bool CanTearDown
        {
            get => isSetupComplete;
        }

        public void TearDown()
        {
            Task tearDownTask = new Task(async () =>
            {
                try
                {

                    IsProcessing = true;
                    OnPropertyChanged("CanResetHierarchy");

                    Log.Information("Performing One time teardown for processor : {Id},{Name}", this.Id, this.Name);

                    var oneTimeTearDown = this.GetFirstComponentOfType<OneTimeTearDownEntity>();
                    await ProcessEntity(oneTimeTearDown);                 

                    Log.Information("One time teardown completed for processor : {Id},{Name}", this.Id, this.Name);

                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex);
                }
                finally
                {
                    IsSetupComplete = false; //even if something breaks during teardown , consider setup complete as false
                    IsProcessing = false;
                    OnPropertyChanged("CanResetHierarchy");
                }

            });
            tearDownTask.Start();
        }

        [NonSerialized]
        Stopwatch stopWatch = new Stopwatch();

        public async Task<TestResult> RunTestAsync(Entity testCase)
        {
            //Guard.Argument(testCase).NotNull().Compatible<TestCaseEntity>();
           
            stopWatch.Reset();
            stopWatch.Start();
            TestResult result = default;
            try
            {           

                Log.Information("Starting execution of test case  : {Id},{Name} now.", testCase.Id, testCase.Name);
                               
                try
                {
                    try
                    {
                        var setupEntity = testCase.GetFirstComponentOfType<SetUpEntity>();
                        await ProcessEntity(setupEntity);
                    }
                    catch
                    {
                        Log.Error("There was an error while processing setup stage.");
                        throw;
                    }

                    try
                    {
                        var testSequence = testCase.GetFirstComponentOfType<TestSequenceEntity>();
                        await ProcessEntity(testSequence);
                    }
                    catch
                    {
                        Log.Error("There was an error while processing test case sequence.");
                        throw;
                    }
                }
                finally
                {
                    try
                    {
                        var teartDownEntity = testCase.GetFirstComponentOfType<TearDownEntity>();
                        await ProcessEntity(teartDownEntity);
                    }
                    catch
                    {
                        Log.Error("There was an error while processing teardown stage.");
                        throw;
                    }
                }
               
                Log.Information("Finished execution of test case  : {Id},{Name}.", testCase.Id, testCase.Name);
               
                result = new TestResult() { Result = TestState.Success};

            }
            catch (Exception ex)
            {
                result = new  TestResult() { Result = TestState.Failed, Error = ex };
            }
            finally
            {
                stopWatch.Stop();
                result.ExecutionTime = TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds);               
            }
            return result;
        }

        public void OpenTestEntity(TestCase testCase)
        {
            var testCaseEntity = testCase.TestCaseEntity;
            if (!string.IsNullOrEmpty(testCase.TestDataId))
            {               
                //we create a new EntityManager for TestCaseEntity that can use ArgumentProcessor with different globals object               
                EntityManager entityManager = new EntityManager(this.EntityManager, null);
                testCaseEntity.EntityManager = entityManager;

                this.AddComponent(testCaseEntity);
                return;               
            }         

            this.AddComponent(testCaseEntity);
        }

        public void RemoveTestEntity(Entity testCaseEntity)
        {
            var entityManager = testCaseEntity.EntityManager;
            entityManager?.Dispose();
            this.RemoveComponent(testCaseEntity);
        }

        #endregion ITestRunner
    }
}

