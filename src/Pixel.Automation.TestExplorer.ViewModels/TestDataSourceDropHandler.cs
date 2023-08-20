using Dawn;
using GongSolutions.Wpf.DragDrop;
using Pixel.Automation.Core;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.TestExplorer.ViewModels;
using Pixel.Persistence.Services.Client.Interfaces;
using Serilog;
using System;
using System.Diagnostics;
using System.Windows;

namespace Pixel.Automation.TestExplorer.Views
{
    public class TestDataSourceDropHandler : IDropTarget
    {
        private readonly ILogger logger = Log.ForContext<TestDataSourceDropHandler>();
        private readonly ITestCaseManager testCaseManager;

        public TestDataSourceDropHandler(ITestCaseManager testCaseManager)
        {
            this.testCaseManager = Guard.Argument(testCaseManager, nameof(testCaseManager)).NotNull().Value;
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data != null)
            {
                if (dropInfo.Data is TestDataSource)
                {
                    var testCase = (dropInfo.VisualTarget as FrameworkElement).DataContext as TestCaseViewModel;
                    if (testCase.IsOpenForEdit)
                    {
                        return;
                    }
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                    dropInfo.Effects = DragDropEffects.Copy;
                }
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            using (var activity = Telemetry.DefaultSource.StartActivity(nameof(Drop), ActivityKind.Internal))
            {
                try
                {
                    if (dropInfo.Data is TestDataSource testDataSource)
                    {
                        var testCase = (dropInfo.VisualTarget as FrameworkElement).DataContext as TestCaseViewModel;
                        testCase.SetTestDataSource(testDataSource.DataSourceId);
                        _ = this.testCaseManager.UpdateTestCaseAsync(testCase.TestCase);
                        activity?.SetTag("TestCase", testCase.DisplayName);
                        activity?.SetTag("TestDataSource", testDataSource.Name);
                        logger.Information("Assigned test data source : {0} to test case : {1}", testDataSource.Name, testCase.DisplayName);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while trying to set test data source on test case");
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                }
            }              
        }
    }
}
