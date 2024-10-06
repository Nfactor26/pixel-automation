using Caliburn.Micro;
using Notifications.Wpf.Core;
using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.Loops;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Components.Sequences;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.TestExplorer.ViewModels.Tests
{
    [TestFixture]
    public class TestExplorerViewModelFixture
    {
        [Test]
        public void ValidateThatTestExplorerViewModelCanBeCorrectlyInitialized()
        {
            //Arrange
            var eventAggregator = Substitute.For<IEventAggregator>();
            var windowManager = Substitute.For<IWindowManager>();
            var notificationManager = Substitute.For<INotificationManager>();
            var platformProvider = Substitute.For<IPlatformProvider>();
            var projectManager = Substitute.For<IAutomationProjectManager>();
            var dataManager = Substitute.For<IProjectAssetsDataManager>();
            var projectFileSystem = Substitute.For<IProjectFileSystem>();
            var componentViewBuilder = Substitute.For<IComponentViewBuilder>();
            var testRunner = Substitute.For<ITestRunner>();
            var applicationSettings = new ApplicationSettings();

            //Act 
            var testExplorerViewModel = new TestExplorerViewModel(eventAggregator, projectManager, projectFileSystem, testRunner, 
                dataManager, componentViewBuilder, windowManager, notificationManager, platformProvider, applicationSettings);

            //Assert
            Assert.That(testExplorerViewModel.TestFixtures.Count(), Is.EqualTo(0));
            Assert.That(string.IsNullOrEmpty(testExplorerViewModel.FilterText));
        }

    }

    /// <summary>
    /// TestFixture for TestExplorerViewModel with focus on managing TestFixture e.g. Add, Edit, Delete, etc.
    /// </summary>
    [TestFixture]
    public class TestExplorerViewModel_TestFixture_Fixture
    {     
        private readonly string fixtureScriptFile = "FixtureScript.csx";     

        private IEventAggregator eventAggregator;
        private IWindowManager windowManager;
        private INotificationManager notificationManger;
        private IPlatformProvider platformProvider;
        private IAutomationProjectManager projectManager;
        private IProjectFileSystem projectFileSystem;
        private IProjectAssetsDataManager dataManager;
        private ITestRunner testRunner;
        private IEntityManager fixtureEntityManager;
        private IScriptEditorFactory scriptEditorFactory;
        private IComponentViewBuilder componentViewBuilder;
        private ApplicationSettings applicationSettings;
        private Action<CallInfo> DoNothing = (a) => { };

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            eventAggregator = Substitute.For<IEventAggregator>();
            windowManager = Substitute.For<IWindowManager>();
            notificationManger = Substitute.For<INotificationManager>();
            platformProvider = Substitute.For<IPlatformProvider>();
            projectManager = Substitute.For<IAutomationProjectManager>();
            projectFileSystem = Substitute.For<IProjectFileSystem>();
            dataManager = Substitute.For<IProjectAssetsDataManager>();
            testRunner = Substitute.For<ITestRunner>();
            componentViewBuilder = Substitute.For<IComponentViewBuilder>();
            fixtureEntityManager = Substitute.For<IEntityManager>();
            scriptEditorFactory = Substitute.For<IScriptEditorFactory>();
            applicationSettings = new ApplicationSettings();

          
            projectFileSystem.GetRelativePath(Arg.Any<string>()).Returns(Path.Combine("FixureId", fixtureScriptFile));
            projectFileSystem.GetTestFixtureFiles(Arg.Any<TestFixture>()).Returns(new TestFixtureFiles(new TestFixture(), Environment.CurrentDirectory));
            projectFileSystem.When(x => x.CreateOrReplaceFile(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())).Do(DoNothing);
            projectFileSystem.When(x => x.SaveToFile<TestFixture>(Arg.Any<TestFixture>(), Arg.Any<string>(), Arg.Any<string>())).Do(DoNothing);
            projectFileSystem.When(x => x.SaveToFile<Entity>(Arg.Any<Entity>(), Arg.Any<string>(), Arg.Any<string>())).Do
             (
                 x =>
                 {
                     var entity = x.ArgAt<Entity>(0);
                     var testCaseEntities = entity.GetComponentsOfType<TestCaseEntity>(SearchScope.Descendants);
                     if (testCaseEntities.Any())
                     {
                         throw new InvalidOperationException("TestFixtureEntity should not have any TestCaseEntity as child entity while doing Save");
                     }
                 }
             );
            projectFileSystem.ReadAllText(Arg.Is<string>(fixtureScriptFile)).Returns(string.Empty);
            projectFileSystem.GetTestFixtureFiles(Arg.Any<TestFixture>()).Returns(x => new TestFixtureFiles(x[0] as TestFixture , Environment.CurrentDirectory));
                     
            scriptEditorFactory.When(x => x.AddProject(Arg.Any<string>(), Arg.Is<string[]>(Array.Empty<string>()), Arg.Is<Type>(typeof(EmptyModel)))).Do(DoNothing);
            scriptEditorFactory.When(x => x.AddDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())).Do(DoNothing);
            scriptEditorFactory.When(x => x.RemoveProject(Arg.Any<string>())).Do(DoNothing);
            fixtureEntityManager.GetServiceOfType<IScriptEditorFactory>().Returns(scriptEditorFactory);

        }

        TestFixtureViewModel CreateTestFixtureViewModel(bool isOpenForEdit)
        {
            var fixtureEntity = new TestFixtureEntity() { EntityManager = fixtureEntityManager };
            var seqeunceEntity = new SequenceEntity();
            fixtureEntity.AddComponent(seqeunceEntity);
            // WhileLoopEntity has Scriptable Attribute. We need this to assert a condition while closing test case
            var whileLoopEntity = new WhileLoopEntity() { ScriptFile = "Script.csx" };
            seqeunceEntity.AddComponent(whileLoopEntity);

            TestFixture testFixture = new TestFixture()
            {
                DisplayName = $"Fixture#1",
                Order = 1,
                ScriptFile = fixtureScriptFile
            };
            TestFixtureViewModel testFixtureVM = new TestFixtureViewModel(testFixture)
            {
                IsOpenForEdit = isOpenForEdit,
                TestFixtureEntity = fixtureEntity
            };
            if (isOpenForEdit)
            {
                fixtureEntity.Name = testFixtureVM.DisplayName;
                fixtureEntity.Tag = testFixtureVM.FixtureId;
            }
            return testFixtureVM;
        }

        [TearDown]
        public void TearDown()
        {
            eventAggregator.ClearReceivedCalls();
            windowManager.ClearReceivedCalls();
            platformProvider.ClearReceivedCalls();
            projectManager.ClearReceivedCalls();
            projectFileSystem.ClearReceivedCalls();
            dataManager.ClearReceivedCalls();
            testRunner.ClearReceivedCalls();
            fixtureEntityManager.ClearReceivedCalls();
            scriptEditorFactory.ClearReceivedCalls();
        }


        /// <summary>
        /// Validate that it is possible to add a new TestFixture using add TestFixtureAsync(TestFixture) method
        /// </summary>
        /// <param name="dialogResult">Boolean result of whether user clicked save or cancel on screen</param>
        /// <returns></returns>
        [TestCase(true)]
        [TestCase(false)]
        public async Task ValidateThatTestFixtureCanBeAdded(bool dialogResult)
        {
            //Arrange
            windowManager.ShowDialogAsync(Arg.Any<EditTestFixtureViewModel>()).Returns(dialogResult);

            //Act
            var testExplorerViewModel = new TestExplorerViewModel(eventAggregator, projectManager, projectFileSystem, testRunner, dataManager, componentViewBuilder,
                windowManager, notificationManger, platformProvider, applicationSettings);
            Assert.That(testExplorerViewModel.CanAddTestFixture);
            await testExplorerViewModel.AddTestFixtureAsync();

            //Assert
            int expected = dialogResult ? 1 : 0;
            Assert.That(testExplorerViewModel.CanAddTestFixture);
            Assert.That(testExplorerViewModel.TestFixtures.Count, Is.EqualTo(expected));
            await windowManager.Received(1).ShowDialogAsync(Arg.Any<EditTestFixtureViewModel>());
            await dataManager.Received(expected).AddTestFixtureAsync(Arg.Any<TestFixture>());
        }

        /// <summary>
        /// Validate that it is possible to edit an existing TestFixture
        /// </summary>
        /// <param name="dialogResult">Boolean result of whether user clicked save (to accept edits) or cancel (to discard edits) on screen </param>
        /// <returns></returns>
        [TestCase(true)]
        [TestCase(false)]
        public async Task ValidateThatTestFixtureCanBeEdited(bool dialogResult)
        {
            //Arrange
            windowManager.ShowDialogAsync(Arg.Any<EditTestFixtureViewModel>()).Returns(dialogResult);
            var testExplorerViewModel = new TestExplorerViewModel(eventAggregator, projectManager, projectFileSystem, testRunner, dataManager,
                componentViewBuilder, windowManager, notificationManger, platformProvider, applicationSettings);
            var fixtureViewModel = CreateTestFixtureViewModel(false);
            testExplorerViewModel.TestFixtures.Add(fixtureViewModel);

            //Act
            await testExplorerViewModel.EditTestFixtureAsync(fixtureViewModel);

            //Assert
            int expected = dialogResult ? 1 : 0;
            Assert.That(testExplorerViewModel.CanAddTestFixture);
            Assert.That(testExplorerViewModel.TestFixtures.Count, Is.EqualTo(1));
            await windowManager.Received(1).ShowDialogAsync(Arg.Any<EditTestFixtureViewModel>());
            await dataManager.Received(expected).UpdateTestFixtureAsync(Arg.Any<TestFixture>());
        }

        [Ignore("Needs refactoring to get rid of MessageBox and Directory.Delete")]
        [TestCase]
        public void ValidateThatTestFixtureCanBeDeleted()
        {
            //TODO
        }

        /// <summary>
        /// Validate that it is possible to open an existing fixture for editing the fixture process
        /// </summary>
        /// <param name="isOpenForEdit">Whether the fixture is already open for edit</param>
        /// <returns></returns>
        [TestCase(true)]
        [TestCase(false)]
        public async Task ValidateThatTestFixtureCanBeOpened(bool isOpenForEdit)
        {
            //Arrange
            var fixtureViewModel = CreateTestFixtureViewModel(isOpenForEdit);
            var testFixture = fixtureViewModel.TestFixture;
            projectManager.Load<Entity>(Arg.Any<string>()).Returns(fixtureViewModel.TestFixtureEntity);
            testRunner.TryOpenTestFixture(Arg.Is<TestFixture>(testFixture)).Returns(true);

            //Act
            var testExplorerViewModel = new TestExplorerViewModel(eventAggregator, projectManager, projectFileSystem, testRunner, dataManager,
                componentViewBuilder, windowManager, notificationManger, platformProvider, applicationSettings);
            testExplorerViewModel.TestFixtures.Add(fixtureViewModel);
            await testExplorerViewModel.OpenTestFixtureAsync(fixtureViewModel.FixtureId);

            //Assert
            Assert.That(fixtureViewModel.IsOpenForEdit);
            Assert.That(fixtureViewModel.TestFixtureEntity is not null);
            Assert.That(fixtureViewModel.TestFixtureEntity.Name.Equals(testFixture.DisplayName));
            Assert.That(fixtureViewModel.TestFixtureEntity.Tag.Equals(testFixture.FixtureId));

            int expected = isOpenForEdit ? 0 : 1;        
            await dataManager.Received(expected).DownloadFixtureDataAsync(Arg.Is<TestFixture>(testFixture));
            projectFileSystem.Received(expected).GetTestFixtureFiles(Arg.Is<TestFixture>(testFixture));
            projectManager.Received(expected).Load<Entity>(Arg.Any<string>());
            await testRunner.Received(expected).TryOpenTestFixture(Arg.Is<TestFixture>(testFixture));
            componentViewBuilder.Received(expected).OpenTestFixture(Arg.Is<TestFixture>(testFixture));
            fixtureEntityManager.Received(expected).GetServiceOfType<IScriptEditorFactory>();
            scriptEditorFactory.Received(expected).AddProject(Arg.Any<string>(), Arg.Is<string[]>(Array.Empty<string>()), Arg.Is<Type>(typeof(EmptyModel)));
            scriptEditorFactory.Received(expected).AddDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        /// <summary>
        /// Validate that a TestFixture that was previously opened can be closed.
        /// </summary>
        /// <param name="isOpenForEdit">Whether the fixture is open for edit</param>   
        /// <returns></returns>
        [TestCase(true)]
        [TestCase(false)]
        public async Task ValidateThatTestFixtureCanBeClosed(bool isOpenForEdit)
        {
            //Arrange
            var fixtureViewModel = CreateTestFixtureViewModel(isOpenForEdit);
            var testFixture = fixtureViewModel.TestFixture;
            fixtureViewModel.AddTestCase(new TestCaseViewModel(new TestCase()) { IsOpenForEdit = false }, this.projectFileSystem);
            testRunner.TryCloseTestFixture(Arg.Is<TestFixture>(testFixture)).Returns(true);

            //Pre-Assert 
            if (isOpenForEdit)
            {
                Assert.That(fixtureViewModel.TestFixtureEntity is not null);
                Assert.That(fixtureViewModel.IsOpenForEdit);
            }

            //Act
            var testExplorerViewModel = new TestExplorerViewModel(eventAggregator, projectManager, projectFileSystem, testRunner, dataManager,
                componentViewBuilder, windowManager, notificationManger, platformProvider, applicationSettings);
            testExplorerViewModel.TestFixtures.Add(fixtureViewModel);
            await testExplorerViewModel.CloseTestFixtureAsync(fixtureViewModel.FixtureId);

            //Assert
            if (isOpenForEdit)
            {
                Assert.That(fixtureViewModel.TestFixtureEntity is null);
                Assert.That(fixtureViewModel.IsOpenForEdit == false);
            }

            int expected = isOpenForEdit ? 1 : 0;
            await dataManager.Received(expected).SaveFixtureDataAsync(Arg.Is<TestFixture>(testFixture));
            await testRunner.Received(expected).TryCloseTestFixture(Arg.Is<TestFixture>(testFixture));
            componentViewBuilder.Received(expected).CloseTestFixture(Arg.Is<TestFixture>(testFixture));
            scriptEditorFactory.Received(expected).RemoveProject(Arg.Is<string>(fixtureViewModel.FixtureId));
            if (isOpenForEdit)
            {
                fixtureEntityManager.Received(2).GetServiceOfType<IScriptEditorFactory>();
                scriptEditorFactory.Received(1).RemoveInlineScriptEditor(Arg.Any<string>()); // 1 when open for edit because of presence of while loop entity
            }
            else
            {
                fixtureEntityManager.Received(0).GetServiceOfType<IScriptEditorFactory>();
            }

        }

        /// <summary>
        /// Validate that it should be possible to open the fixture script to be edited for a TestFixture which is open for edit
        /// </summary>
        /// <param name="isOpenForEdit"></param>
        /// <param name="dialogResult"></param>
        /// <returns></returns>
        [TestCase(false, true)]
        [TestCase(true, true)]
        [TestCase(true, false)]
        public async Task ValidateTestFixtureScriptCanBeEdited(bool isOpenForEdit, bool dialogResult)
        {
            //Arrage
            var fixtureScritEngine = Substitute.For<IScriptEngine>();
            fixtureScritEngine.When(x => x.ClearState()).Do(DoNothing);
            fixtureScritEngine.When(x => x.ExecuteFileAsync(Arg.Any<string>())).Do(DoNothing);
            fixtureEntityManager.GetScriptEngine().Returns(fixtureScritEngine);

            var testEntityManager = Substitute.For<IEntityManager>();
            var testScriptEngine = Substitute.For<IScriptEngine>();
            testScriptEngine.When(x => x.ClearState()).Do(DoNothing);
            testScriptEngine.When(x => x.ExecuteFileAsync(Arg.Any<string>())).Do(DoNothing);
            testEntityManager.GetScriptEngine().Returns(testScriptEngine);

            var scriptEditorScreen = Substitute.For<IScriptEditorScreen>();
            scriptEditorFactory.CreateScriptEditorScreen().Returns(scriptEditorScreen);
            scriptEditorScreen.When(x => x.OpenDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Is<string>(string.Empty))).Do(DoNothing);
            windowManager.ShowDialogAsync(Arg.Any<IScriptEditorScreen>()).Returns(dialogResult);

            var fixtureViewModel = CreateTestFixtureViewModel(isOpenForEdit);

            fixtureViewModel.AddTestCase(new TestCaseViewModel(new TestCase()) 
            { 
                IsOpenForEdit = true, ScriptFile = "TestScript1.csx", TestCaseEntity = new TestCaseEntity() { EntityManager = testEntityManager } 
            }, this.projectFileSystem);
            fixtureViewModel.AddTestCase(new TestCaseViewModel(new TestCase()) 
            { 
                IsOpenForEdit = false, ScriptFile = "TestScript2.csx", TestCaseEntity = new TestCaseEntity() { EntityManager = testEntityManager }
            }, this.projectFileSystem);

            //Act
            var testExplorerViewModel = new TestExplorerViewModel(eventAggregator, projectManager, projectFileSystem, testRunner, dataManager, componentViewBuilder,
                windowManager, notificationManger, platformProvider, applicationSettings);
            await testExplorerViewModel.EditTestFixtureScriptAsync(fixtureViewModel);

            //Assert
            int expected = isOpenForEdit ? 1 : 0;
            fixtureEntityManager.Received(expected).GetServiceOfType<IScriptEditorFactory>();
            scriptEditorFactory.Received(expected).CreateScriptEditorScreen();
            scriptEditorScreen.Received(expected).OpenDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Is<string>(string.Empty));
            await windowManager.Received(expected).ShowDialogAsync(Arg.Any<IScriptEditorScreen>());

            expected = isOpenForEdit && dialogResult ? 1 : 0;
            fixtureEntityManager.Received(expected).GetScriptEngine();
            fixtureScritEngine.Received(expected).ClearState();
            await fixtureScritEngine.Received(expected).ExecuteFileAsync(Arg.Is<string>(fixtureScriptFile));

            testEntityManager.Received(expected).GetScriptEngine();
            testScriptEngine.Received(expected).ClearState();
            await testScriptEngine.Received(expected).ExecuteFileAsync(Arg.Is<string>(fixtureScriptFile));
            await testScriptEngine.Received(expected).ExecuteFileAsync(Arg.Is<string>("TestScript1.csx"));
        }

        [TestCase]
        public async Task ValidateThatTestFixtureDataCanBeSaved()
        {
            var testFixtureViewModel = CreateTestFixtureViewModel(true);
            testFixtureViewModel.ScriptFile = string.Empty;
            var fixtureEntity = testFixtureViewModel.TestFixtureEntity;
            fixtureEntity.AddComponent(new TestCaseEntity());

            var testExplorerViewModel = new TestExplorerViewModel(eventAggregator, projectManager, projectFileSystem, testRunner, dataManager, componentViewBuilder,
                windowManager, notificationManger, platformProvider, applicationSettings);
            await testExplorerViewModel.SaveTestFixtureDataAsync(testFixtureViewModel);

            await dataManager.Received(1).SaveFixtureDataAsync(Arg.Any<TestFixture>());
        }

    }

    /// <summary>
    /// TestFixture for TestExplorerViewModel with focus on managing TestCases e.g. Add, Edit, Delete, etc
    /// </summary>
    [TestFixture]
    public class TestExplorerViewModel_TestCase_Fixture
    {
        private readonly string fixtureScriptFile = "FixtureScript.csx";
        private readonly string testScriptFile = "TestScript.csx";

        private IEventAggregator eventAggregator;
        private IWindowManager windowManager;
        private INotificationManager notificationManger;
        private IPlatformProvider platformProvider;
        private IAutomationProjectManager projectManager;
        private IProjectFileSystem projectFileSystem;
        private IProjectAssetsDataManager dataManager;
        private ITestRunner testRunner;
        private IComponentViewBuilder componentViewBuilder;
        private IEntityManager testEntityManager;
        private IScriptEditorFactory scriptEditorFactory;
        private ApplicationSettings applicationSettings;
        private Action<CallInfo> DoNothing = (a) => { };

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            eventAggregator = Substitute.For<IEventAggregator>();
            windowManager = Substitute.For<IWindowManager>();
            notificationManger = Substitute.For<INotificationManager>();
            platformProvider = Substitute.For<IPlatformProvider>();
            projectManager = Substitute.For<IAutomationProjectManager>();
            projectFileSystem = Substitute.For<IProjectFileSystem>();
            dataManager = Substitute.For<IProjectAssetsDataManager>();
            testRunner = Substitute.For<ITestRunner>();
            componentViewBuilder = Substitute.For<IComponentViewBuilder>();
            testEntityManager = Substitute.For<IEntityManager>();
            scriptEditorFactory = Substitute.For<IScriptEditorFactory>();
            applicationSettings = new ApplicationSettings();
           
            projectFileSystem.GetRelativePath(Arg.Any<string>()).Returns(Path.Combine("FixtureId", testScriptFile));
            projectFileSystem.GetTestFixtureFiles(Arg.Any<TestFixture>()).Returns(new TestFixtureFiles(new TestFixture(), Environment.CurrentDirectory));
            projectFileSystem.When(x => x.CreateOrReplaceFile(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())).Do(DoNothing);
            projectFileSystem.When(x => x.SaveToFile<TestCase>(Arg.Any<TestCase>(), Arg.Any<string>())).Do(DoNothing);
            projectFileSystem.When(x => x.SaveToFile<TestFixture>(Arg.Any<TestFixture>(), Arg.Any<string>(), Arg.Any<string>())).Do(DoNothing);
            projectFileSystem.When(x => x.SaveToFile<Entity>(Arg.Any<Entity>(), Arg.Any<string>(), Arg.Any<string>())).Do(DoNothing);
            projectFileSystem.GetTestCaseFiles(Arg.Any<TestCase>()).Returns(x => new TestCaseFiles(x[0] as TestCase, Environment.CurrentDirectory));

            scriptEditorFactory.When(x => x.AddProject(Arg.Any<string>(), Arg.Is<string[]>(Array.Empty<string>()), Arg.Any<Type>())).Do(DoNothing);
            scriptEditorFactory.When(x => x.AddDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())).Do(DoNothing);
            scriptEditorFactory.When(x => x.RemoveProject(Arg.Any<string>())).Do(DoNothing);
            testEntityManager.GetServiceOfType<IScriptEditorFactory>().Returns(scriptEditorFactory);
        }

        [TearDown]
        public void TearDown()
        {
            eventAggregator.ClearReceivedCalls();
            windowManager.ClearReceivedCalls();
            platformProvider.ClearReceivedCalls();
            projectManager.ClearReceivedCalls();
            projectFileSystem.ClearReceivedCalls();
            dataManager.ClearReceivedCalls();
            testRunner.ClearReceivedCalls();
            testEntityManager.ClearReceivedCalls();
            scriptEditorFactory.ClearReceivedCalls();
        }

        TestFixtureViewModel CreateTestFixtureViewModel(bool isOpenForEdit)
        {
            var fixtureEntity = new TestFixtureEntity() { EntityManager = testEntityManager };
            TestFixture testFixture = new TestFixture()
            {
                DisplayName = $"Fixture#1",
                Order = 1,
                ScriptFile = fixtureScriptFile
            };
            TestFixtureViewModel testFixtureVM = new TestFixtureViewModel(testFixture)
            {
                IsOpenForEdit = isOpenForEdit,
                TestFixtureEntity = fixtureEntity
            };
            if (isOpenForEdit)
            {
                fixtureEntity.Name = testFixtureVM.DisplayName;
                fixtureEntity.Tag = testFixtureVM.FixtureId;
            }
            return testFixtureVM;
        }

        TestCaseViewModel CreateTestCaseViewModel(TestFixture parentFixture, bool isOpenForEdit)
        {
            var testEntity = new TestCaseEntity() { EntityManager = testEntityManager };
            var seqeunceEntity = new SequenceEntity();
            testEntity.AddComponent(seqeunceEntity);
            // WhileLoopEntity has Scriptable Attribute. We need this to assert a condition while closing test case
            var whileLoopEntity = new WhileLoopEntity() { ScriptFile = "Script.csx" };
            seqeunceEntity.AddComponent(whileLoopEntity);

            TestCase testCase = new TestCase()
            {
                FixtureId = parentFixture.FixtureId,
                DisplayName = $"Test#{parentFixture.Tests.Count() + 1}",
                Order = parentFixture.Tests.Count() + 1,
                TestCaseEntity = testEntity,
                ScriptFile = testScriptFile,
                TestDataId = "test-data-id"
            };
            TestCaseViewModel testCaseVM = new TestCaseViewModel(testCase)
            {
                IsOpenForEdit = isOpenForEdit
            };
            if (isOpenForEdit)
            {
                testEntity.Name = testCase.DisplayName;
                testEntity.Tag = testCase.TestCaseId;
            }
            return testCaseVM;
        }

        /// <summary>
        /// Validate that it is possible to add a new TestCase to an existing TestFixture using add AddTestCaseAsync(TestFixture)
        /// </summary>
        /// <param name="dialogResult">Boolean result of whether user clicked save or cancel on screen</param>
        /// <returns></returns>
        [TestCase(true)]
        [TestCase(false)]
        public async Task ValidateThatTestCaseCanBeAdded(bool dialogResult)
        {
            //Arrange
            windowManager.ShowDialogAsync(Arg.Any<EditTestCaseViewModel>()).Returns(dialogResult);
            var testFixtureViewModel = CreateTestFixtureViewModel(false);
            var testExplorerViewModel = new TestExplorerViewModel(eventAggregator, projectManager, projectFileSystem, testRunner, dataManager,
                componentViewBuilder, windowManager, notificationManger, platformProvider, applicationSettings);
            testExplorerViewModel.TestFixtures.Add(testFixtureViewModel);

            //Act
            await testExplorerViewModel.AddTestCaseAsync(testFixtureViewModel);

            //Assert
            int expected = dialogResult ? 1 : 0;
            Assert.That(testFixtureViewModel.Tests.Count, Is.EqualTo(expected));
            await windowManager.Received(1).ShowDialogAsync(Arg.Any<EditTestCaseViewModel>());
            await dataManager.Received(expected).AddTestCaseAsync(Arg.Any<TestCase>());
        }

        /// <summary>
        /// Validate that it is possible to edit an existing TestCase
        /// </summary>
        /// <param name="dialogResult">Boolean result of whether user clicked save (to accept edits) or cancel (to discard edits) on screen </param>
        /// <returns></returns>
        [TestCase(true)]
        [TestCase(false)]
        public async Task ValidateThatTestCaseCanBeEdited(bool dialogResult)
        {
            //Arrange
            windowManager.ShowDialogAsync(Arg.Any<EditTestCaseViewModel>()).Returns(dialogResult);
            var testExplorerViewModel = new TestExplorerViewModel(eventAggregator, projectManager, projectFileSystem, testRunner, dataManager,
                componentViewBuilder, windowManager, notificationManger, platformProvider, applicationSettings);
            var fixtureViewModel = CreateTestFixtureViewModel(false);
            testExplorerViewModel.TestFixtures.Add(fixtureViewModel);
            var testCaseViewModel = CreateTestCaseViewModel(fixtureViewModel.TestFixture, true);
            fixtureViewModel.AddTestCase(testCaseViewModel, this.projectFileSystem);

            //Act
            await testExplorerViewModel.EditTestCaseAsync(testCaseViewModel);

            //Assert
            int expected = dialogResult ? 1 : 0;
            await windowManager.Received(1).ShowDialogAsync(Arg.Any<EditTestCaseViewModel>());
            await dataManager.Received(expected).UpdateTestCaseAsync(Arg.Any<TestCase>());
        }


        [Ignore("Needs refactoring to get rid of MessageBox and Directory.Delete")]
        [TestCase]
        public void ValidateThatTestCaseCanBeDeleted()
        {
            //TODO
        }

        /// <summary>
        /// Validate that it is possible to open an existing test case for editing the test process
        /// </summary>
        /// <param name="isOpenForEdit">Whether the test case is already open for edit</param>
        /// <returns></returns>
        [TestCase(true)]
        [TestCase(false)]
        public async Task ValidateThatTestCaseCanBeOpened(bool isOpenForEdit)
        {
            //Arrange        
            var fixtureViewModel = CreateTestFixtureViewModel(true);
            var testCaseViewModel = CreateTestCaseViewModel(fixtureViewModel.TestFixture, isOpenForEdit);
            fixtureViewModel.AddTestCase(testCaseViewModel, this.projectFileSystem);

            var testExplorerViewModel = new TestExplorerViewModel(eventAggregator, projectManager, projectFileSystem, testRunner, dataManager,
                componentViewBuilder, windowManager, notificationManger, platformProvider, applicationSettings);
            testExplorerViewModel.TestFixtures.Add(fixtureViewModel);

            projectManager.Load<Entity>(Arg.Any<string>()).Returns(testCaseViewModel.TestCaseEntity);
            testRunner.TryOpenTestCase(Arg.Is<TestFixture>(fixtureViewModel.TestFixture), Arg.Is<TestCase>(testCaseViewModel.TestCase)).Returns(true).AndDoes(
                        x =>
                        {
                            x.ArgAt<TestCase>(1).TestCaseEntity.EntityManager.Arguments = new EmptyModel();
                        });

            //Act
            testExplorerViewModel.TestFixtures.Add(fixtureViewModel);
            await testExplorerViewModel.OpenTestCaseAsync(testCaseViewModel.TestCaseId);

            //Assert
            Assert.That(testCaseViewModel.IsOpenForEdit);
            Assert.That(testCaseViewModel.TestCaseEntity is not null);
            Assert.That(testCaseViewModel.TestCaseEntity.Name.Equals(testCaseViewModel.DisplayName));
            Assert.That(testCaseViewModel.TestCaseEntity.Tag.Equals(testCaseViewModel.TestCaseId));

            int expected = isOpenForEdit ? 0 : 1;
            await dataManager.Received(expected).DownloadTestDataAsync(Arg.Any<TestCase>());
            projectFileSystem.Received(expected).GetTestCaseFiles(Arg.Any<TestCase>());
            projectManager.Received(expected).Load<Entity>(Arg.Any<string>());
            await testRunner.Received(expected).TryOpenTestCase(Arg.Is<TestFixture>(fixtureViewModel.TestFixture), Arg.Is<TestCase>(testCaseViewModel.TestCase));
            componentViewBuilder.Received(expected).OpenTestCase(Arg.Is<TestCase>(testCaseViewModel.TestCase));
            testEntityManager.Received(expected).GetServiceOfType<IScriptEditorFactory>();
            scriptEditorFactory.Received(expected).AddProject(Arg.Any<string>(), Arg.Any<string[]>(), Arg.Is<Type>(typeof(EmptyModel)));
            scriptEditorFactory.Received(expected).AddDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        /// <summary>
        /// Validate that a TestCase that was previously opened can be closed.
        /// </summary>
        /// <param name="isOpenForEdit">Whether the test is open for edit</param>   
        /// <returns></returns>
        [TestCase(true)]
        [TestCase(false)]
        public async Task ValidateThatTestCaseCanBeClosed(bool isOpenForEdit)
        {
            //Arrange
            var fixtureViewModel = CreateTestFixtureViewModel(true);
            var testCaseViewModel = CreateTestCaseViewModel(fixtureViewModel.TestFixture, isOpenForEdit);
            fixtureViewModel.AddTestCase(testCaseViewModel, this.projectFileSystem);

            testRunner.TryCloseTestCase(Arg.Is<TestFixture>(fixtureViewModel.TestFixture), Arg.Is<TestCase>(testCaseViewModel.TestCase)).Returns(true);

            var testExplorerViewModel = new TestExplorerViewModel(eventAggregator, projectManager, projectFileSystem, testRunner, dataManager,
                componentViewBuilder, windowManager, notificationManger, platformProvider, applicationSettings);
            testExplorerViewModel.TestFixtures.Add(fixtureViewModel);

            //Act            
            await testExplorerViewModel.CloseTestCaseAsync(testCaseViewModel.TestCaseId);

            //Assert
            if (isOpenForEdit)
            {
                Assert.That(testCaseViewModel.TestCaseEntity is null);
                Assert.That(testCaseViewModel.IsOpenForEdit == false);
            }

            int expected = isOpenForEdit ? 1 : 0;
            await dataManager.Received(expected).SaveTestDataAsync(Arg.Is<TestCase>(testCaseViewModel.TestCase));
            await testRunner.Received(expected).TryCloseTestCase(Arg.Is<TestFixture>(fixtureViewModel.TestFixture), Arg.Is<TestCase>(testCaseViewModel.TestCase));
            componentViewBuilder.Received(expected).CloseTestCase(Arg.Is<TestCase>(testCaseViewModel.TestCase));
            scriptEditorFactory.Received(expected).RemoveProject(Arg.Is<string>(testCaseViewModel.TestCaseId));
            if (isOpenForEdit)
            {
                testEntityManager.Received(2).GetServiceOfType<IScriptEditorFactory>();
                scriptEditorFactory.Received(1).RemoveInlineScriptEditor(Arg.Any<string>()); // 1 when open for edit because of presence of while loop entity
            }
            else
            {
                testEntityManager.Received(0).GetServiceOfType<IScriptEditorFactory>();
            }

        }

        /// <summary>
        /// Validate that it should be possible to open the fixture script to be edited for a TestFixture which is open for edit
        /// </summary>
        /// <param name="isOpenForEdit"></param>
        /// <param name="dialogResult"></param>
        /// <returns></returns>
        [TestCase(false, true)]
        [TestCase(true, true)]
        [TestCase(true, false)]
        public async Task ValidateTestCaseScriptCanBeEdited(bool isOpenForEdit, bool dialogResult)
        {
            //Arrage
            var testScriptEngine = Substitute.For<IScriptEngine>();
            testScriptEngine.When(x => x.ClearState()).Do(DoNothing);
            testScriptEngine.When(x => x.ExecuteFileAsync(Arg.Any<string>())).Do(DoNothing);
            testEntityManager.GetScriptEngine().Returns(testScriptEngine);

            var scriptEditorScreen = Substitute.For<IScriptEditorScreen>();
            scriptEditorFactory.CreateScriptEditorScreen().Returns(scriptEditorScreen);
            scriptEditorScreen.When(x => x.OpenDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Is<string>(string.Empty))).Do(DoNothing);
            windowManager.ShowDialogAsync(Arg.Any<IScriptEditorScreen>()).Returns(dialogResult);

            var fixtureViewModel = CreateTestFixtureViewModel(true);
            var testCaseViewModel = CreateTestCaseViewModel(fixtureViewModel.TestFixture, isOpenForEdit);

            //Act
            var testExplorerViewModel = new TestExplorerViewModel(eventAggregator, projectManager, projectFileSystem, testRunner, dataManager,
                componentViewBuilder, windowManager, notificationManger, platformProvider, applicationSettings);
            testExplorerViewModel.TestFixtures.Add(fixtureViewModel);
            await testExplorerViewModel.EditTestScriptAsync(testCaseViewModel);

            //Assert
            int expected = isOpenForEdit ? 1 : 0;
            testEntityManager.Received(expected).GetServiceOfType<IScriptEditorFactory>();
            scriptEditorFactory.Received(expected).CreateScriptEditorScreen();
            scriptEditorScreen.Received(expected).OpenDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Is<string>(string.Empty));
            await windowManager.Received(expected).ShowDialogAsync(Arg.Any<IScriptEditorScreen>());

            expected = isOpenForEdit && dialogResult ? 1 : 0;
            testEntityManager.Received(expected).GetScriptEngine();
            testScriptEngine.Received(expected).ClearState();
            await testScriptEngine.Received(expected).ExecuteFileAsync(Arg.Is<string>(fixtureScriptFile));
            await testScriptEngine.Received(expected).ExecuteFileAsync(Arg.Is<string>(testScriptFile));
        }

      
        public async Task ValidateThatTestCaseCanBeSaved()
        {
            //Arrange
            var fixtureViewModel = CreateTestFixtureViewModel(true);
            var testCaseViewModel = CreateTestCaseViewModel(fixtureViewModel.TestFixture, true);
            testCaseViewModel.ScriptFile = string.Empty;
            var testEntity = testCaseViewModel.TestCaseEntity;
            testEntity.AddComponent(new PrefabEntity());
            fixtureViewModel.AddTestCase(testCaseViewModel, this.projectFileSystem);

            //Act
            var testExplorerViewModel = new TestExplorerViewModel(eventAggregator, projectManager, projectFileSystem, testRunner, dataManager,
                componentViewBuilder, windowManager, notificationManger, platformProvider, applicationSettings);
            testExplorerViewModel.TestFixtures.Add(fixtureViewModel);
            await testExplorerViewModel.SaveTestCaseDataAsync(testCaseViewModel);

            //Assert
            await dataManager.Received(1).SaveTestDataAsync(Arg.Any<TestCase>());

        }
    }

    /// <summary>
    /// TestFixture for <see cref="TestExplorerViewModel"/> for validating execution operations on test cases
    /// </summary>
    //[TestFixture]
    public class TestExplorerViewModel_Execute_Fixture
    {      
        private readonly string fixtureScriptFile = "FixtureScript.csx";      
        private readonly string testScriptFile = "TestScript.csx";

        private IEventAggregator eventAggregator;
        private IWindowManager windowManager;
        private INotificationManager notificationManager;
        private IPlatformProvider platformProvider;
        private IAutomationProjectManager projectManager;
        private IProjectFileSystem projectFileSystem;
        private IProjectAssetsDataManager dataManager;
        private ITestRunner testRunner;
        private IComponentViewBuilder componentViewBuilder;
        private IEntityManager testEntityManager;
        private IScriptEditorFactory scriptEditorFactory;
        private ApplicationSettings applicationSettings;
        private Action<CallInfo> DoNothing = (a) => { };

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            eventAggregator = Substitute.For<IEventAggregator>();
            windowManager = Substitute.For<IWindowManager>();
            notificationManager = Substitute.For<INotificationManager>();
            platformProvider = Substitute.For<IPlatformProvider>();
            projectManager = Substitute.For<IAutomationProjectManager>();
            projectFileSystem = Substitute.For<IProjectFileSystem>();
            dataManager = Substitute.For<IProjectAssetsDataManager>();
            testRunner = Substitute.For<ITestRunner>();
            componentViewBuilder = Substitute.For<IComponentViewBuilder>();
            testEntityManager = Substitute.For<IEntityManager>();
            scriptEditorFactory = Substitute.For<IScriptEditorFactory>();
            applicationSettings = new ApplicationSettings();

            platformProvider.When(x => x.OnUIThread(Arg.Any<System.Action>())).Do(a =>
            {
                var action = a.ArgAt<System.Action>(0);
                action.Invoke();
            });
                      
            var testFixtureEntity = new TestFixtureEntity() { EntityManager = testEntityManager };
            var testCaseEntity = new TestCaseEntity() { EntityManager = testEntityManager };
            projectManager.Load<Entity>(Arg.Any<string>()).Returns(testFixtureEntity);
            projectManager.Load<Entity>(Arg.Any<string>()).Returns(testCaseEntity);

            projectFileSystem.GetRelativePath(Arg.Any<string>()).Returns(Path.Combine("TestId", testScriptFile));
            projectFileSystem.When(x => x.CreateOrReplaceFile(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())).Do(DoNothing);
            projectFileSystem.When(x => x.SaveToFile<TestCase>(Arg.Any<TestCase>(), Arg.Any<string>())).Do(DoNothing);
            projectFileSystem.When(x => x.SaveToFile<TestFixture>(Arg.Any<TestFixture>(), Arg.Any<string>(), Arg.Any<string>())).Do(DoNothing);
            projectFileSystem.When(x => x.SaveToFile<Entity>(Arg.Any<Entity>(), Arg.Any<string>(), Arg.Any<string>())).Do(DoNothing);
            projectFileSystem.GetTestFixtureFiles(Arg.Any<TestFixture>()).Returns(x => new TestFixtureFiles(x[0] as TestFixture, Environment.CurrentDirectory));
            projectFileSystem.GetTestCaseFiles(Arg.Any<TestCase>()).Returns(x => new TestCaseFiles(x[0] as TestCase, Environment.CurrentDirectory));

            scriptEditorFactory.When(x => x.AddProject(Arg.Any<string>(), Arg.Is<string[]>(Array.Empty<string>()), Arg.Any<Type>())).Do(DoNothing);
            scriptEditorFactory.When(x => x.AddDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())).Do(DoNothing);
            scriptEditorFactory.When(x => x.RemoveProject(Arg.Any<string>())).Do(DoNothing);

            testEntityManager.GetServiceOfType<IScriptEditorFactory>().Returns(scriptEditorFactory);
            testEntityManager.Arguments.Returns(new EmptyModel());

            testRunner.TryOpenTestFixture(Arg.Any<TestFixture>()).Returns(true);
            testRunner.TryOpenTestCase(Arg.Any<TestFixture>(), Arg.Any<TestCase>()).Returns(true);
            testRunner.When(x => x.OneTimeSetUp(Arg.Any<TestFixture>())).Do(DoNothing);
            testRunner.When(x => x.OneTimeTearDown(Arg.Any<TestFixture>())).Do(DoNothing);
            testRunner.When(x => x.SetUpEnvironment()).Do((a) =>
            {
                testRunner.CanRunTests.Returns(true);
            });
            testRunner.When(x => x.TearDownEnvironment()).Do((a) =>
            {
                testRunner.CanRunTests.Returns(false);
            });
            testRunner.RunTestAsync(Arg.Any<TestFixture>(), Arg.Any<TestCase>()).Returns(GetResults());

            async IAsyncEnumerable<TestResult> GetResults()
            {
                yield return new TestResult() { Result = Core.TestData.TestStatus.Success };
                await Task.CompletedTask;
                yield break;
            }
        }

        TestFixtureViewModel CreateTestFixtureViewModel(bool isOpenForEdit)
        {
            TestFixture testFixture = new TestFixture()
            {
                DisplayName = $"Fixture#1",
                Order = 1,
                ScriptFile = fixtureScriptFile
            };
            TestFixtureViewModel testFixtureVM = new TestFixtureViewModel(testFixture)
            {
                IsOpenForEdit = isOpenForEdit
            };
            return testFixtureVM;
        }

        TestCaseViewModel CreateTestCaseViewModel(TestFixture parentFixture, bool isOpenForEdit)
        {
            TestCase testCase = new TestCase()
            {
                FixtureId = parentFixture.FixtureId,
                DisplayName = $"Test#{parentFixture.Tests.Count() + 1}",
                Order = parentFixture.Tests.Count() + 1,
                ScriptFile = testScriptFile,
                TestDataId = "test-data-id"
            };
            TestCaseViewModel testCaseVM = new TestCaseViewModel(testCase)
            {
                IsOpenForEdit = isOpenForEdit
            };
            return testCaseVM;
        }

        [TearDown]
        public void TearDown()
        {
            eventAggregator.ClearReceivedCalls();
            windowManager.ClearReceivedCalls();
            platformProvider.ClearReceivedCalls();
            projectManager.ClearReceivedCalls();
            projectFileSystem.ClearReceivedCalls();
            dataManager.ClearReceivedCalls();
            testRunner.ClearReceivedCalls();
            testEntityManager.ClearReceivedCalls();
            scriptEditorFactory.ClearReceivedCalls();
        }

        [Test]
        public async Task ValidateCanSetupAndTearDownEnvironment()
        {
            var testExplorerViewModel = new TestExplorerViewModel(eventAggregator, projectManager, projectFileSystem, testRunner, dataManager,
                componentViewBuilder, windowManager, notificationManager, platformProvider, applicationSettings);

            Assert.That(testExplorerViewModel.CanSetUpEnvironment);

            await testExplorerViewModel.SetUpEnvironmentAsync();

            Assert.That(testExplorerViewModel.CanSetUpEnvironment == false);
            Assert.That(testExplorerViewModel.CanRunTests);
            Assert.That(testExplorerViewModel.CanTearDownEnvironment);

            await testExplorerViewModel.TearDownEnvironmentAsync();

            Assert.That(testExplorerViewModel.CanSetUpEnvironment);
            Assert.That(testExplorerViewModel.CanRunTests == false);
            Assert.That(testExplorerViewModel.CanTearDownEnvironment == false);

            await testRunner.Received(1).SetUpEnvironment();
            await testRunner.Received(1).TearDownEnvironment();
        }

        [Test]
        public async Task ValidateThatCanRunSelectedTest()
        {
            var fixtureViewModel = CreateTestFixtureViewModel(false);
            var testCaseViewModel = CreateTestCaseViewModel(fixtureViewModel.TestFixture, false);
            fixtureViewModel.AddTestCase(testCaseViewModel, this.projectFileSystem);
            testCaseViewModel.IsSelected = true;

            var testExplorerViewModel = new TestExplorerViewModel(eventAggregator, projectManager, projectFileSystem, testRunner, dataManager,
                                  componentViewBuilder, windowManager, notificationManager, platformProvider, applicationSettings);
            testExplorerViewModel.TestFixtures.Add(fixtureViewModel);

            await testExplorerViewModel.RunSelected();

            Assert.That(testCaseViewModel.TestResults.Count(), Is.EqualTo(1));
            await testRunner.Received(1).OneTimeSetUp(Arg.Is(fixtureViewModel.TestFixture));
            await testRunner.Received(1).OneTimeTearDown(Arg.Is(fixtureViewModel.TestFixture));
            await testRunner.Received(1).TryOpenTestFixture(Arg.Is(fixtureViewModel.TestFixture));
            await testRunner.Received(1).TryCloseTestFixture(Arg.Is(fixtureViewModel.TestFixture));
            await testRunner.Received(1).TryOpenTestCase(Arg.Is(fixtureViewModel.TestFixture), Arg.Is(testCaseViewModel.TestCase));
            await testRunner.Received(1).TryCloseTestCase(Arg.Is(fixtureViewModel.TestFixture), Arg.Is(testCaseViewModel.TestCase));
        }

        [Test]
        public async Task ValidateThatCanRunAllTests()
        {
            var fixtureViewModel = CreateTestFixtureViewModel(false);
            var testCaseViewModel = CreateTestCaseViewModel(fixtureViewModel.TestFixture, false);
            fixtureViewModel.AddTestCase(testCaseViewModel, this.projectFileSystem);
            testCaseViewModel.IsSelected = true;

            var testExplorerViewModel = new TestExplorerViewModel(eventAggregator, projectManager, projectFileSystem, testRunner, dataManager,
                                                        componentViewBuilder, windowManager, notificationManager, platformProvider, applicationSettings);
            testExplorerViewModel.TestFixtures.Add(fixtureViewModel);

            await testExplorerViewModel.RunAll();

            Assert.That(testCaseViewModel.TestResults.Count(), Is.EqualTo(1));
            await testRunner.Received(1).OneTimeSetUp(Arg.Is(fixtureViewModel.TestFixture));
            await testRunner.Received(1).OneTimeTearDown(Arg.Is(fixtureViewModel.TestFixture));
            await testRunner.Received(1).TryOpenTestFixture(Arg.Is(fixtureViewModel.TestFixture));
            await testRunner.Received(1).TryCloseTestFixture(Arg.Is(fixtureViewModel.TestFixture));
            await testRunner.Received(1).TryOpenTestCase(Arg.Is(fixtureViewModel.TestFixture), Arg.Is(testCaseViewModel.TestCase));
            await testRunner.Received(1).TryCloseTestCase(Arg.Is(fixtureViewModel.TestFixture), Arg.Is(testCaseViewModel.TestCase));
        }
    }
}
