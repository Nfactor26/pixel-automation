﻿using Dawn;
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
    /// <inheritdoc/>
    public class DataSourceReader : IDataSourceReader
    {
        private readonly IProjectFileSystem fileSystem;
        private readonly ISerializer serializer;
        private readonly IDataReader[] dataReaders;
        private readonly IScriptEngineFactory scriptEngineFactory;
        private Lazy<IScriptEngine> codeScriptEngine;
        private Lazy<IScriptEngine> csvScriptEngine;

        public DataSourceReader(ISerializer serializer, IProjectFileSystem fileSystem, IScriptEngineFactory scriptEngineFactory, IDataReader[] dataReaders)
        {
            Guard.Argument(serializer).NotNull();
            Guard.Argument(fileSystem).NotNull();
            Guard.Argument(scriptEngineFactory).NotNull();
            Guard.Argument(dataReaders).NotNull();

            this.serializer = serializer;
            this.fileSystem = fileSystem;
            this.scriptEngineFactory = scriptEngineFactory;
            this.dataReaders = dataReaders;

            this.codeScriptEngine = new Lazy<IScriptEngine>(() => { return this.scriptEngineFactory.CreateScriptEngine(fileSystem.WorkingDirectory); });
            this.csvScriptEngine = new Lazy<IScriptEngine>(() => { return this.scriptEngineFactory.CreateScriptEngine(fileSystem.WorkingDirectory); });

        }

        public IEnumerable<object> LoadData(string dataSourceId)
        {            
            Guard.Argument(dataSourceId).NotNull().NotEmpty();          

            string dataSourceFile = Path.Combine(this.fileSystem.TestDataRepository, $"{dataSourceId}.dat");
            if (!this.fileSystem.Exists(dataSourceFile))
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
            ScriptResult result = await codeScriptEngine.Value.ExecuteFileAsync(testDataSource.ScriptFile);          
            result = await codeScriptEngine.Value.ExecuteScriptAsync("GetDataRows()");
            return (result.ReturnValue as IEnumerable<object>) ?? throw new InvalidCastException("Data source should return IEnumerable data");
        }

        private async  Task<IEnumerable<object>> GetCsvTestData(TestDataSource testDataSource)
        {        

            IDataReader csvDataReader = dataReaders.FirstOrDefault(r => r.CanProcessFileType("csv"));
            if (csvDataReader == null)
            {
                throw new Exception("No DataReader for csv file is available");
            }          

            object globals = new DataReaderScriptGlobal() { DataReaderArgument = csvDataReader, DataSourceArgument = testDataSource };
            this.codeScriptEngine.Value.SetGlobals(globals);
            ScriptResult result = await this.csvScriptEngine.Value.ExecuteFileAsync(testDataSource.ScriptFile);
            result = await this.csvScriptEngine.Value.ExecuteScriptAsync("GetDataRows(DataSourceArgument, DataReaderArgument)");
            return (result.ReturnValue as IEnumerable<object>) ?? throw new InvalidCastException("Data source should return IEnumerable data");
        }

        public class DataReaderScriptGlobal
        {
            public IDataReader DataReaderArgument { get; set; }

            public TestDataSource DataSourceArgument { get; set; }
        }
    }
}