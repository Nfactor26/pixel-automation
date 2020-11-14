using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using Pixel.Automation.Core;
using Serilog;

namespace Pixel.Automation.AppExplorer.ViewModels.PrefabBuilder
{
    public class PrefabDataModelEditorViewModel : StagedSmartScreen
    {
        private readonly ILogger logger = Log.ForContext<PrefabDataModelEditorViewModel>();

        private Assembly dataModelAssembly;
        private readonly IPrefabFileSystem prefabFileSystem;
        private readonly PrefabDescription prefabDescription;
        private int iteration = 0;

        public IMultiEditor CodeEditor { get; set; }

        private readonly ICodeEditorFactory codeEditorFactory;

        public PrefabDataModelEditorViewModel(PrefabDescription prefabDescription, IPrefabFileSystem projectFileSystem, ICodeEditorFactory codeEditorFactory)
        {
            this.prefabDescription = prefabDescription;
            this.codeEditorFactory = codeEditorFactory;           
            this.prefabFileSystem = projectFileSystem;
        }      

        public override bool TryProcessStage(out string errorDescription)
        {
            try
            {               
                using (var compilationResult = this.codeEditorFactory.CompileProject(prefabDescription.PrefabId, $"{this.prefabDescription.PrefabName.Trim().Replace(' ', '_')}_{++iteration}"))
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
            logger.Information($"Activate screen is {nameof(PrefabDataModelEditorViewModel)}");
          
            var generatedCode = (this.PreviousScreen as IStagedScreen).GetProcessedResult();
            if(string.IsNullOrEmpty(generatedCode?.ToString()))
            {
                generatedCode = GetDataModelFileContent();
            }
          
            //Can go back to previous screen and come back here again in which case CodeEditor should be already available.
            if(this.CodeEditor == null)
            {
                this.codeEditorFactory.AddProject(prefabDescription.PrefabId, Array.Empty<string>());
                this.CodeEditor = this.codeEditorFactory.CreateMultiCodeEditorControl();
            }
         

            foreach (var file in Directory.GetFiles(prefabFileSystem.DataModelDirectory, "*.cs"))
            {
                await this.CodeEditor.AddDocumentAsync(Path.GetFileName(file), prefabDescription.PrefabId, File.ReadAllText(file), false);
            }
            await this.CodeEditor.AddDocumentAsync($"{Constants.PrefabDataModelName}.cs", prefabDescription.PrefabId, generatedCode.ToString(), false);
            await this.CodeEditor.OpenDocumentAsync($"{Constants.PrefabDataModelName}.cs", prefabDescription.PrefabId);           
        
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
            this.codeEditorFactory.RemoveProject(prefabDescription.PrefabId);
        }
    }
}
