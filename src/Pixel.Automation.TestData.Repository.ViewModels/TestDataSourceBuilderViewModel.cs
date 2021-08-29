using Dawn;
using Pixel.Automation.Editor.Core;
using System.Collections.Generic;
using Pixel.Automation.Core.TestData;

namespace Pixel.Automation.TestData.Repository.ViewModels
{
    /// <summary>
    /// Host for the screens that user goes through while configuring a <see cref="TestDataSource"/>
    /// </summary>
    public class TestDataSourceBuilderViewModel : Wizard
    {     
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="stagedScreens">collection of screens</param>
        public TestDataSourceBuilderViewModel(IEnumerable<IStagedScreen> stagedScreens)
        {
            Guard.Argument(stagedScreens).NotNull().NotEmpty();
            this.DisplayName = "Data Source Editor";
            this.stagedScreens.AddRange(stagedScreens);
        }     
    }
}
