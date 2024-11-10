using Caliburn.Micro;
using Notifications.Wpf.Core;
using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Notifications;
using Pixel.Automation.Reference.Manager.Contracts;
using Pixel.Persistence.Services.Client.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Automation.TestDataExplorer.ViewModels.Tests
{
    /// <summary>
    /// Test Fixture for <see cref="TestDataRepositoryViewModel"/>
    /// </summary>
    [TestFixture]
    public class TestDataRepositoryViewModelFixture
    {
        private ISerializer serializer;
        private IProjectFileSystem projectFileSystem;
        private IScriptEditorFactory scriptEditorFactory;
        private IArgumentTypeBrowserFactory typeBrowserFactory;
        private IWindowManager windowManager;
        private INotificationManager notificationManager;
        private IEventAggregator eventAggregator;
        private IProjectAssetsDataManager dataManager;
        private IAutomationProjectManager projectManager;
        private IReferenceManager referenceManager;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            serializer = Substitute.For<ISerializer>();
            projectFileSystem = Substitute.For<IProjectFileSystem>();
            scriptEditorFactory = Substitute.For<IScriptEditorFactory>();
            typeBrowserFactory = Substitute.For<IArgumentTypeBrowserFactory>();
            windowManager = Substitute.For<IWindowManager>();
            notificationManager = Substitute.For<INotificationManager>();
            eventAggregator = Substitute.For<IEventAggregator>();
            dataManager = Substitute.For<IProjectAssetsDataManager>();
            projectManager = Substitute.For<IAutomationProjectManager>();
            referenceManager = Substitute.For<IReferenceManager>();

            var codeDataSource = new TestDataSource()
            {
                DataSourceId = "Code",
                DataSource = DataSource.Code,
                Name = "CodeDataSource",
                ScriptFile = "CodeDataSource.csx",
                MetaData = new DataSourceConfiguration()
                {
                    TargetTypeName = typeof(EmptyModel).Name
                }
            };

            var csvDataSource = new TestDataSource()
            {
                DataSourceId = "Csv",
                DataSource = DataSource.CsvFile,
                Name = "CodedDataSource",
                ScriptFile = "CsvDataSource.csx",
                MetaData = new CsvDataSourceConfiguration()
                {
                    TargetTypeName = typeof(EmptyModel).Name,
                    TargetFile = "Empty.csv"
                }
            };

            projectManager.GetReferenceManager().Returns(referenceManager);
            referenceManager.GetTestDataSourceGroups().Returns(new[] { "Group-Code", "Group-Csv" });
            referenceManager.GetTestDataSources(Arg.Is<string>("Group-Code")).Returns(new[] { codeDataSource.DataSourceId });
            referenceManager.GetTestDataSources(Arg.Is<string>("Group-Csv")).Returns(new[] { csvDataSource.DataSourceId });

            projectFileSystem.WorkingDirectory.Returns(Environment.CurrentDirectory);
            projectFileSystem.TestDataRepository.Returns(Path.Combine(Environment.CurrentDirectory, "TestDataRepository"));
            projectFileSystem.GetTestDataSourceById(Arg.Is<string>(codeDataSource.DataSourceId)).Returns(codeDataSource);
            projectFileSystem.GetTestDataSourceById(Arg.Is<string>(csvDataSource.DataSourceId)).Returns(csvDataSource);

            typeBrowserFactory.CreateArgumentTypeBrowser().Returns(Substitute.For<IArgumentTypeBrowser>());

            windowManager.ShowDialogAsync(Arg.Any<TestDataSourceBuilderViewModel>()).Returns(true);
            windowManager.ShowDialogAsync(Arg.Any<IScriptEditorScreen>()).Returns(true);

            serializer.When(x => x.Serialize<TestDataSource>(Arg.Any<string>(), Arg.Any<TestDataSource>())).Do(x => { });
        }

        [TearDown]
        public void TearDown()
        {
            serializer.ClearReceivedCalls();
            projectFileSystem.ClearReceivedCalls();
            dataManager.ClearReceivedCalls();
            scriptEditorFactory.ClearReceivedCalls();
            typeBrowserFactory.ClearReceivedCalls();
            windowManager.ClearReceivedCalls();
            referenceManager.ClearReceivedCalls();
        }

        [TestCase]
        public async Task ValidateThatTestDataRepositoryViewModelCanBeCorrectlyInitialized()
        {
            var testDataRepositoryViewModel = new TestDataExplorerViewModel(serializer, projectManager, projectFileSystem, scriptEditorFactory,
                windowManager, notificationManager, eventAggregator, typeBrowserFactory, dataManager);

            Assert.That(testDataRepositoryViewModel.TestDataSourceCollection.Count, Is.EqualTo(0));
            Assert.That(testDataRepositoryViewModel.SelectedTestDataSource is null);
            Assert.That(string.IsNullOrEmpty(testDataRepositoryViewModel.FilterText));

            //projectManager.ProjectLoaded += Raise.Event<AsyncEventHandler<ProjectLoadedEventArgs>>();
            await testDataRepositoryViewModel.OnProjectLoaded(projectManager, new ProjectLoadedEventArgs(string.Empty, string.Empty, null));

            Assert.That(testDataRepositoryViewModel.Groups.Count, Is.EqualTo(2));
            Assert.That(testDataRepositoryViewModel.SelectedGroup, Is.EqualTo("Group-Code"));
            Assert.That(testDataRepositoryViewModel.TestDataSourceCollection.Count, Is.EqualTo(1));        
            referenceManager.Received(2).GetTestDataSources(Arg.Is<string>("Group-Code"));
            projectFileSystem.Received(1).GetTestDataSourceById(Arg.Is<string>("Code"));
        }

        [TestCase]
        public async Task ValidateThatCanCreateNewCodeDataSource()
        {
            var testDataRepositoryViewModel = new TestDataExplorerViewModel(serializer, projectManager, projectFileSystem, scriptEditorFactory,
                windowManager, notificationManager, eventAggregator, typeBrowserFactory , dataManager);

            //projectManager.ProjectLoaded += Raise.Event<AsyncEventHandler<ProjectLoadedEventArgs>>();
            await testDataRepositoryViewModel.OnProjectLoaded(projectManager, new ProjectLoadedEventArgs(string.Empty, string.Empty, null));

            Assert.That(testDataRepositoryViewModel.SelectedGroup, Is.EqualTo("Group-Code"));
            Assert.That(testDataRepositoryViewModel.TestDataSourceCollection.Count, Is.EqualTo(1));

            await testDataRepositoryViewModel.CreateCodedTestDataSource();

            Assert.That(testDataRepositoryViewModel.SelectedGroup, Is.EqualTo("Group-Code"));
            Assert.That(testDataRepositoryViewModel.TestDataSourceCollection.Count, Is.EqualTo(2));
          
            await dataManager.Received(1).AddTestDataSourceAsync(Arg.Any<string>(), Arg.Any<TestDataSource>());
            await windowManager.Received(1).ShowDialogAsync(Arg.Any<TestDataSourceBuilderViewModel>());
        }

        [TestCase]
        public async Task ValidateThatCanCreateNewCsvDataSource()
        {
            var testDataRepositoryViewModel = new TestDataExplorerViewModel(serializer, projectManager, projectFileSystem, scriptEditorFactory,
                windowManager, notificationManager, eventAggregator, typeBrowserFactory, dataManager);
            
            //projectManager.ProjectLoaded += Raise.Event<AsyncEventHandler<ProjectLoadedEventArgs>>();
            await testDataRepositoryViewModel.OnProjectLoaded(projectManager, new ProjectLoadedEventArgs(string.Empty, string.Empty, null));

            Assert.That(testDataRepositoryViewModel.SelectedGroup, Is.EqualTo("Group-Code"));
            Assert.That(testDataRepositoryViewModel.TestDataSourceCollection.Count, Is.EqualTo(1));

            testDataRepositoryViewModel.SelectedGroup = "Group-Csv";
            Assert.That(testDataRepositoryViewModel.SelectedGroup, Is.EqualTo("Group-Csv"));
            Assert.That(testDataRepositoryViewModel.TestDataSourceCollection.Count, Is.EqualTo(1));

            await testDataRepositoryViewModel.CreateCsvTestDataSource();

            Assert.That(testDataRepositoryViewModel.SelectedGroup, Is.EqualTo("Group-Csv"));
            Assert.That(testDataRepositoryViewModel.TestDataSourceCollection.Count, Is.EqualTo(2));
          
            await dataManager.Received(1).AddTestDataSourceAsync(Arg.Any<string>(), Arg.Any<TestDataSource>());
            await windowManager.Received(1).ShowDialogAsync(Arg.Any<TestDataSourceBuilderViewModel>());
        }

        [TestCase]
        public async Task ValidateThatCanEditCodedDataSource()
        {
            //Arrange
            var scriptEditor = Substitute.For<IScriptEditorScreen>();
            scriptEditor.When(x => x.OpenDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Is<string>(string.Empty))).Do(x => { });
            scriptEditorFactory.CreateScriptEditorScreen().Returns(scriptEditor);
            scriptEditorFactory.When(x => x.AddProject(Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<Type>())).Do(XamlGeneratedNamespace => { });
            scriptEditorFactory.When(x => x.RemoveProject(Arg.Any<string>())).Do(x => { });

            var testDataRepositoryViewModel = new TestDataExplorerViewModel(serializer, projectManager, projectFileSystem, scriptEditorFactory,
                windowManager, notificationManager, eventAggregator, typeBrowserFactory, dataManager);
       
            //projectManager.ProjectLoaded += Raise.Event<AsyncEventHandler<ProjectLoadedEventArgs>>();
            await testDataRepositoryViewModel.OnProjectLoaded(projectManager, new ProjectLoadedEventArgs(string.Empty, string.Empty, null));
            var codeDataSource = testDataRepositoryViewModel.TestDataSourceCollection.First(a => a.DataSource.Equals(DataSource.Code));

            //Act
            await testDataRepositoryViewModel.EditDataSource(codeDataSource);

            //Assert
            serializer.Received(0).Serialize<TestDataSource>(Arg.Any<string>(), Arg.Any<TestDataSource>());
            scriptEditorFactory.Received(1).CreateScriptEditorScreen();
            scriptEditorFactory.Received(1).AddProject(Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<Type>());
            scriptEditor.Received(1).OpenDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Is<string>(string.Empty));
            await dataManager.Received(1).SaveTestDataSourceDataAsync(Arg.Any<TestDataSource>());
            await windowManager.Received(1).ShowDialogAsync(Arg.Any<IScriptEditorScreen>());
        }

        [TestCase]
        public async Task ValidateThatCanEditCsvDataSource()
        {
            //Arrange
            var testDataRepositoryViewModel = new TestDataExplorerViewModel(serializer, projectManager, projectFileSystem, scriptEditorFactory,
                windowManager, notificationManager, eventAggregator, typeBrowserFactory, dataManager);
           
            //projectManager.ProjectLoaded += Raise.Event<AsyncEventHandler<ProjectLoadedEventArgs>>();
            await testDataRepositoryViewModel.OnProjectLoaded(projectManager, new ProjectLoadedEventArgs(string.Empty, string.Empty, null));
            testDataRepositoryViewModel.SelectedGroup = "Group-Csv";
            var csvDataSource = testDataRepositoryViewModel.TestDataSourceCollection.First(a => a.DataSource.Equals(DataSource.CsvFile));

            //Act
            await testDataRepositoryViewModel.EditDataSource(csvDataSource);

            //Assert
            await dataManager.Received(1).UpdateTestDataSourceAsync(Arg.Any<TestDataSource>());            //Assert
            await dataManager.Received(1).SaveTestDataSourceDataAsync(Arg.Any<TestDataSource>());
            await windowManager.Received(1).ShowDialogAsync(Arg.Any<TestDataSourceBuilderViewModel>());
        }

        [TestCase]
        public async Task ValidateThatTestDataSourceIsClearedWhenDeactivated()
        {
            //Arrange
            var testDataRepositoryViewModel = new TestDataExplorerViewModel(serializer, projectManager, projectFileSystem, scriptEditorFactory,
                windowManager, notificationManager, eventAggregator, typeBrowserFactory, dataManager);
            await testDataRepositoryViewModel.ActivateAsync();
            //projectManager.ProjectLoaded += Raise.Event<AsyncEventHandler<ProjectLoadedEventArgs>>();
            await testDataRepositoryViewModel.OnProjectLoaded(projectManager, new ProjectLoadedEventArgs(string.Empty, string.Empty, null));

            Assert.That(testDataRepositoryViewModel.SelectedGroup, Is.EqualTo("Group-Code"));
            Assert.That(testDataRepositoryViewModel.TestDataSourceCollection.Count, Is.EqualTo(1));

            await testDataRepositoryViewModel.DeactivateAsync(true); //deactivate will happen only if view model was activiated before
            Assert.That(testDataRepositoryViewModel.TestDataSourceCollection.Count, Is.EqualTo(0));
        }

        [TestCase]
        public async Task ValidateThatCanHandleShowTestDataSourceNotificationMessage()
        {
            //Arrange
            var testDataRepositoryViewModel = new TestDataExplorerViewModel(serializer, projectManager, projectFileSystem, scriptEditorFactory,
                windowManager, notificationManager, eventAggregator, typeBrowserFactory, dataManager);

            //Act
            await testDataRepositoryViewModel.HandleAsync(new ShowTestDataSourceNotification("test-data-id"), CancellationToken.None);

            //Assert
            Assert.That(testDataRepositoryViewModel.FilterText, Is.EqualTo("test-data-id"));

        }
    }
}
