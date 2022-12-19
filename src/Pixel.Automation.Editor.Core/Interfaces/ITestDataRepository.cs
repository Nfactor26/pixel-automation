using Pixel.Automation.Core.TestData;
using System.Threading.Tasks;

namespace Pixel.Automation.Editor.Core.Interfaces
{
    /// <summary>
    /// Module for managing the <see cref="TestDataSource"/> e.g. create or edit.
    /// </summary>
    public interface ITestDataRepository
    {
        /// <summary>
        /// Show the wizard for creating a coded test data source
        /// </summary>
        Task CreateCodedTestDataSource();

        /// <summary>
        /// Show the wizard for creating a csv backed test data source
        /// </summary>
        Task CreateCsvTestDataSource();

        /// <summary>
        /// Open an existing TestData source for edit
        /// </summary>
        /// <param name="testDataSource"></param>
        Task EditDataSource(TestDataSource testDataSource);
    }
}
