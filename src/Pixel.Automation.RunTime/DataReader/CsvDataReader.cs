using CsvHelper;
using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pixel.Automation.RunTime.DataReader;

/// <inheritdoc/>
public class CsvDataReader : IDataReader
{
    
    private CsvDataSourceConfiguration metaData;   
    private CsvReaderOptions configuration;

    public void Initialize(DataSourceConfiguration metaData)
    {
        Guard.Argument(metaData).HasValue();
        this.metaData = Guard.Argument<DataSourceConfiguration>(metaData).Cast<CsvDataSourceConfiguration>();    
        this.configuration = new CsvReaderOptions() 
        { 
            Separator = this.metaData.Delimiter,
            HasHeader = this.metaData.HasHeader
        };

        if (!File.Exists(this.metaData.TargetFile))
        {
            throw new FileNotFoundException("File doesn't exist", this.metaData.TargetFile);
        }             
    }
   
    public IEnumerable<string[]> GetAllRows()
    {
        using (var csvReader = CsvReader.FromFile(this.metaData.TargetFile, this.configuration))
        {
            if (metaData.HasHeader)
            {
                csvReader.ReadHeader();
            }
            while (csvReader.Read())
            {
                string[] rowData = csvReader.Current.ToArray();
                yield return rowData;
            }

        }
    }

    public IEnumerable<T> GetAllRowsAs<T>() where T : class, new()
    {
        List<T> rows = new List<T>();
        using (var csvReader = CsvReader.FromFile(this.metaData.TargetFile, this.configuration))
        {
            if (metaData.HasHeader)
            {
                csvReader.ReadHeader();
            }
            while (csvReader.Read())
            {
                T instance = csvReader.GetRecord<T>();
                rows.Add(instance);
            }

        }      
        return rows;
    }

    public string[] GetHeaders()
    {
        if (metaData.HasHeader)
        {
            using (var csvReader = CsvReader.FromFile(this.metaData.TargetFile, this.configuration))
            {
                csvReader.ReadHeader();
                return csvReader.Headers.ToArray();
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
        if (metaData.HasHeader)
        {
            --rowCount;
        }
        return rowCount;
    }

    public string[] GetRowData(int rowIndex)
    {
        using (var csvReader = CsvReader.FromFile(this.metaData.TargetFile, this.configuration))
        {               
            if (metaData.HasHeader)
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
            return csvReader.Current.ToArray();
        }

    }

    public T GetRowDataAs<T>(int rowIndex) where T : class, new()
    {
        using (var csvReader = CsvReader.FromFile(this.metaData.TargetFile, this.configuration))
        {
            if (metaData.HasHeader)
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

    public bool CanProcessFileType(string fileType)
    {
        return fileType?.Equals("csv") ?? false;
    }
}
