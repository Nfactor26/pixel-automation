using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Reference.Manager.Contracts;
using Pixel.Automation.RunTime;
using Pixel.Scripting.Editor.Core.Contracts;

namespace Pixel.Automation.Designer.ViewModels.AutomationBuilder
{
    /// <summary>
    /// DesignTimePrefabLoader configure some additional services such as ScriptEditor while loading a Prefab at design time.
    /// </summary>
    public class DesignTimePrefabLoader : PrefabLoader
    {
        public DesignTimePrefabLoader(IProjectFileSystem projectFileSystem, IReferenceManager referenceManager) : base(projectFileSystem, referenceManager)
        {

        }

        protected override void ConfigureServices(IEntityManager parentEntityManager, IPrefabFileSystem prefabFileSystem)
        {
            base.ConfigureServices(parentEntityManager, prefabFileSystem);           
        
            var scriptEditorFactory = parentEntityManager.GetServiceOfType<IScriptEditorFactory>();          
            scriptEditorFactory.AddSearchPaths(prefabFileSystem.ReferencesDirectory);
        }
    }

}
