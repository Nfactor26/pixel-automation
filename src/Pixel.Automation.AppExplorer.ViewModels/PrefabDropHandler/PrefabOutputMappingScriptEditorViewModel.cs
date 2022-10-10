using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Scripting.Editor.Core.Contracts;

namespace Pixel.Automation.AppExplorer.ViewModels.PrefabDropHandler
{
    /// <summary>
    /// Show script editor for configuring output mapping for the Prefab.  
    /// </summary>
    public class PrefabOutputMappingScriptEditorViewModel : PrefabMappingScriptEditorViewModel
    {
        /// <inheritdoc/>   
        public PrefabOutputMappingScriptEditorViewModel(IScriptEngine scriptEngine, IScriptEditorFactory scriptEditorFactory,
            PrefabEntity prefabEntity, Entity dropTarget) : base(scriptEngine, scriptEditorFactory, prefabEntity, dropTarget)
        {
            this.DisplayName = "(3/3) Configure output mapping for Prefab";
        }

        /// <inheritdoc/>   
        protected override string GetGeneratedCode()
        {
            var prefabArgumentMapper = new PrefabOutputMapper();
            var assignFrom = prefabEntity.GetPrefabDataModelType();
            var assignTo = dropTarget.EntityManager.Arguments.GetType();
            var propertyMappings = prefabArgumentMapper.GenerateMapping(this.scriptEngine, assignFrom, assignTo).ToList();
            string generatedCode = prefabArgumentMapper.GeneratedMappingCode(propertyMappings, assignFrom, assignTo);
            return generatedCode;
        }

        /// <inheritdoc/>   
        protected override void AddProject(string[] projectReferences)
        {
            this.scriptEditorFactory.AddProject(GetProjectName(), projectReferences, prefabEntity.GetPrefabDataModelType());
        }

        /// <inheritdoc/>   
        protected override string GetProjectName()
        {
            return $"{prefabEntity.Id}-OutputMapping-Script";
        }

        /// <inheritdoc/>   
        protected override string GetScriptFile()
        {
            return this.prefabEntity.OutputMappingScriptFile;
        }

        /// <inheritdoc/>   
        public override bool Validate()
        {
            var editorText = this.ScriptEditor.GetEditorText();
            var prefabArgument = Activator.CreateInstance(prefabEntity.GetPrefabDataModelType());
            var (isValid, errors) = this.scriptEngine.IsScriptValid(editorText, prefabArgument);
            if (!isValid)
            {
                AddOrAppendErrors("", errors);
            }
            return isValid;
        }
    }
}
