using Caliburn.Micro;
using Dawn;
using Microsoft.Win32;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pixel.Automation.TestData.Repository.ViewModels
{
    /// <summary>
    /// Allows to configure the details of a <see cref="TestDataSource"/>
    /// </summary>
    public class TestDataSourceViewModel : StagedSmartScreen
    {
        private readonly IWindowManager windowManager;
        private readonly IProjectFileSystem fileSystem;
        private readonly IArgumentTypeBrowser typeBrowser;
        private readonly IEnumerable<string> existingDataSources;
       
        /// <summary>
        /// Indicates if the screen is in edit mode.
        /// Edit mode is used to modify an existing TestDataSource.
        /// Default mode is used to create a new TestDataSource.
        /// </summary>
        public bool IsInEditMode { get;}

        /// <summary>
        /// TestDataSource model which is being configured
        /// </summary>
        public TestDataSource TestDataSource { get; }

        /// <summary>
        /// Name of the data source
        /// </summary>
        public string Name
        {
            get => TestDataSource.Name;
            set
            {
                TestDataSource.Name = value;
                NotifyOfPropertyChange(() => Name);
                ValidateProperty(nameof(Name));
            }
        }

        /// <summary>
        /// Script file for the data source.
        /// </summary>
        public string ScriptFile
        {
            get => TestDataSource.ScriptFile;
            set
            {
                TestDataSource.ScriptFile = value;
                NotifyOfPropertyChange(() => ScriptFile);
                ValidateProperty(nameof(ScriptFile));
            }
        }
        
        /// <summary>
        /// Type of DataSource i.e. script code or csv file
        /// </summary>
        public DataSource DataSource
        {
            get => TestDataSource.DataSource;
            private set
            {
                TestDataSource.DataSource = value;
                switch (TestDataSource.DataSource)
                {
                    case DataSource.Code:
                        this.MetaData = new DataSourceConfiguration();
                        break;
                    case DataSource.CsvFile:
                        this.MetaData = new CsvDataSourceConfiguration();
                        break;
                }
                NotifyOfPropertyChange(() => DataSource);
            }
        }

        /// <summary>
        /// Additional metadata for TestDataSource based on the type of DataSource
        /// </summary>
        public DataSourceConfiguration MetaData
        {
            get => TestDataSource.MetaData;      
            set
            {
                TestDataSource.MetaData = value;
                NotifyOfPropertyChange(() => MetaData);
            }
        }     
        
        /// <summary>
        /// Name of Type of data source
        /// </summary>
        public string TestDataType
        {
            get => MetaData.TargetTypeName;
            set
            {
                MetaData.TargetTypeName = value;
                NotifyOfPropertyChange(() => TestDataType);
                ValidateProperty(nameof(TestDataType));
            }
        }

        /// <summary>
        /// Name of the csv data file
        /// </summary>
        public string DataFileName
        {
            get => Path.GetFileName((MetaData as CsvDataSourceConfiguration)?.TargetFile ?? string.Empty);
            set
            {
                if (MetaData is CsvDataSourceConfiguration fsMetaData)
                {
                    fsMetaData.TargetFile = value;
                    NotifyOfPropertyChange(() => DataFileName);
                    ValidateProperty(nameof(DataFileName));
                }
            }
        }

        /// <summary>
        /// Delimiter used in csv data file
        /// </summary>
        public string Delimiter
        {
            get => (MetaData as CsvDataSourceConfiguration)?.Delimiter;
            set
            {
                if (MetaData is CsvDataSourceConfiguration fsMetaData)
                {
                    fsMetaData.Delimiter = value;
                    NotifyOfPropertyChange(() => Delimiter);
                    ValidateProperty(nameof(Delimiter));
                }
            }
        }

        /// <summary>
        /// TypeDefinition for the TestDataSource. TypeDefinition contains the details of data model type returned by data source
        /// </summary>
        public TypeDefinition TypeDefinition { get; private set; }

        /// <summary>
        /// constructor when creating a new TestDataSource
        /// </summary>
        /// <param name="windowManager"></param>
        /// <param name="fileSystem"></param>
        /// <param name="dataSource"></param>
        /// <param name="existingDataSources"></param>
        /// <param name="typeBrowser"></param>
        public TestDataSourceViewModel(IWindowManager windowManager, IProjectFileSystem fileSystem, DataSource dataSource,
           IEnumerable<string>  existingDataSources, IArgumentTypeBrowser typeBrowser)
        {          
            this.windowManager = Guard.Argument(windowManager, nameof(windowManager)).NotNull().Value;
            this.typeBrowser = Guard.Argument(typeBrowser, nameof(typeBrowser)).NotNull().Value;
            this.fileSystem = Guard.Argument(fileSystem, nameof(fileSystem)).NotNull().Value;
       
            this.TestDataSource = new TestDataSource() { Id = Guid.NewGuid().ToString() };
            this.ScriptFile = Path.GetRelativePath(fileSystem.WorkingDirectory, Path.Combine(fileSystem.TestDataRepository, $"{this.TestDataSource.Id}.csx"));
            this.DataSource = dataSource;
            this.existingDataSources = existingDataSources;
        }

        /// <summary>
        /// constructor when editing an existing TestDataSource
        /// </summary>
        /// <param name="windowManager"></param>
        /// <param name="fileSystem"></param>
        /// <param name="dataSource"></param>
        public TestDataSourceViewModel(IWindowManager windowManager, IProjectFileSystem fileSystem, TestDataSource dataSource)
        {           
            this.windowManager = Guard.Argument(windowManager, nameof(windowManager)).NotNull().Value;
            this.fileSystem = Guard.Argument(fileSystem, nameof(fileSystem)).NotNull().Value;
            this.TestDataSource = Guard.Argument(dataSource, nameof(dataSource)).NotNull().Value;
            this.IsInEditMode = true;
            NotifyOfPropertyChange(() => CanSelectTestDataType);
        }

        /// <summary>
        /// Indicates whether data type returned by TestDataSource can be changed
        /// </summary>
        public bool CanSelectTestDataType => !this.IsInEditMode;

        /// <summary>
        /// Select the data type for the TestDataSource
        /// </summary>
        public async void SelectTestDataType()
        {
            var result  = await windowManager.ShowDialogAsync(typeBrowser);
            if(result.HasValue && result.Value)
            {
                this.TypeDefinition = typeBrowser.GetCreatedType();
                this.TestDataType = this.TypeDefinition.DisplayName;
            }
        }

        /// <summary>
        /// Browse for a csv file when DataSource is a csv file
        /// </summary>
        public void BrowseForFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV files (*.csv)|*.csv";
            openFileDialog.InitialDirectory = this.fileSystem.TestDataRepository;
            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFile = openFileDialog.FileName;            
                string destFile = Path.Combine(this.fileSystem.TestDataRepository, Path.GetFileName(selectedFile));
                
                //A file with same name already exists
                if(File.Exists(destFile) && !selectedFile.Equals(destFile))
                {
                    AddOrAppendErrors(nameof(DataFileName), "A file already exists in Data Repository directory with same name.");
                    return;
                }
                if(!File.Exists(destFile))
                {
                    File.Copy(selectedFile, destFile, true);
                }
                this.DataFileName = Path.GetFileName(selectedFile);

            }
            NotifyOfPropertyChange(() => MetaData);
        }   

        ///<inheritdoc/>
        public override bool TryProcessStage(out string errorDescription)
        {
            errorDescription = string.Empty;
            return true;
        }

        ///<inheritdoc/>
        public override object GetProcessedResult()
        {
            return new TestDataSourceResult(this.TestDataSource, this.TypeDefinition.ActualType);
        }

        #region INotifyDataErrorInfo


        private void ValidateProperty(string propertyName)
        {                      
            ClearErrors(propertyName);
            switch (propertyName)
            {
                case nameof(Name):
                    ValidateRequiredProperty(nameof(Name), Name);                    
                    //existingDataSources can be null when data source is opened in edit mode.
                    if((!string.IsNullOrEmpty(Name)) && (existingDataSources?.Any(a => a.ToLower().Equals(Name.ToLower())) ?? false) )
                    {
                       AddOrAppendErrors(nameof(Name), $"DataSource with name : {Name} already exists");
                    }
                    break;
                case nameof(TestDataType):
                    ValidateRequiredProperty(nameof(TestDataType), TestDataType);
                    break;
                case nameof(Delimiter):
                    ValidateRequiredProperty(nameof(Delimiter), Delimiter);
                    if (Delimiter?.Length > 1)
                    {
                        AddOrAppendErrors(nameof(Delimiter), $"Delimiter must be a character.");                     
                    }
                    break;
                case nameof(DataFileName):
                    ValidateRequiredProperty(nameof(DataFileName), DataFileName);                   
                    break;
            }         

            NotifyOfPropertyChange(() => IsValid);           
        }

        #endregion INotifyDataErrorInfo

        /// <summary>
        /// Validation logic for the TestDataSource
        /// </summary>
        /// <returns></returns>
        public override bool Validate()
        {
            ClearErrors("");
            ValidateProperty(nameof(Name));
            ValidateProperty(nameof(TestDataType));
            switch(DataSource)
            {
                case DataSource.Code:
                    break;
                case DataSource.CsvFile:
                    ValidateProperty(nameof(Delimiter));
                    ValidateProperty(nameof(DataFileName));
                    break;
            }              
            return IsValid;
        }
    }
}
