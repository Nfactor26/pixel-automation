using Dawn;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Core.ViewModels;

namespace Pixel.Automation.Designer.ViewModels.AutomationBuilder
{
    internal class ComponentViewBuilder : IComponentViewBuilder
    {
        private EntityComponentViewModel root;

        public ComponentViewBuilder()
        {

        }

        public void SetRoot(EntityComponentViewModel root)
        {
            this.root = Guard.Argument(root);
        }

        public void OpenTestFixture(TestFixture fixture)
        {
            root.AddComponent(fixture.TestFixtureEntity);
        }

        public void CloseTestFixture(TestFixture fixture)
        {
            root.RemoveComponent(fixture.TestFixtureEntity);
        }

        public void OpenTestCase(TestCase testCase)
        {
            foreach(var component in root.ComponentCollection)
            {
                if(component.Model.Tag.Equals(testCase.FixtureId) && component is EntityComponentViewModel fixtureEntityViewmodel)
                {
                    fixtureEntityViewmodel.AddComponent(testCase.TestCaseEntity);
                }
            }
        }

        public void CloseTestCase(TestCase testCase)
        {
            foreach (var component in root.ComponentCollection)
            {
                if (component.Model.Tag.Equals(testCase.FixtureId) && component is EntityComponentViewModel fixtureEntityViewmodel)
                {
                    fixtureEntityViewmodel.RemoveComponent(testCase.TestCaseEntity);
                }
            }
        }      

    }
}
