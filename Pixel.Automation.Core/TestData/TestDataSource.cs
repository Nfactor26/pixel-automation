using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.TestData
{
    [DataContract]
    [Serializable]
    public class TestDataSource 
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public DataSource DataSource { get; set; } = DataSource.Code;

        [DataMember]
        public DataSourceConfiguration MetaData { get; set; }
    }

    [DataContract]
    [Serializable]
    public class DataSourceConfiguration
    {
        [DataMember]
        public string TargetTypeName { get; set; }

    }

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
