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
        public string DataSourceId { get; set; }

        /// <summary>
        /// Name of the data source
        /// </summary>
        [DataMember(IsRequired = true, Order = 20)]
        public string Name { get; set; }

        /// <summary>
        /// Script file that is executed to retrieve the data.
        /// </summary>
        [DataMember(IsRequired = true, Order = 30)]
        public string ScriptFile { get; set; }

        /// <summary>
        /// Identifies the source of the data e.g. code based, csv based
        /// </summary>
        [DataMember(IsRequired = true, Order = 40)]
        public DataSource DataSource { get; set; } = DataSource.Code;

        /// <summary>
        /// Metadata based on the selected DataSource
        /// </summary>
        [DataMember(IsRequired = true, Order = 50)]
        public DataSourceConfiguration MetaData { get; set; }


        /// <summary>
        /// Indicates if the TestDataSource is deleted. Deleted data sources are not loaded in explorer.
        /// </summary>
        [DataMember(IsRequired = false, Order = 1000)]
        public bool IsDeleted { get; set; }
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
        [DataMember(IsRequired = true, Order = 10)]
        public string TargetTypeName { get; set; }

    }

    /// <summary>
    /// Configuration details for the Csv based <see cref="TestDataSource"/>
    /// </summary>
    [DataContract]
    [Serializable]
    public class CsvDataSourceConfiguration : DataSourceConfiguration
    {
        [DataMember(IsRequired = true, Order = 20)]
        public bool HasHeader { get; set; } = true;

        [DataMember(IsRequired = true, Order = 30)]
        public string Delimiter { get; set; } = ",";

        [DataMember(IsRequired = true, Order = 40)]
        public string TargetFile { get; set; }
         
    }

    public enum DataSource
    {
        Code,
        CsvFile
    }
}
