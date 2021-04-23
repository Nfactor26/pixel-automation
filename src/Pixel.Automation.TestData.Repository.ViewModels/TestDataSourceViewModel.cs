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
    public class TestDataSourceViewModel : StagedSmartScreen
    {
        private readonly IWindowManager windowManager;
        private readonly IProjectFileSystem fileSystem;
        private readonly IArgumentTypeBrowser typeBrowser;
        private readonly IEnumerable<string> existingDataSources;
       
        public bool IsInEditMode { get;}

        public TestDataSource TestDataSource { get; }

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

        public DataSourceConfiguration MetaData
        {
            get => TestDataSource.MetaData;      
            set
            {
                TestDataSource.MetaData = value;
                NotifyOfPropertyChange(() => MetaData);
            }
        }     
        
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

        public TypeDefinition TypeDefinition { get; private set; }

        public TestDataSourceViewModel(IWindowManager windowManager, IProjectFileSystem fileSystem, DataSource dataSource,
           IEnumerable<string>  existingDataSources, IArgumentTypeBrowser typeBrowser)
        {          
            this.windowManager = Guard.Argument(windowManager, nameof(windowManager)).NotNull().Value;
            this.typeBrowser = Guard.Argument(typeBrowser, nameof(typeBrowser)).NotNull().Value;
            this.fileSystem = Guard.Argument(fileSystem, nameof(fileSystem)).NotNull().Value;
       
            this.TestDataSource = new TestDataSource() { Id = Guid.NewGuid().ToString() };
            this.TestDataSource.ScriptFile = Path.GetRelativePath(fileSystem.WorkingDirectory, Path.Combine(fileSystem.TestDataRepository, $"{this.TestDataSource.Id}.csx"));
            this.DataSource = dataSource;
            this.existingDataSources = existingDataSources;
        }

        public TestDataSourceViewModel(IWindowManager windowManager, IProjectFileSystem fileSystem, TestDataSource dataSource)
        {           
            this.windowManager = Guard.Argument(windowManager, nameof(windowManager)).NotNull().Value;
            this.fileSystem = Guard.Argument(fileSystem, nameof(fileSystem)).NotNull().Value;
            this.TestDataSource = Guard.Argument(dataSource, nameof(dataSource)).NotNull().Value;
            this.IsInEditMode = true;
            NotifyOfPropertyChange(() => CanSelectTestDataType);
        }


        public bool CanSelectTestDataType => !this.IsInEditMode;

        public async void SelectTestDataType()
        {
            var result  = await windowManager.ShowDialogAsync(typeBrowser);
            if(result.HasValue && result.Value)
            {
                this.TypeDefinition = typeBrowser.GetCreatedType();
                this.TestDataType = this.TypeDefinition.DisplayName;
            }
        }

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


        public override bool TryProcessStage(out string errorDescription)
        {
            errorDescription = string.Empty;
            return true;
        }

        public override object GetProcessedResult()
        {
            return this.TestDataSource;
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
