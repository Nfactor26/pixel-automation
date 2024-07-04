using NUnit.Framework;
using Pixel.Automation.Core.TestData;
using System;

namespace Pixel.Automation.Core.Tests.TestData
{
    class TestDataSourceFixture
    {
        [Test]
        public void ValidateThatTestDataSourceCanBeInitialized()
        {
            var testDataSource = new TestDataSource();
            Assert.IsNull(testDataSource.DataSourceId);
            Assert.IsNull(testDataSource.Name);
            Assert.IsNull(testDataSource.ScriptFile);
            Assert.AreEqual(DataSource.Code, testDataSource.DataSource);
            Assert.IsNull(testDataSource.MetaData);

            testDataSource.DataSourceId = Guid.NewGuid().ToString();
            testDataSource.Name = "TestDataSource";
            testDataSource.ScriptFile = "Script.csx";
            testDataSource.DataSource = DataSource.Code;
            testDataSource.MetaData = new DataSourceConfiguration() { TargetTypeName = "Person" };

            Assert.IsNotNull(testDataSource.DataSourceId);
            Assert.AreEqual("TestDataSource", testDataSource.Name);
            Assert.AreEqual("Script.csx", testDataSource.ScriptFile);
            Assert.AreEqual(DataSource.Code, testDataSource.DataSource);
            Assert.IsNotNull(testDataSource.MetaData);

        }
    }

    class CsvDataSourceConfigurationFixture
    {
        [Test]
        public void ValidateThatCsvDataSourceConfigurationCanBeInitialized()
        {
            var dataSourceConfiguration = new CsvDataSourceConfiguration();
            Assert.IsNull(dataSourceConfiguration.TargetTypeName);
            Assert.IsNull(dataSourceConfiguration.TargetFile);
            Assert.AreEqual(",", dataSourceConfiguration.Delimiter);
            Assert.IsTrue(dataSourceConfiguration.HasHeader);


            dataSourceConfiguration.TargetTypeName = "Person";
            dataSourceConfiguration.TargetFile = "Persons.csv";
            dataSourceConfiguration.Delimiter = "|";
            dataSourceConfiguration.HasHeader = false;
         
       
            Assert.AreEqual("Person", dataSourceConfiguration.TargetTypeName);
            Assert.AreEqual("Persons.csv", dataSourceConfiguration.TargetFile);
            Assert.AreEqual("|", dataSourceConfiguration.Delimiter);
            Assert.IsFalse(dataSourceConfiguration.HasHeader);

        }
    }
}
