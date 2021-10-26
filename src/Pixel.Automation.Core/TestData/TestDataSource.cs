﻿using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.TestData
{
    /// <summary>
    /// A test case requires a data source.
    /// TestDataSource captures the details of data source for the test.
    /// </summary>
    [DataContract]
    [Serializable]
    public class TestDataSource 
    {
        /// <summary>
        /// Identifier of the data source
        /// </summary>
        [DataMember(IsRequired = true, Order = 10)]
        public string Id { get; set; }

        /// <summary>
        /// Name of the data source
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Script file that is executed to retrieve the data.
        /// </summary>
        [DataMember]
        public string ScriptFile { get; set; }

        /// <summary>
        /// Identifies the source of the data e.g. code based, csv based
        /// </summary>
        [DataMember]
        public DataSource DataSource { get; set; } = DataSource.Code;

        /// <summary>
        /// Metadata based on the selected DataSource
        /// </summary>
        [DataMember]
        public DataSourceConfiguration MetaData { get; set; }
    }

    /// <summary>
    /// Configuration details for the <see cref="TestDataSource"/>
    /// </summary>
    [DataContract]
    [Serializable]
    public class DataSourceConfiguration
    {
        /// <summary>
        /// Name of the data type returned by data source
        /// </summary>
        [DataMember]
        public string TargetTypeName { get; set; }

    }

    /// <summary>
    /// Configuration details for the Csv based <see cref="TestDataSource"/>
    /// </summary>
    [DataContract]
    [Serializable]
    public class CsvDataSourceConfiguration : DataSourceConfiguration
    {
        [DataMember]
        public string TargetFile { get; set; }

        [DataMember]        
        public string Delimiter { get; set; } = ",";

        [DataMember]
        public bool HasHeaders { get; set; } = true;
    }

    public enum DataSource
    {
        Code,
        CsvFile
    }
}
