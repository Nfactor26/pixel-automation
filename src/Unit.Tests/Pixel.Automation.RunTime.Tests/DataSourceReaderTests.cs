using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Core.TestData;
using System.Collections.Generic;
using System.Linq;

namespace Pixel.Automation.RunTime.Tests
{
    public class DataSourceReaderTests
    {    

        [TestCase("", false)]
        [TestCase("custom", true)]
        [TestCase("custom", false)]
        public void ValidateThatCodedDataSourceCanBeLoaded(string dataSourceSuffix, bool suffixedFileExists)
        {
            var testDataSource = new TestDataSource() { DataSource = DataSource.Code, ScriptFile = "Data.csx" };

            var fileSystem = Substitute.For<IProjectFileSystem>();
            fileSystem.TestCaseRepository.Returns(string.Empty);
            fileSystem.Exists("DataSourceId.dat").Returns(true);
            fileSystem.Exists("Data.csx").Returns(true);
            fileSystem.Exists($"Data.{dataSourceSuffix}.csx").Returns(suffixedFileExists);

            var serializer = Substitute.For<ISerializer>();
            serializer.Deserialize<TestDataSource>(Arg.Any<string>()).Returns(testDataSource);

            var scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.ExecuteFileAsync(Arg.Any<string>()).Returns(new ScriptResult());
            scriptEngine.ExecuteScriptAsync(Arg.Is<string>("GetDataRows()")).Returns(new ScriptResult(new List<EmptyModel>() { new EmptyModel(), new EmptyModel() }));

            var csvDataReader = Substitute.For<IDataReader>();

            var scriptEngineFactory = Substitute.For<IScriptEngineFactory>();
            scriptEngineFactory.CreateScriptEngine(Arg.Any<string>()).Returns(scriptEngine);

            var dataSourceReader = new DataSourceReader(serializer, fileSystem, scriptEngineFactory, new []{ csvDataReader });
            dataSourceReader.SetDataSourceSuffix(dataSourceSuffix);

            var dataRows = dataSourceReader.LoadData("DataSourceId");

            Assert.AreEqual(2, dataRows.Count());

            fileSystem.Received(1).Exists("DataSourceId.dat");
            if(!string.IsNullOrEmpty(dataSourceSuffix))
            {
                fileSystem.Received(1).Exists($"Data.{dataSourceSuffix}.csx");
            }
            scriptEngine.Received(1).ExecuteFileAsync(Arg.Any<string>());
            scriptEngine.Received(1).ExecuteScriptAsync(Arg.Is<string>("GetDataRows()"));

        }


        [TestCase("", false)]
        [TestCase("custom", true)]
        [TestCase("custom", false)]
        public void ValidateThatCsvDataSourceCanBeLoaded(string dataSourceSuffix, bool suffixedFileExists)
        {
            var testDataSource = new TestDataSource() 
            { 
                DataSource = DataSource.CsvFile,
                MetaData = new CsvDataSourceConfiguration()
                {
                    TargetFile = "CsvFile.csv"
                }
            };

            var fileSystem = Substitute.For<IProjectFileSystem>();
            fileSystem.TestCaseRepository.Returns(string.Empty);
            fileSystem.TestDataRepository.Returns(string.Empty);
            fileSystem.Exists("DataSourceId.dat").Returns(true);
            fileSystem.Exists("CsvFile.csv").Returns(true);
            fileSystem.Exists($"CsvFile.{dataSourceSuffix}.csx").Returns(suffixedFileExists);

            var serializer = Substitute.For<ISerializer>();
            serializer.Deserialize<TestDataSource>(Arg.Any<string>()).Returns(testDataSource);

            var scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.When(x => { x.SetGlobals(Arg.Any<object>()); }).Do((x) => { });
            scriptEngine.ExecuteFileAsync(Arg.Any<string>()).Returns(new ScriptResult());
            scriptEngine.ExecuteScriptAsync(Arg.Is<string>("GetDataRows(DataSourceArgument, DataReaderArgument)")).Returns(new ScriptResult(new List<EmptyModel>() { new EmptyModel(), new EmptyModel() }));

            var csvDataReader = Substitute.For<IDataReader>();
            csvDataReader.CanProcessFileType("csv").Returns(true);


            var scriptEngineFactory = Substitute.For<IScriptEngineFactory>();
            scriptEngineFactory.CreateScriptEngine(Arg.Any<string>()).Returns(scriptEngine);

            var dataSourceReader = new DataSourceReader(serializer, fileSystem, scriptEngineFactory, new[] { csvDataReader });
            dataSourceReader.SetDataSourceSuffix(dataSourceSuffix);

            var dataRows = dataSourceReader.LoadData("DataSourceId");

            Assert.AreEqual(2, dataRows.Count());

            fileSystem.Received(1).Exists("DataSourceId.dat");
            if (!string.IsNullOrEmpty(dataSourceSuffix))
            {
                fileSystem.Received(1).Exists($"CsvFile.{dataSourceSuffix}.csv");
            }
            csvDataReader.Received(1).CanProcessFileType("csv");
            scriptEngine.Received(1).SetGlobals(Arg.Any<object>());
            scriptEngine.Received(1).ExecuteFileAsync(Arg.Any<string>());
            scriptEngine.Received(1).ExecuteScriptAsync(Arg.Is<string>("GetDataRows(DataSourceArgument, DataReaderArgument)"));

        }
    }
}