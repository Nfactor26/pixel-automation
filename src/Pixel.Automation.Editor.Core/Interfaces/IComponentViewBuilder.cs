using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core.ViewModels;

namespace Pixel.Automation.Editor.Core.Interfaces
{  
    public interface IComponentViewBuilder
    {
        void SetRoot(EntityComponentViewModel root);

        void OpenTestFixture(TestFixture fixture);

        void CloseTestFixture(TestFixture fixture);

        void OpenTestCase(TestCase testCase);

        void CloseTestCase(TestCase testCase);
    }
}
