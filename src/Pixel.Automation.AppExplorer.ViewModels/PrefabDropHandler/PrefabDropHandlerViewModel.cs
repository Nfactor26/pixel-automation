using Pixel.Automation.AppExplorer.ViewModels.Prefab;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.ViewModels;
using Pixel.Scripting.Editor.Core.Contracts;
using System.IO;

namespace Pixel.Automation.AppExplorer.ViewModels.PrefabDropHandler
{
    /// <summary>
    /// Show a wizard to allow selection of prefab version to use and configure input and output mapping scripts for Prefab
    /// when a prefab is dropped on to automation editor from prefab explorer. The Prefab will be added as a child of the entity 
    /// which is the drop target as a part of this process.
    /// </summary>
    public class PrefabDropHandlerViewModel : Wizard
    {
        private readonly IPrefabFileSystem prefabFileSystem;
        private readonly IProjectFileSystem projectFileSystem;
        private readonly IScriptEngine scriptEngine;
        private readonly IScriptEditorFactory scriptEditorFactory;

        private PrefabEntity prefabEntity;
        private EntityComponentViewModel dropTarget;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="entityManager">EntityManager of the Entity which is the drop target</param>
        public PrefabDropHandlerViewModel(IEntityManager entityManager, PrefabProjectViewModel prefabProjectViewModel, EntityComponentViewModel dropTarget)
        {
            this.DisplayName = "(1/3) Select prefab version and mapping scripts";
            this.scriptEditorFactory = entityManager.GetServiceOfType<IScriptEditorFactory>(); ;
            this.projectFileSystem = entityManager.GetCurrentFileSystem() as IProjectFileSystem;
            this.prefabFileSystem = entityManager.GetServiceOfType<IPrefabFileSystem>();
            this.scriptEngine = entityManager.GetScriptEngine();
            this.dropTarget = dropTarget;

            this.prefabEntity = new PrefabEntity()
            {
                Name = prefabProjectViewModel.PrefabName,
                PrefabId = prefabProjectViewModel.PrefabId,
                ApplicationId = prefabProjectViewModel.ApplicationId
            };
            var initializers = prefabEntity.GetType().GetCustomAttributes(typeof(InitializerAttribute), true).OfType<InitializerAttribute>();
            foreach (var intializer in initializers)
            {
                IComponentInitializer componentInitializer = Activator.CreateInstance(intializer.Initializer) as IComponentInitializer;
                componentInitializer.IntializeComponent(prefabEntity, dropTarget.Model.EntityManager);
            }

            this.stagedScreens.Clear();

            var prefabVersionSelector = new PrefabVersionSelectorViewModel(this.projectFileSystem, this.prefabFileSystem, this.prefabEntity,
                prefabProjectViewModel, dropTarget);
            var prefabInputMappingScript = new PrefabInputMappingScriptEditorViewModel(this.scriptEngine, this.scriptEditorFactory, this.prefabEntity, dropTarget.Model as Entity);
            var prefabOutputMappingScript = new PrefabOutputMappingScriptEditorViewModel(this.scriptEngine, this.scriptEditorFactory, this.prefabEntity, dropTarget.Model as Entity);

            prefabVersionSelector.NextScreen = prefabInputMappingScript;
            prefabInputMappingScript.NextScreen = prefabOutputMappingScript;

            this.stagedScreens.Add(prefabVersionSelector);
            this.stagedScreens.Add(prefabInputMappingScript);
            this.stagedScreens.Add(prefabOutputMappingScript);
        }

        public override async Task Cancel()
        {
            var prefabComponentViewModel = this.dropTarget.ComponentCollection.FirstOrDefault(a => a.Model.Equals(this.prefabEntity));
            if (prefabComponentViewModel != null)
            {
                dropTarget.RemoveComponent(prefabComponentViewModel);
                var inputMappingScriptFile = Path.Combine(this.projectFileSystem.ScriptsDirectory, Path.GetFileName(this.prefabEntity.InputMappingScriptFile));
                if (File.Exists(inputMappingScriptFile))
                {
                    File.Delete(inputMappingScriptFile);
                }
                var ouutpuMappingScriptFile = Path.Combine(this.projectFileSystem.ScriptsDirectory, Path.GetFileName(this.prefabEntity.OutputMappingScriptFile));
                if (File.Exists(ouutpuMappingScriptFile))
                {
                    File.Delete(ouutpuMappingScriptFile);
                }
            }
            await this.TryCloseAsync(false);
        }
    }
}
