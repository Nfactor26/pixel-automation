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

            Assert.IsFalse(testDataSourceViewModel.IsInEditMode);
            Assert.IsTrue(testDataSourceViewModel.CanSelectTestDataType);
            Assert.IsNotNull(testDataSourceViewModel.TestDataSource);
            Assert.IsTrue(!string.IsNullOrEmpty(testDataSourceViewModel.ScriptFile));
            Assert.AreEqual(dataSource, testDataSourceViewModel.DataSource);
            Assert.IsNotNull(testDataSourceViewModel.MetaData);
            switch (dataSource)
            {
                case DataSource.Code:
                    Assert.IsTrue(testDataSourceViewModel.MetaData.GetType().Equals(typeof(DataSourceConfiguration)));
                    break;
                case DataSource.CsvFile:
                    Assert.IsTrue(testDataSourceViewModel.MetaData.GetType().Equals(typeof(CsvDataSourceConfiguration)));
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
                    TargetTypeName = typeof(Empty).Name
                }
            };
            var testDataSourceViewModel = new TestDataSourceViewModel(windowManager, fileSystem, codeDataSource);

            Assert.IsTrue(testDataSourceViewModel.IsInEditMode);
            Assert.IsFalse(testDataSourceViewModel.CanSelectTestDataType);
            Assert.AreSame(codeDataSource, testDataSourceViewModel.TestDataSource);           
        }

        [Test]       
        public async Task ValidateThatCanSelectDataType()
        {
            //Arrange
            windowManager.ShowDialogAsync(Arg.Any<IArgumentTypeBrowser>()).Returns(true);
            var typeDefinition = new Editor.Core.TypeDefinition(typeof(Empty));
            typeBrowser.GetCreatedType().Returns(typeDefinition);
            var testDataSourceViewModel = new TestDataSourceViewModel(windowManager, fileSystem, DataSource.Code, Enumerable.Empty<string>(), typeBrowser);

            //Act
            testDataSourceViewModel.SelectTestDataType();

            //Assert
            Assert.AreSame(typeDefinition, testDataSourceViewModel.TypeDefinition);
            Assert.AreEqual("Empty", testDataSourceViewModel.TestDataType);
            await windowManager.Received(1).ShowDialogAsync(Arg.Any<IArgumentTypeBrowser>());
            typeBrowser.Received(1).GetCreatedType();           
        }

        [Test]
        public void ValidateThatCanProcessStage()
        {
            //Arrange           
            var testDataSourceViewModel = new TestDataSourceViewModel(windowManager, fileSystem, DataSource.Code, Enumerable.Empty<string>(), typeBrowser);

            //Act
            var couldProcessStage = testDataSourceViewModel.TryProcessStage(out string errors);
            var result = testDataSourceViewModel.GetProcessedResult();

            //Assert
            Assert.IsEmpty(errors);
            Assert.IsTrue(couldProcessStage);
            Assert.AreSame(testDataSourceViewModel.TestDataSource, result);
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
            Assert.AreEqual(expected, result);
        }
    }
}
