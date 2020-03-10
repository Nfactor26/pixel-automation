using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.TestData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pixel.Automation.RunTime
{
    //public class TestDataManager : ITestDataManager
    //{
    //    private readonly IProjectFileSystem fileSystem;
    //    private readonly ISerializer serializer;
    //    private readonly IDataReader[] dataReaders;
      
    //    public TestDataManager(ISerializer serializer, IProjectFileSystem fileSystem, IDataReader[] dataReaders)
    //    {
    //        Guard.Argument(serializer).NotNull();
    //        Guard.Argument(fileSystem).NotNull();
    //        Guard.Argument(dataReaders).NotNull();

    //        this.serializer = serializer;
    //        this.fileSystem = fileSystem;
    //        this.dataReaders = dataReaders;          
    //    }

    //    public IEnumerable<object> GetTestCaseData(TestCase testCase)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public object GetTestDataModelInstance(EntityManager entityManager, TestCase testCase)
    //    {
    //        Guard.Argument(testCase).NotNull();
    //        Guard.Argument(testCase.TestDataId).NotNull().NotEmpty();

    //        string repositoryFolder = this.fileSystem.TestDataRepoDirectory;                       
    //        string dataSourceFile = Path.Combine(repositoryFolder, $"{testCase.TestDataId}.dat");            
    //        if(!File.Exists(dataSourceFile))
    //        {
    //            throw new FileNotFoundException($"{dataSourceFile} doesn't exist. Can't load required data for test case");
    //        }
    //        var dataSource = serializer.Deserialize<TestDataSource>(dataSourceFile);

    //        switch(dataSource.DataSource)
    //        {
    //            case DataSource.Code:
    //                return GetCodedTestData(entityManager, dataSource);                    
    //            case DataSource.CsvFile:
    //                return GetCsvTestData(entityManager, dataSource);
    //            default:
    //                throw new ArgumentException($"DataSource {dataSource.DataSource} isn't supported");
    //        }
    //    }

    //    private object GetCodedTestData(EntityManager entityManager, TestDataSource testDataSource)
    //    {
    //        var dataModelAssembly = entityManager.Arguments.GetType().Assembly;
    //        var targetDataModelType = dataModelAssembly.GetTypes().FirstOrDefault(a => a.Name.Equals(testDataSource.Name));
    //        var dataModelInstance = Activator.CreateInstance(targetDataModelType);
    //        return dataModelInstance;
    //    }

    //    private object GetCsvTestData(EntityManager entityManager, TestDataSource testDataSource)
    //    {
    //        var dataModelAssembly = entityManager.Arguments.GetType().Assembly;
    //        var targetDataModelType = dataModelAssembly.GetTypes().FirstOrDefault(a => a.Name.Equals(testDataSource.Name));
    //        var dataModelInstance = Activator.CreateInstance(targetDataModelType);

    //        IDataReader csvDataReader = dataReaders.FirstOrDefault(r => r.CanProcessFileType("csv"));
    //        if(csvDataReader == null)
    //        {
    //            throw new Exception("No DataReader for csv file is available");
    //        }
    //        csvDataReader.Initialize(testDataSource.MetaData);
            
    //        var dataRowType = dataModelAssembly.GetTypes().FirstOrDefault(a => a.Name.Equals(testDataSource.MetaData.TargetTypeName));
    //        var getAllRowsMethod = csvDataReader.GetType().GetMethod(nameof(IDataReader.GetAllRowsAs), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
    //        var getAllRowsMethodWithType = getAllRowsMethod.MakeGenericMethod(dataRowType);

    //        var rows = getAllRowsMethodWithType.Invoke(csvDataReader, null);

    //        var testDataCollectionProperty = dataModelInstance.GetType().GetProperty("TestDataCollection", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
    //        testDataCollectionProperty.SetValue(dataModelInstance, rows);

    //        return dataModelInstance;

    //    }
    //}
}
