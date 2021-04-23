using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Automation.TestData.Repository.ViewModels
{
    public class TestDataModelEditorViewModel : StagedSmartScreen
    {
        public IInlineScriptEditor ScriptEditor { get; set; }
       
        private readonly IScriptEditorFactory editorFactory;
       
        private TestDataSource testDataSource;

        public TestDataModelEditorViewModel(IScriptEditorFactory editorFactory)
        {
            this.editorFactory = editorFactory;
            this.ScriptEditor = editorFactory.CreateInlineScriptEditor();
            this.ScriptEditor.SetEditorOptions(new EditorOptions() { FontSize = 23 });
        }

        private bool TryGenerateDataModelCode(out string generatedCode, out string errorDescription)
        {
            generatedCode = string.Empty;
            errorDescription = string.Empty;

            TestDataSourceViewModel testDataSourceViewModel = this.PreviousScreen as TestDataSourceViewModel;
            if(testDataSourceViewModel == null)
            {
                return false;
            }

            testDataSource = testDataSourceViewModel.TestDataSource;
            Type dataSourceType = testDataSourceViewModel.TypeDefinition.ActualType;

            switch (testDataSource.DataSource)
            {
                case DataSource.Code:
                    generatedCode = GenerateScriptForCodedDataSource(dataSourceType);
                    break;
                case DataSource.CsvFile:
                    generatedCode = GenerateScriptForCsvDataSource(dataSourceType);
                    break;
                default:
                    errorDescription = $"DataSource {testDataSource.DataSource} isn't supported";
                    return false;
            }

            errorDescription = string.Empty;
            return true;
        }

        private string GenerateScriptForCodedDataSource(Type dataSourceType)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(dataSourceType.GetRequiredImportsForType(Enumerable.Empty<Assembly>(), new string[] { typeof(object).Namespace, typeof(IEnumerable<>).Namespace }));
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append($"IEnumerable<{dataSourceType.GetDisplayName()}> GetDataRows()");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("{");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("\t");
            stringBuilder.Append("yield break;");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }

        private string GenerateScriptForCsvDataSource(Type dataSourceType)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(dataSourceType.GetRequiredImportsForType(Enumerable.Empty<Assembly>(), new string[] { typeof(object).Namespace, typeof(IEnumerable<>).Namespace, 
            typeof(IDataReader).Namespace, typeof(TestDataSource).Namespace }));           
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append($"IEnumerable<{dataSourceType.GetDisplayName()}> GetDataRows(TestDataSource dataSource, IDataReader dataReader)");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("{");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("\t dataReader.Initialize(dataSource.MetaData);");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append($"\t return dataReader.GetAllRowsAs<{dataSourceType.GetDisplayName()}>();");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }       

        public override bool IsValid => true;

        public override bool TryProcessStage(out string errorDescription)
        {
            this.ScriptEditor.CloseDocument(true);
            errorDescription = string.Empty;
            return true;
        }

        public override object GetProcessedResult()
        {
            return true;
        }


        protected override  async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            if (TryGenerateDataModelCode(out string generatedCode, out string errorDescription))
            {                
                editorFactory.AddProject(testDataSource.Name, Array.Empty<string>(), typeof(Empty));
                this.ScriptEditor.OpenDocument(this.testDataSource.ScriptFile, testDataSource.Name, generatedCode.ToString()); //File is saved to disk as well 
                this.ScriptEditor.Activate();
            }
            if (!string.IsNullOrEmpty(errorDescription))
            {
                AddOrAppendErrors(string.Empty, errorDescription);
            }

            await base.OnActivateAsync(cancellationToken);
        }

       

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            this.ScriptEditor.CloseDocument(false); //TODO : this should be disposed ... Need to add methods like OnCancel and OnBack() to stages screens where cleanup should happen.
            this.editorFactory.RemoveProject(testDataSource.Name);
            await base.OnDeactivateAsync(close, cancellationToken);
        }
    }
}
