using GongSolutions.Wpf.DragDrop;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.TestExplorer.ViewModels;
using Serilog;
using System;
using System.Windows;

namespace Pixel.Automation.TestExplorer.Views
{
    public class TestDataSourceDropHandler : IDropTarget
    {
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
            try
            {
                if (dropInfo.Data is TestDataSource testDataSource)
                {
                    var testCase = (dropInfo.VisualTarget as FrameworkElement).DataContext as TestCaseViewModel;
                    testCase.SetTestDataSource(testDataSource.DataSourceId);             
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }
        }
    }
}
