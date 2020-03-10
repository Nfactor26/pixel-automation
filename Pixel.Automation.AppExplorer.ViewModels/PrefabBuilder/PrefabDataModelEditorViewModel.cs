using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;

namespace Pixel.Automation.AppExplorer.ViewModels.PrefabBuilder
{
    public class PrefabDataModelEditorViewModel : StagedSmartScreen
    {
        private Assembly dataModelAssembly;
        private readonly IPrefabFileSystem prefabFileSystem;
        private readonly PrefabDescription prefabDescription;
        private int iteration = -1;

        public ICodeEditorControl CodeEditor { get; set; }

        private readonly ICodeEditorFactory codeEditorFactory;

        public PrefabDataModelEditorViewModel(PrefabDescription prefabDescription, IPrefabFileSystem projectFileSystem, ICodeEditorFactory codeEditorFactory)
        {
            this.prefabDescription = prefabDescription;
            this.codeEditorFactory = codeEditorFactory;
            this.CodeEditor = codeEditorFactory.CreateCodeEditorControl();
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
                    this.CodeEditor.CloseDocument(true);
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
            this.CodeEditor.OpenDocument("DataModel.cs", string.Empty); //File is saved to disk as well
            this.CodeEditor.SetContent("DataModel.cs", generatedCode.ToString());
            this.CodeEditor.Activate();
            await base.OnActivateAsync(cancellationToken);

            string GetDataModelFileContent()
            {
                return $"using System;{Environment.NewLine}public partial class DataModel{Environment.NewLine}{{{Environment.NewLine}}}";
            }
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            this.CodeEditor.CloseDocument(false);
            await base.OnDeactivateAsync(close, cancellationToken);
        }
    }
}
