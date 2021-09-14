using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.RunTime;
using Pixel.Scripting.Editor.Core.Contracts;

namespace Pixel.Automation.Designer.ViewModels.AutomationBuilder
{
    /// <summary>
    /// DesignTimePrefabLoader configure some additional services such as ScriptEditor while loading a Prefab at design time.
    /// </summary>
    public class DesignTimePrefabLoader : PrefabLoader
    {
        public DesignTimePrefabLoader(IProjectFileSystem projectFileSystem) : base(projectFileSystem)
        {

        }

        protected override void ConfigureServices(IEntityManager parentEntityManager, IPrefabFileSystem prefabFileSystem)
        {
            //Process entity manager should be able to resolve any assembly from prefab references folder such as prefab data model assembly 
            var scriptEngineFactory = parentEntityManager.GetServiceOfType<IScriptEngineFactory>();
            scriptEngineFactory.WithAdditionalSearchPaths(prefabFileSystem.ReferencesDirectory);

            //Similalry, Script editor when working with prefab input and output mapping script should be able to resolve references from prefab references folder
            var scriptEditorFactory = parentEntityManager.GetServiceOfType<IScriptEditorFactory>();
            scriptEditorFactory.AddSearchPaths(prefabFileSystem.ReferencesDirectory);
        }
    }

}
