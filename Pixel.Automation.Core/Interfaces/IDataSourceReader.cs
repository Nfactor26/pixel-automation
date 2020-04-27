using System.Collections.Generic;

namespace Pixel.Automation.Core.Interfaces
{
    public interface IDataSourceReader
    {
        /// <summary>
        /// Load data from Test data source given it's id
        /// </summary>
        /// <param name="dataSourceId">Test Case</param>
        /// <returns></returns>
        IEnumerable<object> LoadData(string dataSourceId);
    }
}
