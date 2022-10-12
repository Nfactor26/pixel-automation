using GongSolutions.Wpf.DragDrop;
using Pixel.Automation.AppExplorer.ViewModels.Prefab;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.Prefabs;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Editor.Core.ViewModels;
using Serilog;
using System.Windows;

namespace Pixel.Automation.AppExplorer.ViewModels.PrefabBuilder
{
    public class PrefabDragHandler : IDropTarget
    {
        private readonly ILogger logger = Log.ForContext<PrefabDragHandler>();
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data != null)
            {
                if (dropInfo.Data is EntityComponentViewModel ecvm)
                {
                    if ((ecvm.Model is PrefabEntity) || (ecvm.Model is Entity e
                        && e.GetFirstComponentOfType<PrefabEntity>(SearchScope.Descendants, false) != null))
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
                if (dropInfo.Data is EntityComponentViewModel sourceItem)
                {
                    var prefabExplorer = (dropInfo.VisualTarget as FrameworkElement).DataContext as PrefabExplorerViewModel;
                    _ = prefabExplorer.CreatePrefab(sourceItem.Model as Entity);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
        }
    }
}
