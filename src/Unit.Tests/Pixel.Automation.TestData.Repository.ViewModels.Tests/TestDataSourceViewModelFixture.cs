using Caliburn.Micro;
using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.TestData.Repository.ViewModels.Tests
{
    /// <summary>
    /// Test Fixture for <see cref="TestDataSourceViewModel"/>
    /// </summary>
    [TestFixture]
    public class TestDataSourceViewModelFixture
    {

        private IWindowManager windowManager;
        private IProjectFileSystem fileSystem;
        private IArgumentTypeBrowser typeBrowser;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            windowManager = Substitute.For<IWindowManager>();
            fileSystem = Substitute.For<IProjectFileSystem>();
            typeBrowser = Substitute.For<IArgumentTypeBrowser>();

            fileSystem.WorkingDirectory.Returns(Environment.CurrentDirectory);
            fileSystem.TestDataRepository.Returns(Path.Combine(Environment.CurrentDirectory, "TestDataRepository"));

            windowManager.ShowDialogAsync(Arg.Any<IArgumentTypeBrowser>()).Returns(true);
          
            var typeDefinition = new Editor.Core.TypeDefinition(typeof(EmptyModel));
            typeBrowser.GetCreatedType().Returns(typeDefinition);
        }

        [TearDown]
        public void TearDown()
        {
            windowManager.ClearReceivedCalls();
            fileSystem.ClearReceivedCalls();
            typeBrowser.ClearReceivedCalls();
        }

        [TestCase(DataSource.Code)]
        [TestCase(DataSource.CsvFile)]
        public void ValidateThatTestDataSourceViewModelCanBeCorrectlyInitialized(DataSource dataSource)
        {
            var testDataSourceViewModel = new TestDataSourceViewModel(windowManager, fileSystem, dataSource, Enumerable.Empty<string>(), typeBrowser);

            Assert.That(testDataSourceViewModel.IsInEditMode == false);
            Assert.That(testDataSourceViewModel.CanSelectTestDataType);
            Assert.That(testDataSourceViewModel.TestDataSource is not null);
            Assert.That(!string.IsNullOrEmpty(testDataSourceViewModel.ScriptFile));
            Assert.That(testDataSourceViewModel.DataSource, Is.EqualTo(dataSource));
            Assert.That(testDataSourceViewModel.MetaData is not null);
            switch (dataSource)
            {
                case DataSource.Code:
                    Assert.That(testDataSourceViewModel.MetaData.GetType().Equals(typeof(DataSourceConfiguration)));
                    break;
                case DataSource.CsvFile:
                    Assert.That(testDataSourceViewModel.MetaData.GetType().Equals(typeof(CsvDataSourceConfiguration)));
                    break;
            }    
        }

        [Test]      
        public void ValidateThatTestDataSourceViewModelCanBeCorrectlyInitializedForEditMode()
        {
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
            var testDataSourceViewModel = new TestDataSourceViewModel(windowManager, fileSystem, codeDataSource);

            Assert.That(testDataSourceViewModel.IsInEditMode);
            Assert.That(testDataSourceViewModel.CanSelectTestDataType == false);
            Assert.That(testDataSourceViewModel.TestDataSource, Is.SameAs(codeDataSource));           
        }

        [Test]       
        public async Task ValidateThatCanSelectDataType()
        {
            //Arrange           
            var testDataSourceViewModel = new TestDataSourceViewModel(windowManager, fileSystem, DataSource.Code, 
                Enumerable.Empty<string>(), typeBrowser);

            //Act
            testDataSourceViewModel.SelectTestDataType();

            //Assert
            var result = testDataSourceViewModel.GetProcessedResult() as TestDataSourceResult;
            Assert.That(result.DataSourceType, Is.EqualTo(typeof(EmptyModel)));
            Assert.That(testDataSourceViewModel.TestDataType, Is.EqualTo(nameof(EmptyModel)));
        
            await windowManager.Received(1).ShowDialogAsync(Arg.Any<IArgumentTypeBrowser>());
            typeBrowser.Received(1).GetCreatedType();           
        }

        [Test]
        public async Task ValidateThatCanProcessStage()
        {
            //Arrange           
            var testDataSourceViewModel = new TestDataSourceViewModel(windowManager, fileSystem, DataSource.Code, Enumerable.Empty<string>(), typeBrowser);
            testDataSourceViewModel.SelectTestDataType();
           
            //Act
            var couldProcessStage = await testDataSourceViewModel.TryProcessStage();
            var result = testDataSourceViewModel.GetProcessedResult() as TestDataSourceResult;

            //Assert          
            Assert.That(couldProcessStage);
            Assert.That(result.TestDataSource, Is.SameAs(testDataSourceViewModel.TestDataSource));
            Assert.That(result.DataSourceType, Is.EqualTo(typeof(EmptyModel)));
        }

        [TestCase(DataSource.Code, "EmptyDataSource", "Empty", "", "", true)]
        [TestCase(DataSource.Code, "NonEmptyDataSource", "Empty", "", "", false)]
        [TestCase(DataSource.Code, "EmptyDataSource", "", "", "", false)]
        [TestCase(DataSource.Code, "", "Empty", "", "", false)]
        [TestCase(DataSource.Code, "", "", "", "", false)]
        [TestCase(DataSource.CsvFile, "EmptyDataSource", "Empty", ",", "Empty.csv", true)]
        [TestCase(DataSource.CsvFile, "EmptyDataSource", "Empty", ",;", "Empty.csv", false)]
        [TestCase(DataSource.CsvFile, "EmptyDataSource", "Empty", "", "Empty.csv", false)]
        [TestCase(DataSource.CsvFile, "EmptyDataSource", "Empty", ",", "", false)]
        public void ValidateThatModelIsCorrectlyValidated(DataSource dataSource, string name, string testDataType, string delimiter, string dataFileName, bool expected)
        {
            //Arrange           
            var testDataSourceViewModel = new TestDataSourceViewModel(windowManager, fileSystem, dataSource, new[] { "NonEmptyDataSource" }, typeBrowser);
            testDataSourceViewModel.Name = name;
            testDataSourceViewModel.TestDataType = testDataType;
            testDataSourceViewModel.Delimiter = delimiter;
            testDataSourceViewModel.DataFileName = dataFileName;

            //Act
            bool result = testDataSourceViewModel.Validate();

            //Assert
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
