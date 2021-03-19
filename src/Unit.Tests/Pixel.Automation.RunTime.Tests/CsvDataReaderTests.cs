using NUnit.Framework;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.RunTime.DataReader;
using System;
using System.Linq;

namespace Pixel.Automation.RunTime.Tests
{
    class CsvDataReaderTests
    {
        private CsvDataReader csvDataReader;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var metaData = new CsvDataSourceConfiguration()
            {
                TargetTypeName = "Student",
                Delimiter = ",",
                HasHeaders = true,
                TargetFile = "Resources\\student.csv"
            };
            csvDataReader = new CsvDataReader();
            csvDataReader.Initialize(metaData);
        }

        [Test]
        public void ValidateThatCsvHeadersCanBeReceived()
        {
            string[] headers = csvDataReader.GetHeaders();

            Assert.AreEqual(2, headers.Length);
            Assert.IsTrue(headers[0].Equals("Id"));
            Assert.IsTrue(headers[1].Equals("Name"));
        }

        [Test]
        public void ValidateThatNumberOfRowsCanBeRetrieved()
        {
            int rowCount = csvDataReader.GetRowCount();

            Assert.AreEqual(2, rowCount);
        }

        [Test]
        public void ValidateThatAllRowsCanBeRetrieved()
        {
            var rows = csvDataReader.GetAllRows();

            Assert.AreEqual(2, rows.Count());
            Assert.IsTrue(rows.ElementAt(0)[0].Equals("1"));
            Assert.IsTrue(rows.ElementAt(0)[1].Equals("one"));
            Assert.IsTrue(rows.ElementAt(1)[0].Equals("2"));
            Assert.IsTrue(rows.ElementAt(1)[1].Equals("two"));
        }


        [Test]
        public void ValidateThatAllRowsCanBeRetrievedAsSpecifiedType()
        {
            var rows = csvDataReader.GetAllRowsAs<Student>();

            Assert.AreEqual(2, rows.Count());
            Assert.IsTrue(rows.ElementAt(0).Id.Equals(1));
            Assert.IsTrue(rows.ElementAt(0).Name.Equals("one"));
            Assert.IsTrue(rows.ElementAt(1).Id.Equals(2));
            Assert.IsTrue(rows.ElementAt(1).Name.Equals("two"));
        }


        [TestCase(0, "1", "one")]
        [TestCase(1, "2", "two")]
        public void ValidateThatRowCanBeRetrievedByIndex(int rowIndex, string expectedId, string expectedName)
        {
            var row = csvDataReader.GetRowData(rowIndex);

            Assert.IsNotNull(row);
            Assert.IsTrue(row[0].Equals(expectedId));
            Assert.IsTrue(row[1].Equals(expectedName));           
        }       

        [TestCase(0, 1, "one")]
        [TestCase(1, 2, "two")]
        public void ValidateThatRowCanBeRetrievedByIndexAsSpecifiedType(int rowIndex, int expectedId, string expectedName)
        {
            var row = csvDataReader.GetRowDataAs<Student>(rowIndex);

            Assert.IsNotNull(row);
            Assert.IsTrue(row.Id.Equals(expectedId));
            Assert.IsTrue(row.Name.Equals(expectedName));
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
            Assert.AreEqual(expectedResult, canProcessFileType);
        }

        public class Student
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
