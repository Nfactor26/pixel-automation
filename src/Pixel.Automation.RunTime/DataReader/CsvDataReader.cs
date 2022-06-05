using CsvHelper;
using CsvHelper.Configuration;
using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Pixel.Automation.RunTime.DataReader
{
    /// <inheritdoc/>
    public class CsvDataReader : IDataReader
    {
        
        private CsvDataSourceConfiguration metaData;   
        private CsvConfiguration configuration;

        public void Initialize(DataSourceConfiguration metaData)
        {
            Guard.Argument(metaData).HasValue();
            this.metaData = Guard.Argument<DataSourceConfiguration>(metaData).Cast<CsvDataSourceConfiguration>();    
            this.configuration = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = this.metaData.Delimiter };

            if (!File.Exists(this.metaData.TargetFile))
            {
                throw new FileNotFoundException("File doesn't exist", this.metaData.TargetFile);
            }
            if(string.IsNullOrEmpty(this.metaData.Delimiter))
            {
                throw new ArgumentException("Delimiter is not specified");
            }        
        }
       
        public IEnumerable<string[]> GetAllRows()
        {
            using (var streamReader = new StreamReader(this.metaData.TargetFile))
            {
                using (var csvReader = new CsvReader(streamReader, this.configuration))
                {                  
                    csvReader.Read();
                    if (metaData.HasHeaders)
                    {
                        csvReader.ReadHeader();
                    }
                    while (csvReader.Read())
                    {
                        string[] rowData = csvReader.Parser.Record;
                        yield return rowData;
                    }

                }
            }
        }

        public IEnumerable<T> GetAllRowsAs<T>() where T : class, new()
        {
            List<T> rows = new List<T>();
            using (var streamReader = new StreamReader(this.metaData.TargetFile))
            {
                using (var csvReader = new CsvReader(streamReader, this.configuration))
                {                  
                    csvReader.Read();
                    if (metaData.HasHeaders)
                    {
                        csvReader.ReadHeader();
                    }
                    while (csvReader.Read())
                    {
                        T instance = csvReader.GetRecord<T>();
                        rows.Add(instance);
                    }

                }
            }
            return rows;
        }

        public string[] GetHeaders()
        {
            if (metaData.HasHeaders)
            {
                using (var streamReader = new StreamReader(this.metaData.TargetFile))
                {
                    using (var csvReader = new CsvReader(streamReader, this.configuration))
                    {                       
                        csvReader.Read();
                        csvReader.ReadHeader();
                        return csvReader.HeaderRecord;
                    }
                }
            }
            return Array.Empty<string>();
        }

        public int GetRowCount()
        {
            int rowCount = 0;
            using (StreamReader streamReader = new StreamReader(this.metaData.TargetFile))
            {
                while (streamReader.ReadLine() != null)
                {
                    rowCount++;
                }
            }
            if (metaData.HasHeaders)
            {
                --rowCount;
            }
            return rowCount;
        }

        public string[] GetRowData(int rowIndex)
        {
            using (var streamReader = new StreamReader(this.metaData.TargetFile))
            {
                using (var csvReader = new CsvReader(streamReader, this.configuration))
                {
                    csvReader.Read();
                    if (metaData.HasHeaders)
                    {
                        csvReader.ReadHeader();
                    }

                    int currentRow = -1;
                    while (currentRow < rowIndex && csvReader.Read())
                    {
                        currentRow++;
                    }
                    if (currentRow < rowIndex)
                    {
                        throw new IndexOutOfRangeException($"Row Index {rowIndex} doesn't exist in file");
                    }
                    return csvReader.Parser.Record;
                }
            }

        }

        public T GetRowDataAs<T>(int rowIndex) where T : class, new()
        {
            using (var streamReader = new StreamReader(this.metaData.TargetFile))
            {
                using (var csvReader = new CsvReader(streamReader, this.configuration))
                {                    
                    csvReader.Read();
                    if (metaData.HasHeaders)
                    {
                        csvReader.ReadHeader();
                    }

                    int currentRow = -1;
                    while (currentRow < rowIndex && csvReader.Read())
                    {
                        currentRow++;
                    }
                    if (currentRow < rowIndex)
                    {
                        throw new IndexOutOfRangeException($"Row Index {rowIndex} doesn't exist in file");
                    }
                    T instance = csvReader.GetRecord<T>();
                    return instance;
                }
            }

        }

        public bool CanProcessFileType(string fileType)
        {
            return fileType?.Equals("csv") ?? false;
        }
    }
}
