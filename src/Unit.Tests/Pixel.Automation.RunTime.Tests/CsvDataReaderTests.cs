using NUnit.Framework;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.RunTime.DataReader;
using System;
using System.Linq;

namespace Pixel.Automation.RunTime.Tests
{
    public class CsvDataReaderTests
    {
        private CsvDataReader csvDataReader;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var metaData = new CsvDataSourceConfiguration()
            {
                TargetTypeName = "Student",
                Delimiter = ",",
                HasHeader = true,
                TargetFile = "Resources\\student.csv"
            };
            csvDataReader = new CsvDataReader();
            csvDataReader.Initialize(metaData);
        }

        [Test]
        public void ValidateThatCsvHeadersCanBeReceived()
        {
            string[] headers = csvDataReader.GetHeaders();

            Assert.That(headers.Length, Is.EqualTo(2));
            Assert.That(headers[0].Equals("Id"));
            Assert.That(headers[1].Equals("Name"));
        }

        [Test]
        public void ValidateThatNumberOfRowsCanBeRetrieved()
        {
            int rowCount = csvDataReader.GetRowCount();

            Assert.That(rowCount, Is.EqualTo(2));
        }

        [Test]
        public void ValidateThatAllRowsCanBeRetrieved()
        {
            var rows = csvDataReader.GetAllRows();

            Assert.That(rows.Count(), Is.EqualTo(2));
            Assert.That(rows.ElementAt(0)[0].Equals("1"));
            Assert.That(rows.ElementAt(0)[1].Equals("one"));
            Assert.That(rows.ElementAt(1)[0].Equals("2"));
            Assert.That(rows.ElementAt(1)[1].Equals("two"));
        }


        [Test]
        public void ValidateThatAllRowsCanBeRetrievedAsSpecifiedType()
        {
            var rows = csvDataReader.GetAllRowsAs<Student>();

            Assert.That(rows.Count(), Is.EqualTo(2));
            Assert.That(rows.ElementAt(0).Id.Equals(1));
            Assert.That(rows.ElementAt(0).Name.Equals("one"));
            Assert.That(rows.ElementAt(1).Id.Equals(2));
            Assert.That(rows.ElementAt(1).Name.Equals("two"));
        }


        [TestCase(0, "1", "one")]
        [TestCase(1, "2", "two")]
        public void ValidateThatRowCanBeRetrievedByIndex(int rowIndex, string expectedId, string expectedName)
        {
            var row = csvDataReader.GetRowData(rowIndex);

            Assert.That(row is not null);
            Assert.That(row[0].Equals(expectedId));
            Assert.That(row[1].Equals(expectedName));           
        }       

        [TestCase(0, 1, "one")]
        [TestCase(1, 2, "two")]
        public void ValidateThatRowCanBeRetrievedByIndexAsSpecifiedType(int rowIndex, int expectedId, string expectedName)
        {
            var row = csvDataReader.GetRowDataAs<Student>(rowIndex);

            Assert.That(row is not null);
            Assert.That(row.Id.Equals(expectedId));
            Assert.That(row.Name.Equals(expectedName));
        }

        [Test]
        public void ValidateThatExceptionIsThrowIfRowDoesnotExist()
        {
            Assert.Throws<IndexOutOfRangeException>(() => { csvDataReader.GetRowData(5); });
            Assert.Throws<IndexOutOfRangeException>(() => { csvDataReader.GetRowDataAs<Student>(5); });
        }

        [TestCase("csv", true)]
        [TestCase("xls", false)]
        public void ValidateThatCsvDataReaderCanBeQueriedForWhetherItCanProcessAParticularFileExtension(string fileExtension, bool expectedResult)
        {
            var canProcessFileType = csvDataReader.CanProcessFileType(fileExtension);
            Assert.That(canProcessFileType, Is.EqualTo(expectedResult));
        }

        public class Student
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
