﻿using Caliburn.Micro;
using Microsoft.Win32;
using Pixel.Automation.Arguments.Editor;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core;
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
        private readonly ArgumentTypeBrowserViewModel typeBrowserWindow;
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

        public TestDataSourceViewModel(IWindowManager windowManager,IProjectFileSystem fileSystem, DataSource dataSource,
           IEnumerable<string>  existingDataSources,
            ArgumentTypeBrowserViewModel typeBrowserWindow)
        {
            this.windowManager = windowManager;
            this.typeBrowserWindow = typeBrowserWindow;
            this.fileSystem = fileSystem;
            this.TestDataSource = new TestDataSource() { Id = Guid.NewGuid().ToString() };
            this.DataSource = dataSource;
            this.existingDataSources = existingDataSources;
        }

        public TestDataSourceViewModel(IWindowManager windowManager, IProjectFileSystem fileSystem, TestDataSource dataSource, ArgumentTypeBrowserViewModel typeBrowserWindow)
        {
            this.windowManager = windowManager;
            this.typeBrowserWindow = typeBrowserWindow;
            this.fileSystem = fileSystem;          
            this.TestDataSource = dataSource;
            this.IsInEditMode = true;
        }

        public async void SelectTestDataType()
        {
            var result  = await windowManager.ShowDialogAsync(typeBrowserWindow);
            if(result.HasValue && result.Value)
            {
                this.TypeDefinition = typeBrowserWindow.SelectedType;
                this.TestDataType = typeBrowserWindow.SelectedType.DisplayName;
            }
        }

        public void BrowseForFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV files (*.csv)|*.csv";
            openFileDialog.InitialDirectory = "Automations";
            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFile = openFileDialog.FileName;
                string testDataRepository = this.fileSystem.TestDataRepository;
                string destFile = Path.Combine(testDataRepository, Path.GetFileName(selectedFile));
                //TODO : What is file already exists with same name?
                File.Copy(selectedFile, destFile, true);
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
