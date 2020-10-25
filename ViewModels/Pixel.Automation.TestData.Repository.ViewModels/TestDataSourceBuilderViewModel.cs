using Caliburn.Micro;
using Pixel.Automation.Editor.Core;
using System.Collections.Generic;

namespace Pixel.Automation.TestData.Repository.ViewModels
{
    public class TestDataSourceBuilderViewModel : Wizard
    {     
        public TestDataSourceBuilderViewModel(IEnumerable<IScreen> stagedScreens)
        {
            this.DisplayName = "Create new data source";
            this.stagedScreens.AddRange(stagedScreens);
        }     
    }
}
