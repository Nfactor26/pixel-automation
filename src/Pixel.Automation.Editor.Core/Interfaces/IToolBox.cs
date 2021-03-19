using System.Windows.Input;

namespace Pixel.Automation.Editor.Core
{
    //Borrowed from Gemini Framework
    public enum PaneLocation
    {
        Left,
        Right,
        Bottom
    }

    //Borrowed from Gemini Framework
    public interface IToolBox
    {
        string DisplayName { get; set; }
        PaneLocation PreferredLocation { get; }
        double PreferredWidth { get; }
        double PreferredHeight { get; }
        bool IsActiveItem { get; set; }
        bool IsVisible { get; set; }         
        bool IsSelected { get; set; }
        ICommand CloseCommand { get; }
    }
   
}
