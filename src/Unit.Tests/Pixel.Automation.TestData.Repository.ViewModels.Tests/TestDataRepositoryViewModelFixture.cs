using Caliburn.Micro;
using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Core.Notfications;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Automation.TestData.Repository.ViewModels.Tests
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
        private IEventAggregator eventAggregator;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            serializer = Substitute.For<ISerializer>();
            projectFileSystem = Substitute.For<IProjectFileSystem>();
            scriptEditorFactory = Substitute.For<IScriptEditorFactory>();
            typeBrowserFactory = Substitute.For<IArgumentTypeBrowserFactory>();
            windowManager = Substitute.For<IWindowManager>();
            eventAggregator = Substitute.For<IEventAggregator>();


            var codeDataSource = new TestDataSource()
            {
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
                DataSource = DataSource.CsvFile,
                Name = "CodedDataSource",
                ScriptFile = "CsvDataSource.csx",
                MetaData = new CsvDataSourceConfiguration()
                {
                    TargetTypeName = typeof(EmptyModel).Name,
                    TargetFile = "Empty.csv"
                }
            };

            projectFileSystem.WorkingDirectory.Returns(Environment.CurrentDirectory);
            projectFileSystem.TestDataRepository.Returns(Path.Combine(Environment.CurrentDirectory, "TestDataRepository"));
            projectFileSystem.GetTestDataSources().Returns(new[] { codeDataSource, csvDataSource });

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
            scriptEditorFactory.ClearReceivedCalls();
            typeBrowserFactory.ClearReceivedCalls();
            windowManager.ClearReceivedCalls();
        }

        [TestCase]
        public async Task ValidateThatTestDataRepositoryViewModelCanBeCorrectlyInitialized()
        {
            var testDataRepositoryViewModel = new TestDataRepositoryViewModel(serializer, projectFileSystem, scriptEditorFactory, windowManager, eventAggregator, typeBrowserFactory);

            Assert.AreEqual(0, testDataRepositoryViewModel.TestDataSourceCollection.Count);
            Assert.IsNull(testDataRepositoryViewModel.SelectedTestDataSource);
            Assert.IsTrue(string.IsNullOrEmpty(testDataRepositoryViewModel.FilterText));

            //activating view model will call OnInitializeAsync handler which will load TestDataSources from local storage
            await testDataRepositoryViewModel.ActivateAsync();

            Assert.AreEqual(2, testDataRepositoryViewModel.TestDataSourceCollection.Count);
        }

        [TestCase]
        public async Task ValidateThatCanCreateNewCodeDataSource()
        {
            var testDataRepositoryViewModel = new TestDataRepositoryViewModel(serializer, projectFileSystem, scriptEditorFactory, windowManager, eventAggregator, typeBrowserFactory);
            await testDataRepositoryViewModel.ActivateAsync();

            testDataRepositoryViewModel.CreateCodedTestDataSource();

            Assert.AreEqual(3, testDataRepositoryViewModel.TestDataSourceCollection.Count);
            serializer.Received(1).Serialize<TestDataSource>(Arg.Any<string>(), Arg.Any<TestDataSource>());
            await windowManager.Received(1).ShowDialogAsync(Arg.Any<TestDataSourceBuilderViewModel>());
        }

        [TestCase]
        public async Task ValidateThatCanCreateNewCsvDataSource()
        {
            var testDataRepositoryViewModel = new TestDataRepositoryViewModel(serializer, projectFileSystem, scriptEditorFactory, windowManager, eventAggregator, typeBrowserFactory);
            await testDataRepositoryViewModel.ActivateAsync();

            testDataRepositoryViewModel.CreateCsvTestDataSource();

            Assert.AreEqual(3, testDataRepositoryViewModel.TestDataSourceCollection.Count);
            serializer.Received(1).Serialize<TestDataSource>(Arg.Any<string>(), Arg.Any<TestDataSource>());
            await windowManager.Received(1).ShowDialogAsync(Arg.Any<TestDataSourceBuilderViewModel>());
        }

        [TestCase]
        public async Task ValidateThatCanEditCodedDataSource()
        {
            //Arrange
            var scriptEditor = Substitute.For<IScriptEditorScreen>();
            scriptEditor.When(x => x.OpenDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Is<string>(string.Empty))).Do(x => { });
            scriptEditorFactory.CreateScriptEditor().Returns(scriptEditor);
            scriptEditorFactory.When(x => x.AddProject(Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<Type>())).Do(XamlGeneratedNamespace => { });
            scriptEditorFactory.When(x => x.RemoveProject(Arg.Any<string>())).Do(x => { });

            var testDataRepositoryViewModel = new TestDataRepositoryViewModel(serializer, projectFileSystem, scriptEditorFactory, windowManager, eventAggregator, typeBrowserFactory);
            await testDataRepositoryViewModel.ActivateAsync();
            var codeDataSource = testDataRepositoryViewModel.TestDataSourceCollection.First(a => a.DataSource.Equals(DataSource.Code));

            //Act
            testDataRepositoryViewModel.EditDataSource(codeDataSource);

            //Assert
            serializer.Received(0).Serialize<TestDataSource>(Arg.Any<string>(), Arg.Any<TestDataSource>());
            scriptEditorFactory.Received(1).CreateScriptEditor();
            scriptEditorFactory.Received(1).AddProject(Arg.Any<string>(), Arg.Any<string[]>(), Arg.Any<Type>());
            scriptEditor.Received(1).OpenDocument(Arg.Any<string>(), Arg.Any<string>(), Arg.Is<string>(string.Empty));
            await windowManager.Received(1).ShowDialogAsync(Arg.Any<IScriptEditorScreen>());
        }

        [TestCase]
        public async Task ValidateThatCanEditCsvDataSource()
        {
            //Arrange
            var testDataRepositoryViewModel = new TestDataRepositoryViewModel(serializer, projectFileSystem, scriptEditorFactory, windowManager, eventAggregator, typeBrowserFactory);
            await testDataRepositoryViewModel.ActivateAsync();
            var csvDataSource = testDataRepositoryViewModel.TestDataSourceCollection.First(a => a.DataSource.Equals(DataSource.CsvFile));

            //Act
            testDataRepositoryViewModel.EditDataSource(csvDataSource);

            //Assert
            serializer.Received(1).Serialize<TestDataSource>(Arg.Any<string>(), Arg.Any<TestDataSource>());
            await windowManager.Received(1).ShowDialogAsync(Arg.Any<TestDataSourceBuilderViewModel>());
        }

        [TestCase]
        public async Task ValidateThatTestDataSourceIsClearedWhenDeactivated()
        {
            //Arrange
            var testDataRepositoryViewModel = new TestDataRepositoryViewModel(serializer, projectFileSystem, scriptEditorFactory, windowManager, eventAggregator, typeBrowserFactory);

            await testDataRepositoryViewModel.ActivateAsync();
            Assert.AreEqual(2, testDataRepositoryViewModel.TestDataSourceCollection.Count);

            await testDataRepositoryViewModel.DeactivateAsync(true);
            Assert.AreEqual(0, testDataRepositoryViewModel.TestDataSourceCollection.Count);
        }

        [TestCase]
        public async Task ValidateThatCanHandleShowTestDataSourceNotificationMessage()
        {
            //Arrange
            var testDataRepositoryViewModel = new TestDataRepositoryViewModel(serializer, projectFileSystem, scriptEditorFactory, windowManager, eventAggregator, typeBrowserFactory);

            //Act
            await testDataRepositoryViewModel.HandleAsync(new ShowTestDataSourceNotification("test-data-id"), CancellationToken.None);

            //Assert
            Assert.AreEqual("test-data-id", testDataRepositoryViewModel.FilterText);

        }
    }
}
