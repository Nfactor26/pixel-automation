using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Scripting.Editor.Core.Contracts;
using Serilog;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;

namespace Pixel.Automation.TestData.Repository.ViewModels
{
    /// <summary>
    /// TestDataRepository allows creating different data sources which can be used in a test. TestDataRepository belongs to a workspace.
    /// </summary>
    public class TestDataRepository : PropertyChangedBase
    {
        private readonly ILogger logger = Log.ForContext<TestDataRepository>();

        private readonly IProjectFileSystem projectFileSystem;     
        private readonly IScriptEditorFactory scriptEditorFactory;
        private readonly ISerializer serializer;
        private readonly IArgumentTypeProvider typeProvider;
        private readonly IArgumentTypeBrowserFactory typeBrowserFactory;
        private readonly IWindowManager windowManager;      

        public BindableCollection<TestDataSource> TestDataSourceCollection { get; set; } = new BindableCollection<TestDataSource>();

        private TestDataSource selectedTestDataSource;
        public TestDataSource SelectedTestDataSource
        {
            get => this.selectedTestDataSource;
            set
            {
                this.selectedTestDataSource = value;
                NotifyOfPropertyChange(() => SelectedTestDataSource);
            }
        }

        #region Constructor
        public TestDataRepository(ISerializer serializer, IProjectFileSystem projectFileSystem, IScriptEditorFactory scriptEditorFactory, IWindowManager windowManager, IArgumentTypeProvider typeProvider, IArgumentTypeBrowserFactory typeBrowserFactory)
        {
            this.serializer = Guard.Argument(serializer, nameof(serializer)).NotNull().Value;
            this.projectFileSystem = Guard.Argument(projectFileSystem, nameof(projectFileSystem)).NotNull().Value;
            this.scriptEditorFactory = Guard.Argument(scriptEditorFactory, nameof(scriptEditorFactory)).NotNull().Value;
            this.windowManager = Guard.Argument(windowManager, nameof(windowManager)).NotNull().Value;
            this.typeProvider = Guard.Argument(typeProvider, nameof(typeProvider)).NotNull().Value;
            this.typeBrowserFactory = Guard.Argument(typeBrowserFactory, nameof(typeBrowserFactory)).NotNull().Value;

            CreateCollectionView();
            LoadDataSources();
        }

        private void LoadDataSources()
        {
            string repositoryFolder = this.projectFileSystem.TestDataRepository;
            string[] dataSourceFiles = Directory.GetFiles(repositoryFolder, "*.dat");
            foreach (var dataSourceFile in dataSourceFiles)
            {
                var dataSource = serializer.Deserialize<TestDataSource>(dataSourceFile);
                this.TestDataSourceCollection.Add(dataSource);
            }
        }

        #endregion Constructor

        #region Filter 


        string filterText = string.Empty;
        public string FilterText
        {
            get
            {
                return filterText;
            }
            set
            {
                filterText = value;
                NotifyOfPropertyChange(() => FilterText);
                var view = CollectionViewSource.GetDefaultView(TestDataSourceCollection);
                view.Refresh();
                NotifyOfPropertyChange(() => TestDataSourceCollection);

            }
        }

        private void CreateCollectionView()
        {
            var groupedItems = CollectionViewSource.GetDefaultView(TestDataSourceCollection);
            groupedItems.GroupDescriptions.Add(new PropertyGroupDescription(nameof(TestDataSource.Name)));
            groupedItems.SortDescriptions.Add(new SortDescription(nameof(TestDataSource.Name), ListSortDirection.Ascending));
            groupedItems.Filter = new Predicate<object>((a) =>
            {
                var testDataSource = a as TestDataSource;
                return testDataSource.Name.ToLower().Contains(this.filterText.ToLower()) || testDataSource.Id.ToLower().Contains(this.filterText.ToLower());
            });
        }

        #endregion Filter         
     
        #region Create Test Data Source

        public void CreateCodedTestDataSource()
        {
            CreateDataSource(DataSource.Code);            
        }

        public void CreateCsvTestDataSource()
        {
            CreateDataSource(DataSource.CsvFile);
        }

        private async void CreateDataSource(DataSource dataSourceType)
        {
            try
            {
                var argumentTypeBrowser = typeBrowserFactory.CreateArgumentTypeBrowser();
                TestDataSourceViewModel newTestDataSource = new TestDataSourceViewModel(this.windowManager, this.projectFileSystem, dataSourceType, this.TestDataSourceCollection.Select(t => t.Name) ?? Array.Empty<string>(), argumentTypeBrowser);

                TestDataModelEditorViewModel testDataModelEditor = new TestDataModelEditorViewModel(this.scriptEditorFactory);

                newTestDataSource.NextScreen = testDataModelEditor;
                testDataModelEditor.PreviousScreen = newTestDataSource;

                TestDataSourceBuilderViewModel testDataSourceBuilder = new TestDataSourceBuilderViewModel(new IStagedScreen[] { newTestDataSource, testDataModelEditor });

                var result = await windowManager.ShowDialogAsync(testDataSourceBuilder);
                if (result.HasValue && result.Value)
                {
                    serializer.Serialize<TestDataSource>(Path.Combine(this.projectFileSystem.TestDataRepository, $"{newTestDataSource.TestDataSource.Id}.dat"), newTestDataSource.TestDataSource);
                    this.TestDataSourceCollection.Add(newTestDataSource.TestDataSource);
                    //OnDataSouceChanged(newTestDataSource.TestDataSource.Id);
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        #endregion Create Test Data Source       

        #region Edit Test Data Source

        public void EditDataSource(TestDataSource testDataSource)
        {
            try
            {
                switch (testDataSource.DataSource)
                {
                    case DataSource.Code:
                        EditCodedDataSource(testDataSource);
                        break;
                    case DataSource.CsvFile:
                        EditCsvDataSource(testDataSource);
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }      
         
        }

        private async void EditCodedDataSource(TestDataSource testDataSource)
        {
            string projectName = testDataSource.Id;          
            this.scriptEditorFactory.AddProject(projectName, Array.Empty<string>(), typeof(Empty));
            using (var scriptEditor = this.scriptEditorFactory.CreateScriptEditor())
            {
                scriptEditor.OpenDocument(testDataSource.ScriptFile, projectName, string.Empty); //File contents will be fetched from disk         
                var result = await windowManager.ShowDialogAsync(scriptEditor);
                this.scriptEditorFactory.RemoveProject(projectName);
            }
    
        }

        private async void EditCsvDataSource(TestDataSource testDataSource)
        {            
            TestDataSourceViewModel dataSourceViewModel = new TestDataSourceViewModel(this.windowManager, this.projectFileSystem, testDataSource);
            TestDataSourceBuilderViewModel testDataSourceBuilder = new TestDataSourceBuilderViewModel(new IStagedScreen[] { dataSourceViewModel });
            var result = await windowManager.ShowDialogAsync(testDataSourceBuilder);
            if (result.HasValue && result.Value)
            {
                serializer.Serialize<TestDataSource>(Path.Combine(this.projectFileSystem.TestDataRepository, $"{testDataSource.Id}.dat"), testDataSource);                
            }
        }

        #endregion Edit Data Source

    }
}
