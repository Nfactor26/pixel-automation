using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.RunTime;
using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Scripting.Reference.Manager.Contracts;
using System.Reflection;

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

        protected override void ConfigureServices(IEntityManager parentEntityManager, IPrefabFileSystem prefabFileSystem, Assembly prefabAssembly)
        {
            base.ConfigureServices(parentEntityManager, prefabFileSystem, prefabAssembly);
            
            //Script editor when working with prefab input and output mapping script should be able to resolve references from prefab references folder
            //at design time
            var scriptEditorFactory = parentEntityManager.GetServiceOfType<IScriptEditorFactory>();
            scriptEditorFactory.AddSearchPaths(prefabFileSystem.ReferencesDirectory);
        }    
    }

}
