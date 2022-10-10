using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System.IO;
using System.Reflection;

namespace Pixel.Automation.AppExplorer.ViewModels.PrefabBuilder
{
    public class PrefabDataModelEditorViewModel : StagedSmartScreen
    {
        private readonly ILogger logger = Log.ForContext<PrefabDataModelEditorViewModel>();

        private Assembly dataModelAssembly;
        private readonly IPrefabFileSystem prefabFileSystem;
        private readonly PrefabProject prefabProject;
        private int iteration = 0;

        public IMultiEditor CodeEditor { get; set; }

        private readonly ICodeEditorFactory codeEditorFactory;

        public PrefabDataModelEditorViewModel(PrefabProject prefabProject, IPrefabFileSystem projectFileSystem, ICodeEditorFactory codeEditorFactory)
        {
            this.prefabProject = prefabProject;
            this.codeEditorFactory = codeEditorFactory;           
            this.prefabFileSystem = projectFileSystem;
        }      

        public override bool TryProcessStage(out string errorDescription)
        {
            try
            {               
                using (var compilationResult = this.codeEditorFactory.CompileProject(prefabProject.PrefabId, $"{this.prefabProject.Namespace}_{++iteration}"))
                {
                    logger.Information("Prefab assembly was successfuly compiled");
                    compilationResult.SaveAssemblyToDisk(this.prefabFileSystem.TempDirectory);                 
                    dataModelAssembly = Assembly.LoadFrom(Path.Combine(this.prefabFileSystem.TempDirectory, compilationResult.OutputAssemblyName));    
                    logger.Information($"Loaded prefab assembly : {compilationResult.OutputAssemblyName}");
                    errorDescription = string.Empty;
                    return true;
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                errorDescription = ex.Message;
                AddOrAppendErrors("", errorDescription);
                return false;
            }

        }

        public override object GetProcessedResult()
        {
            return dataModelAssembly;
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            logger.Information($"Activated screen {nameof(PrefabDataModelEditorViewModel)}");
          
            var generatedCode = this.PreviousScreen.GetProcessedResult();
            if(string.IsNullOrEmpty(generatedCode?.ToString()))
            {
                generatedCode = GetDataModelFileContent();
            }
          
            //Can go back to previous screen and come back here again in which case CodeEditor should be already available.
            if(this.CodeEditor == null)
            {
                this.codeEditorFactory.AddProject(prefabProject.PrefabId, prefabProject.Namespace, Array.Empty<string>());
                this.CodeEditor = this.codeEditorFactory.CreateMultiCodeEditorControl();
            }
         

            foreach (var file in Directory.GetFiles(prefabFileSystem.DataModelDirectory, "*.cs"))
            {
                await this.CodeEditor.AddDocumentAsync(Path.GetFileName(file), prefabProject.PrefabId, File.ReadAllText(file), false);
            }
            await this.CodeEditor.AddDocumentAsync($"{Constants.PrefabDataModelName}.cs", prefabProject.PrefabId, generatedCode.ToString(), false);
            await this.CodeEditor.OpenDocumentAsync($"{Constants.PrefabDataModelName}.cs", prefabProject.PrefabId);           
        
            await base.OnActivateAsync(cancellationToken);

            string GetDataModelFileContent()
            {
                return $"using System;{Environment.NewLine}public partial class {Constants.PrefabDataModelName}{Environment.NewLine}{{{Environment.NewLine}}}";
            }
        }

        public override void OnFinished()
        {          
            CleanUp();
            base.OnPreviousScreen();
        }

        public override void OnCancelled()
        {          
            CleanUp();
            base.OnCancelled();
        }

        private void CleanUp()
        {
            this.CodeEditor?.Dispose();
            this.CodeEditor = null;
            this.codeEditorFactory.RemoveProject(prefabProject.PrefabId);
        }
    }
}
