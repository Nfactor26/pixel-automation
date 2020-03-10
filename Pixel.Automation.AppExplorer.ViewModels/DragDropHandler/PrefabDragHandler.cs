using GongSolutions.Wpf.DragDrop;
using Pixel.Automation.AppExplorer.ViewModels.Prefab;
using Pixel.Automation.Core;
using Serilog;
using System;
using System.Windows;

namespace Pixel.Automation.AppExplorer.ViewModels.DragDropHandler
{
    public class PrefabDragHandler : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data != null)
            {
                if (dropInfo.Data is Entity)
                {
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                    dropInfo.Effects = DragDropEffects.Copy;
                }
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            try
            {
                if (dropInfo.Data is Entity sourceItem)
                {
                    var prefabExplorer = (dropInfo.VisualTarget as FrameworkElement).DataContext as PrefabExplorerViewModel;
                    prefabExplorer.CreatePrefab(dropInfo.Data as Entity);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }
        }
    }
}
