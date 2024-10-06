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
            Assert.That(testDataSource.DataSourceId is null);
            Assert.That(testDataSource.Name is null);
            Assert.That(testDataSource.ScriptFile is null);
            Assert.That(testDataSource.DataSource, Is.EqualTo(DataSource.Code));
            Assert.That(testDataSource.MetaData is null);

            testDataSource.DataSourceId = Guid.NewGuid().ToString();
            testDataSource.Name = "TestDataSource";
            testDataSource.ScriptFile = "Script.csx";
            testDataSource.DataSource = DataSource.Code;
            testDataSource.MetaData = new DataSourceConfiguration() { TargetTypeName = "Person" };

            Assert.That(testDataSource.DataSourceId is not null);
            Assert.That(testDataSource.Name, Is.EqualTo("TestDataSource"));
            Assert.That(testDataSource.ScriptFile, Is.EqualTo("Script.csx"));
            Assert.That(testDataSource.DataSource, Is.EqualTo(DataSource.Code));
            Assert.That(testDataSource.MetaData is not null);

        }
    }

    class CsvDataSourceConfigurationFixture
    {
        [Test]
        public void ValidateThatCsvDataSourceConfigurationCanBeInitialized()
        {
            var dataSourceConfiguration = new CsvDataSourceConfiguration();
            Assert.That(dataSourceConfiguration.TargetTypeName is null);
            Assert.That(dataSourceConfiguration.TargetFile is null);
            Assert.That(dataSourceConfiguration.Delimiter, Is.EqualTo(","));
            Assert.That(dataSourceConfiguration.HasHeader);


            dataSourceConfiguration.TargetTypeName = "Person";
            dataSourceConfiguration.TargetFile = "Persons.csv";
            dataSourceConfiguration.Delimiter = "|";
            dataSourceConfiguration.HasHeader = false;
         
       
            Assert.That(dataSourceConfiguration.TargetTypeName, Is.EqualTo("Person"));
            Assert.That(dataSourceConfiguration.TargetFile, Is.EqualTo("Persons.csv"));
            Assert.That(dataSourceConfiguration.Delimiter, Is.EqualTo("|"));
            Assert.That(dataSourceConfiguration.HasHeader == false);

        }
    }
}
