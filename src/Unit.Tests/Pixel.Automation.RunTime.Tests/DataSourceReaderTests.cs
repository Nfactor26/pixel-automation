using NSubstitute;
using NUnit.Framework;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Core.TestData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pixel.Automation.RunTime.Tests
{
    public class DataSourceReaderTests
    {      
      

        [Test]
        public void ValidateThatCodedDataSourceCanBeLoaded()
        {
            var testDataSource = new TestDataSource() { DataSource = DataSource.Code };

            var fileSystem = Substitute.For<IProjectFileSystem>();
            fileSystem.TestCaseRepository.Returns(Environment.CurrentDirectory);
            fileSystem.Exists(Arg.Any<string>()).Returns(true);

            var serializer = Substitute.For<ISerializer>();
            serializer.Deserialize<TestDataSource>(Arg.Any<string>()).Returns(testDataSource);

            var scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.ExecuteFileAsync(Arg.Any<string>()).Returns(new ScriptResult());
            scriptEngine.ExecuteScriptAsync(Arg.Is<string>("GetDataRows()")).Returns(new ScriptResult(new List<string>() { "Hello", "World" }));

            var csvDataReader = Substitute.For<IDataReader>();

            var scriptEngineFactory = Substitute.For<IScriptEngineFactory>();
            scriptEngineFactory.CreateScriptEngine(Arg.Any<string>()).Returns(scriptEngine);

            var dataSourceReader = new DataSourceReader(serializer, fileSystem, scriptEngineFactory, new []{ csvDataReader });

            var dataRows = dataSourceReader.LoadData("DataSourceId");

            Assert.AreEqual(2, dataRows.Count());

            fileSystem.Received(1).Exists(Arg.Any<string>());
            scriptEngine.Received(1).ExecuteFileAsync(Arg.Any<string>());
            scriptEngine.Received(1).ExecuteScriptAsync(Arg.Is<string>("GetDataRows()"));

        }



        [Test]
        public void ValidateThatCsvDataSourceCanBeLoaded()
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
            fileSystem.TestCaseRepository.Returns(Environment.CurrentDirectory);
            fileSystem.TestDataRepository.Returns(Environment.CurrentDirectory);
            fileSystem.Exists(Arg.Any<string>()).Returns(true);

            var serializer = Substitute.For<ISerializer>();
            serializer.Deserialize<TestDataSource>(Arg.Any<string>()).Returns(testDataSource);

            var scriptEngine = Substitute.For<IScriptEngine>();
            scriptEngine.When(x => { x.SetGlobals(Arg.Any<object>()); }).Do((x) => { });
            scriptEngine.ExecuteFileAsync(Arg.Any<string>()).Returns(new ScriptResult());
            scriptEngine.ExecuteScriptAsync(Arg.Is<string>("GetDataRows(DataSourceArgument, DataReaderArgument)")).Returns(new ScriptResult(new List<string>() { "Hello", "World" }));

            var csvDataReader = Substitute.For<IDataReader>();
            csvDataReader.CanProcessFileType("csv").Returns(true);


            var scriptEngineFactory = Substitute.For<IScriptEngineFactory>();
            scriptEngineFactory.CreateScriptEngine(Arg.Any<string>()).Returns(scriptEngine);

            var dataSourceReader = new DataSourceReader(serializer, fileSystem, scriptEngineFactory, new[] { csvDataReader });

            var dataRows = dataSourceReader.LoadData("DataSourceId");

            Assert.AreEqual(2, dataRows.Count());

            fileSystem.Received(1).Exists(Arg.Any<string>());
            csvDataReader.Received(1).CanProcessFileType("csv");
            scriptEngine.Received(1).SetGlobals(Arg.Any<object>());
            scriptEngine.Received(1).ExecuteFileAsync(Arg.Any<string>());
            scriptEngine.Received(1).ExecuteScriptAsync(Arg.Is<string>("GetDataRows(DataSourceArgument, DataReaderArgument)"));

        }
    }
}