using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Components.TestCase;
using Pixel.Automation.Editor.Core;
using Pixel.Scripting.Editor.Core.Contracts;

namespace Pixel.Automation.AppExplorer.ViewModels.PrefabDropHandler
{
    /// <summary>
    /// Base class that handles generation of default mapping script and editing.
    /// Script is also validated as a part of step.
    /// </summary>
    public abstract class PrefabMappingScriptEditorViewModel : StagedSmartScreen
    {
        protected readonly IScriptEngine scriptEngine;
        protected readonly IScriptEditorFactory scriptEditorFactory;
        protected readonly PrefabEntity prefabEntity;
        protected readonly Entity dropTarget;

        public IInlineScriptEditor ScriptEditor { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="scriptEngine">ScriptEngine associated with the entity which is the drop target</param>
        /// <param name="scriptEditorFactory">ScriptEditorFactory for the active Automation Process</param>
        /// <param name="prefabEntity">PrefabEntity which is being configured</param>
        /// <param name="dropTarget"><see cref="Entity"/> to which Prefab is being added</param>
        public PrefabMappingScriptEditorViewModel(IScriptEngine scriptEngine, IScriptEditorFactory scriptEditorFactory, PrefabEntity prefabEntity, Entity dropTarget)
        {
            this.scriptEngine = scriptEngine;
            this.scriptEditorFactory = scriptEditorFactory;
            this.prefabEntity = prefabEntity;
            this.dropTarget = dropTarget;
        }

        /// <inheritdoc/>       
        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            string generatedCode = GetGeneratedCode();
            this.ScriptEditor = this.scriptEditorFactory.CreateInlineScriptEditor(new EditorOptions() 
            {
                EnableDiagnostics = true,
                ShowLineNumbers = true,
                FontSize = 23
            });           
            if (dropTarget.TryGetAnsecstorOfType<TestCaseEntity>(out TestCaseEntity testCaseEntity))
            {
                //Test cases have a initialization script file which contains all declared variables. In order to get intellisense support for those variable, we need a reference to that project
                AddProject(new string[] { testCaseEntity.Tag });
            }
            else if (dropTarget.TryGetAnsecstorOfType<TestFixtureEntity>(out TestFixtureEntity testFixtureEntity))
            {
                //Test fixture have a initialization script file which contains all declared variables. In order to get intellisense support for those variable, we need a reference to that project                
                AddProject(new string[] { testFixtureEntity.Tag });
            }
            else
            {
                AddProject(Array.Empty<string>());
            }
            this.scriptEditorFactory.AddDocument(GetScriptFile(), GetProjectName(), generatedCode);
            this.ScriptEditor.OpenDocument(GetScriptFile(), GetProjectName(), generatedCode);

            NotifyOfPropertyChange(nameof(this.ScriptEditor));

            this.ScriptEditor.Activate();
          
            return base.OnActivateAsync(cancellationToken);
        }

        protected abstract void AddProject(string[] projectReferences);

        /// <summary>
        /// Get the generated code for mapping
        /// </summary>
        /// <returns></returns>
        protected abstract string GetGeneratedCode();

        /// <summary>
        /// Get the name of the project to which script should be added in the workspace
        /// </summary>
        /// <returns></returns>
        protected abstract string GetProjectName();

        /// <summary>
        /// Get the name of the script file which is being edited
        /// </summary>
        /// <returns></returns>
        protected abstract string GetScriptFile();

        /// <inheritdoc/> 
        public override object GetProcessedResult()
        {
            return this.ScriptEditor?.GetEditorText() ?? string.Empty;
        }

        /// <inheritdoc/> 
        public override bool TryProcessStage(out string errorDescription)
        {          
            this.ScriptEditor.Deactivate();
            errorDescription = String.Empty;
            return true;
        }

        /// <inheritdoc/> 
        public override void OnFinished()
        {
            DisposeEditor();
            base.OnFinished();
        }

        /// <inheritdoc/> 
        public override void OnCancelled()
        {
            DisposeEditor();
            base.OnCancelled();
        }

        /// <summary>
        /// Remove the project that was added and close the script document and dispose the editor.
        /// </summary>
        void DisposeEditor()
        {

            if (this.ScriptEditor != null)
            {
                this.scriptEditorFactory.RemoveProject(prefabEntity.Id);
                this.ScriptEditor.Deactivate();
                this.ScriptEditor.CloseDocument();
                this.ScriptEditor.Dispose();
            }
        }
    }
}
