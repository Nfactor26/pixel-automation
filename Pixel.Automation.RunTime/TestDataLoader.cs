using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Core.TestData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pixel.Automation.RunTime
{
    public class TestDataLoader : ITestDataLoader
    {
        private readonly IProjectFileSystem fileSystem;
        private readonly ISerializer serializer;
        private readonly IDataReader[] dataReaders;
        private readonly IScriptEngine scriptEngine;

        public TestDataLoader(ISerializer serializer, IProjectFileSystem fileSystem, IScriptEngine scriptEngine, IDataReader[] dataReaders)
        {
            Guard.Argument(serializer).NotNull();
            Guard.Argument(fileSystem).NotNull();
            Guard.Argument(scriptEngine).NotNull();
            Guard.Argument(dataReaders).NotNull();

            this.serializer = serializer;
            this.fileSystem = fileSystem;
            this.scriptEngine = scriptEngine;
            this.dataReaders = dataReaders;
        }

        public IEnumerable<object> GetTestCaseData(TestCase testCase)
        {
            Guard.Argument(testCase).NotNull();
            Guard.Argument(testCase.TestDataId).NotNull().NotEmpty();

            string repositoryFolder = this.fileSystem.TestDataRepository;
            string dataSourceFile = Path.Combine(repositoryFolder, $"{testCase.TestDataId}.dat");
            if (!File.Exists(dataSourceFile))
            {
                throw new FileNotFoundException($"{dataSourceFile} doesn't exist. Can't load required data for test case");
            }

            var dataSource = serializer.Deserialize<TestDataSource>(dataSourceFile);

            switch (dataSource.DataSource)
            {
                case DataSource.Code:
                    return GetCodedTestData(dataSource).Result;
                case DataSource.CsvFile:
                    return GetCsvTestData(dataSource).Result;
                default:
                    throw new ArgumentException($"DataSource {dataSource.DataSource} isn't supported");
            }
        }

        private async Task<IEnumerable<object>> GetCodedTestData(TestDataSource testDataSource)
        {
            ScriptResult result = await scriptEngine.ExecuteFileAsync($"{testDataSource.Id}.csx");          
            result = await scriptEngine.ExecuteScriptAsync("GetDataRows()");
            return (result.ReturnValue as IEnumerable<object>) ?? throw new InvalidCastException("Data source should return IEnumerable data");
        }

        private async  Task<IEnumerable<object>> GetCsvTestData(TestDataSource testDataSource)
        {        

            IDataReader csvDataReader = dataReaders.FirstOrDefault(r => r.CanProcessFileType("csv"));
            if (csvDataReader == null)
            {
                throw new Exception("No DataReader for csv file is available");
            }
            //csvDataReader.Initialize(testDataSource.MetaData);

            object globals = new DataReaderScriptGlobal() { DataReaderArgument = csvDataReader, DataSourceArgument = testDataSource };
            this.scriptEngine.SetGlobals(globals);
            ScriptResult result = await scriptEngine.ExecuteFileAsync($"{testDataSource.Id}.csx");
            result = await scriptEngine.ExecuteScriptAsync("GetDataRows(DataSourceArgument, DataReaderArgument)");
            return (result.ReturnValue as IEnumerable<object>) ?? throw new InvalidCastException("Data source should return IEnumerable data");
        }

        public class DataReaderScriptGlobal
        {
            public IDataReader DataReaderArgument { get; set; }

            public TestDataSource DataSourceArgument { get; set; }
        }
    }
}
