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

namespace Pixel.Automation.AppExplorer.ViewModels.PrefabBuilder
{
    public class PrefabDataModelEditorViewModel : StagedSmartScreen
    {
        private Assembly dataModelAssembly;
        private readonly IPrefabFileSystem prefabFileSystem;
        private readonly PrefabDescription prefabDescription;
        private int iteration = -1;

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
                var workspaceManager = this.codeEditorFactory.GetWorkspaceManager();
                using (var compilationResult = workspaceManager.CompileProject($"{this.prefabDescription.PrefabName.Trim().Replace(' ', '_')}_{++iteration}"))
                {
                    compilationResult.SaveAssemblyToDisk(this.prefabFileSystem.TempDirectory);                 
                    dataModelAssembly = Assembly.LoadFrom(Path.Combine(this.prefabFileSystem.TempDirectory, compilationResult.OutputAssemblyName));              
                    errorDescription = string.Empty;
                    return true;
                }

            }
            catch (Exception exception)
            {
                errorDescription = exception.Message;
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
            var generatedCode = (this.PreviousScreen as IStagedScreen).GetProcessedResult();
            if(string.IsNullOrEmpty(generatedCode?.ToString()))
            {
                generatedCode = GetDataModelFileContent();
            }           
           
            this.CodeEditor = codeEditorFactory.CreateMultiCodeEditorControl();

            foreach (var file in Directory.GetFiles(prefabFileSystem.DataModelDirectory, "*.cs"))
            {
                await this.CodeEditor.AddDocumentAsync(Path.GetFileName(file), File.ReadAllText(file), false);
            }
            await this.CodeEditor.AddDocumentAsync($"{Constants.PrefabDataModelName}.cs", generatedCode.ToString(), false);
            await this.CodeEditor.OpenDocumentAsync($"{Constants.PrefabDataModelName}.cs");           
        
            await base.OnActivateAsync(cancellationToken);

            string GetDataModelFileContent()
            {
                return $"using System;{Environment.NewLine}public partial class {Constants.PrefabDataModelName}{Environment.NewLine}{{{Environment.NewLine}}}";
            }
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            this.CodeEditor?.Dispose();
            this.CodeEditor = null;
            await base.OnDeactivateAsync(close, cancellationToken);
        }
    }
}
