using System.Collections.Generic;

namespace Pixel.Automation.Core.Interfaces
{
    public interface IDataSourceReader
    {
        /// <summary>
        /// Append a suffix to data source file name to dynamically load data from different data source.
        /// Ex: DataSource is configured to use script dataSource.csx . However, suffix is set to a non-empty value say secondary.
        /// In this case , DataSourceReader should check if file dataSource.secondary.csx exists. If it exists, use that file.
        /// If suffix file doesn't exist, use the default file dataSource.csx instead.
        /// </summary>
        /// <param name="environment"></param>
        void SetDataSourceSuffix(string environment);

        /// <summary>
        /// Load data from Test data source given it's id
        /// </summary>
        /// <param name="dataSourceId">Test Case</param>
        /// <returns></returns>
        IEnumerable<object> LoadData(string dataSourceId);
    }
}
