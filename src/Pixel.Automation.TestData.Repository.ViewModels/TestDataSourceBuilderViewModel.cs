using Pixel.Automation.Editor.Core;
using System.Collections.Generic;

namespace Pixel.Automation.TestData.Repository.ViewModels
{
    public class TestDataSourceBuilderViewModel : Wizard
    {     
        public TestDataSourceBuilderViewModel(IEnumerable<IStagedScreen> stagedScreens)
        {
            this.DisplayName = "Data Source Editor";
            this.stagedScreens.AddRange(stagedScreens);
        }     
    }
}
