using ICSharpCode.AvalonEdit;
using Pixel.Scripting.Editor.Core.Models.TypeLookup;
using Pixel.Scripting.Script.Editor.Controls;
using Pixel.Scripting.Script.Editor.Model;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Pixel.Scripting.Script.Editor
{
    public partial class CodeTextEditor : TextEditor
    {
        private ToolTip toolTip = new ToolTip();
        partial void InitializeMouseHover()
        {
            MouseHover += OnMouseHover;
            MouseHoverStopped += OnHoverStopped;
        }

        private void OnHoverStopped(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(()=>
                {
                    if (toolTip != null)
                    {
                        toolTip.IsOpen = false;
                    }
                }));
           
        }

        private async void OnMouseHover(object sender, System.Windows.Input.MouseEventArgs e)
        {       
            await Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal,new Action(() => ProcessMouseOver(e)));      
        }

        private async void ProcessMouseOver(System.Windows.Input.MouseEventArgs e)
        {            
            TextViewPosition? position;
            try
            {
                position = TextArea.TextView.GetPositionFloor(e.GetPosition(TextArea.TextView) + TextArea.TextView.ScrollOffset);
            }
            catch (ArgumentOutOfRangeException)
            {
                // TODO: check why this happens
                toolTip.IsOpen = false;
                e.Handled = true;
                return;
            }
            if (!position.HasValue || position.Value.Location.IsEmpty)
            {
                toolTip.IsOpen = false;
                return;
            }

            var offset = this.Document.GetOffset(position.Value.Location);
            var markers = textMarkerService.GetMarkersAtOffset(offset);
            if (markers.Any())
            {
                var markerWithToolTip = markers.FirstOrDefault(marker => marker.ToolTip != null);
                if (markerWithToolTip != null)
                {                 
                    //toolTip = new ToolTip();
                    toolTip.Content = new DiagnosticsTextPanel() { DataContext = markerWithToolTip };
                    //toolTip.Closed += delegate
                    //{
                    //    toolTip = null;
                    //};
                    toolTip.IsOpen = true;
                    return;
                }
                return;
            }
            else
            {
                var typeDescription = await this.editorService.GetTypeDescriptionAsync(new TypeLookupRequest()
                {
                    FileName = this.Document.FileName,
                    Line = position.Value.Line - 1,
                    Column = position.Value.Column - 1,
                    IncludeDocumentation = true
                });
                if (string.IsNullOrEmpty(typeDescription.Type))
                {
                    toolTip.IsOpen = false;
                    return;
                }

                //toolTip = new ToolTip();
                toolTip.Content = new TypeDescriptionPanel() { DataContext = new TypeDescription(typeDescription) };
                //toolTip.Closed += delegate
                //{
                //    toolTip = null;
                //};
                toolTip.IsOpen = true;
                return;
            }
         
        }

    }
}
