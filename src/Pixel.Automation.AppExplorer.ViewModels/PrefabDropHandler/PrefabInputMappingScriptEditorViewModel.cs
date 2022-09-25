using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Scripting.Editor.Core.Contracts;

namespace Pixel.Automation.AppExplorer.ViewModels.PrefabDropHandler
{
    /// <summary>
    /// Show script editor for configuring input mapping for the Prefab.  
    /// </summary>
    public class PrefabInputMappingScriptEditorViewModel : PrefabMappingScriptEditorViewModel
    {
        /// <inheritdoc/>   
        public PrefabInputMappingScriptEditorViewModel(IScriptEngine scriptEngine, IScriptEditorFactory scriptEditorFactory,
            PrefabEntity prefabEntity, Entity dropTarget) : base(scriptEngine, scriptEditorFactory, prefabEntity, dropTarget)
        {
            this.DisplayName = "(2/3) Configure input mapping for Prefab";
        }

        /// <inheritdoc/>   
        protected override string GetGeneratedCode()
        {
            var prefabArgumentMapper = new PrefabInputMapper();
            var assignTo = prefabEntity.GetPrefabDataModelType();
            var assignFrom = dropTarget.EntityManager.Arguments.GetType();
            var propertyMappings = prefabArgumentMapper.GenerateMapping(this.scriptEngine, assignFrom, assignTo).ToList();
            string generatedCode = prefabArgumentMapper.GeneratedMappingCode(propertyMappings, assignFrom, assignTo);
            return generatedCode;
        }

        /// <inheritdoc/>   
        protected override string GetProjectName()
        {
            return $"{prefabEntity.Id}-InputMapping-Script";
        }

        /// <inheritdoc/>   
        protected override string GetScriptFile()
        {
            return this.prefabEntity.InputMappingScriptFile;
        }

        /// <inheritdoc/>   
        public override bool Validate()
        {
            var editorText = this.ScriptEditor.GetEditorText();
            var (isValid, errors) = this.scriptEngine.IsScriptValid(editorText, dropTarget.EntityManager.Arguments);
            if (!isValid)
            {
                AddOrAppendErrors("", errors);
            }
            return isValid;
        }
    }
}
