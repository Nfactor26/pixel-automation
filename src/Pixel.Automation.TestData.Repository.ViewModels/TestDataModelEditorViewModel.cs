using Pixel.Automation.Core.Extensions;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Automation.TestData.Repository.ViewModels
{
    /// <summary>
    /// Generate the code for data source script and show the code in script editor
    /// </summary>
    public class TestDataModelEditorViewModel : StagedSmartScreen
    {
        private readonly ILogger logger = Log.ForContext<TestDataModelEditorViewModel>();

        private readonly IScriptEditorFactory editorFactory;
        private TestDataSource testDataSource;

        /// <summary>
        /// Script editor control
        /// </summary>
        public IInlineScriptEditor ScriptEditor { get; set; }       
   
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="editorFactory"></param>
        public TestDataModelEditorViewModel(IScriptEditorFactory editorFactory)
        {
            this.editorFactory = editorFactory;
            logger.Debug("Created a new instance of {0}", nameof(TestDataModelEditorViewModel));
        }

        /// <summary>
        /// Generate the code based on the TestDataSource details configured in previous screen
        /// </summary>
        /// <param name="generatedCode"></param>
        /// <param name="errorDescription"></param>
        /// <returns></returns>
        private bool TryGenerateDataModelCode(out string generatedCode, out string errorDescription)
        {
            generatedCode = string.Empty;
           
            Type dataSourceType;
            var result = this.PreviousScreen.GetProcessedResult() as TestDataSourceResult;
            (testDataSource, dataSourceType) = result;

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

        /// <summary>
        /// Generate the code when TestDataSource is of type Code. A method with return type IEnumerable<SourceDataType> is generated
        /// where user can create instances of SourceDataType and return these instances.
        /// </summary>
        /// <param name="dataSourceType"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Generate the code to read the data from csv file  when TestDataSource is of type Csv
        /// </summary>
        /// <param name="dataSourceType"></param>
        /// <returns></returns>
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

        ///<inheritdoc/>
        public override bool TryProcessStage(out string errorDescription)
        {
            this.ScriptEditor.CloseDocument(true);
            errorDescription = string.Empty;
            return true;
        }

        ///<inheritdoc/>
        public override object GetProcessedResult()
        {
            return true;
        }

        ///<inheritdoc/>
        protected override  async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            if(this.ScriptEditor == null)
            {
                this.ScriptEditor = editorFactory.CreateInlineScriptEditor(new EditorOptions() { EnableDiagnostics = true });
                this.ScriptEditor.SetEditorOptions(new EditorOptions() { FontSize = 23 });
            }            
            if (TryGenerateDataModelCode(out string generatedCode, out string errorDescription))
            {                
                editorFactory.AddProject(testDataSource.Name, Array.Empty<string>(), typeof(EmptyModel));
                this.ScriptEditor.OpenDocument(this.testDataSource.ScriptFile, testDataSource.Name, generatedCode.ToString()); //File is saved to disk as well 
                this.ScriptEditor.Activate();
            }
            if (!string.IsNullOrEmpty(errorDescription))
            {
                AddOrAppendErrors(string.Empty, errorDescription);
                logger.Warning("There was an error while generating data model code. {0}", errorDescription);
            }
            await base.OnActivateAsync(cancellationToken);

            logger.Debug("{0} is activated now", nameof(TestDataModelEditorViewModel));
        }

        ///<inheritdoc/>
        public override void OnCancelled()
        {
            if(this.ScriptEditor != null)
            {
                this.ScriptEditor.CloseDocument(false);              
                this.ScriptEditor.Dispose();
                this.ScriptEditor = null;
                logger.Debug("Operation cancelled by user");
            }           
        }

        ///<inheritdoc/>
        public override void OnFinished()
        {
            if (this.ScriptEditor != null)
            {
                this.ScriptEditor.CloseDocument(true);
                this.ScriptEditor.Dispose();
                this.ScriptEditor = null;
                logger.Debug("Operation completed by user");
            }         
        }
    }
}
