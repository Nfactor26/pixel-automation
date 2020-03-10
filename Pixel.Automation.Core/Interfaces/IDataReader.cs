using Pixel.Automation.Core.TestData;
using System.Collections.Generic;

namespace Pixel.Automation.Core.Interfaces
{
    /// <summary>
    /// Read data from row based data files such as csv, excel , etc
    /// </summary>
    public interface IDataReader
    {
        void Initialize(DataSourceConfiguration metaData);

        /// <summary>
        /// Get the number of rows in data souce
        /// </summary>
        /// <returns></returns>
        int GetRowCount();

        /// <summary>
        /// Get the headers for data source
        /// </summary>
        /// <returns></returns>
        string[] GetHeaders();

        /// <summary>
        /// Get data for a given row as array of strings
        /// </summary>
        /// <returns></returns>
        string[] GetRowData(int rowIndex);

        /// <summary>
        /// Get data for a given row mapped to type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        T GetRowDataAs<T>(int rowIndex) where T : class, new();

        /// <summary>
        /// Get the data from all rows
        /// </summary>
        /// <returns></returns>
        IEnumerable<string[]> GetAllRows();

        /// <summary>
        /// Get the data from all rows mapped to type T
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> GetAllRowsAs<T>() where T : class, new();

        /// <summary>
        /// Returns true or false based on whether this data reader can process a file of a given type e.g. csv / xls / etc.
        /// </summary>
        /// <param name="fileType">File type  e.g. csv, xls, etc. </param>
        /// <returns></returns>
        bool CanProcessFileType(string fileType);
    }

}
