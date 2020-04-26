using Caliburn.Micro;
using Pixel.Scripting.Editor.Core.Contracts;
using Pixel.Automation.Arguments.Editor;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using Serilog;

namespace Pixel.Automation.TestData.Repository.ViewModels
{
    public class TestDataRepository : PropertyChangedBase
    {
        private readonly ILogger logger = Log.ForContext<TestDataRepository>();

        private readonly IProjectFileSystem projectFileSystem;     
        private readonly IScriptEditorFactory scriptEditorFactory;
        private readonly ISerializer serializer;
        private readonly IArgumentTypeProvider argumentTypeProvider;
        private readonly IWindowManager windowManager;      

        public BindableCollection<TestDataSource> TestDataSourceCollection { get; set; } = new BindableCollection<TestDataSource>();

        public TestDataSource SelectedTestDataSource { get; set; }

        #region Constructor
        public TestDataRepository(ISerializer serializer, IProjectFileSystem projectFileSystem, IScriptEditorFactory scriptEditorFactory, IWindowManager windowManager, IArgumentTypeProvider argumentTypeProvider)
        {
            this.projectFileSystem = projectFileSystem;           
            this.serializer = serializer;
        
            this.scriptEditorFactory = scriptEditorFactory;
            this.windowManager = windowManager;
            this.argumentTypeProvider = argumentTypeProvider;
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
                ArgumentTypeBrowserViewModel argumentTypeBrowser = new ArgumentTypeBrowserViewModel(this.argumentTypeProvider);
                TestDataSourceViewModel newTestDataSource = new TestDataSourceViewModel(this.windowManager, this.projectFileSystem, dataSourceType, this.TestDataSourceCollection.Select(t => t.Name) ?? Array.Empty<string>(), argumentTypeBrowser);

                using (var scriptEditor = this.scriptEditorFactory.CreateInlineScriptEditor())
                {
                    TestDataModelEditorViewModel testDataModelEditor = new TestDataModelEditorViewModel(scriptEditor);

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

            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }

        #endregion Create Test Data Source

        #region Delete Test Data Source

        public void DeleteTestDataSource()
        {
            //TODO : We need to check if data source is in use by some test case before we allow deleting it
        }

        #endregion Delete Test Data Source

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
            var scriptEditor = this.scriptEditorFactory.CreateScriptEditor();
            scriptEditor.OpenDocument(testDataSource.ScriptFile, string.Empty); //File contents will be fetched from disk         
            var result = await windowManager.ShowDialogAsync(scriptEditor);          
        }

        private async void EditCsvDataSource(TestDataSource testDataSource)
        {
            ArgumentTypeBrowserViewModel argumentTypeBrowser = new ArgumentTypeBrowserViewModel(this.argumentTypeProvider);
            TestDataSourceViewModel dataSourceViewModel = new TestDataSourceViewModel(this.windowManager, this.projectFileSystem, testDataSource, argumentTypeBrowser);
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
