using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Core.ViewModels;
using System.Diagnostics;

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
        
        public void OpenPrefab(Prefab prefab, Entity prefabEntity)
        {
            var prefabHost = GetPrefabHost(this.root, prefab);
            prefabHost.AddComponent(prefabEntity);
            Debug.Assert(prefabHost.ComponentCollection.Count == 1, "PrefabHost should have exactly 1 component after it is opened");
        }

        public void ClosePrefab(Prefab prefab, Entity prefabEntity)
        {
            var prefabHost = GetPrefabHost(this.root, prefab);
            prefabHost.RemoveComponent(prefabEntity);
            Debug.Assert(prefabHost.ComponentCollection.Count == 0, "PrefabHost should not have any components after it is closed");
        }


        public static EntityComponentViewModel GetPrefabHost(EntityComponentViewModel rootEntity, Prefab targetPrefab)
        {
            foreach (var component in rootEntity.ComponentCollection)
            {
                Debug.WriteLine(component.Model.Name);
                if (component is EntityComponentViewModel entityComponentViewModel)
                {
                    //when edit button is clicked we set in design mode to true and when closing is in design mode is already true.
                    if (entityComponentViewModel.Model is PrefabHostEntity prefabHost && prefabHost.IsInDesignMode
                        && prefabHost.PrefabId.Equals(targetPrefab.PrefabId) )
                    {
                        return entityComponentViewModel;
                    }
                    var result =  GetPrefabHost(entityComponentViewModel, targetPrefab);
                    if(result is not null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }
      
    }
}
