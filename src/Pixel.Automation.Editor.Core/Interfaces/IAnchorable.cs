using System.Windows.Input;

namespace Pixel.Automation.Editor.Core
{
  
    /// <summary>
    /// Preferred location of the <see cref="IAnchorable"/> pane
    /// </summary>
    public enum PaneLocation
    {
        Left,
        Right,
        Bottom
    }
  
    /// <summary>
    /// Contract to be implemented by dockable screens
    /// </summary>
    public interface IAnchorable
    {
        /// <summary>
        /// Display Name for the anchorable pane
        /// </summary>
        string DisplayName { get; set; }
        
        /// <summary>
        /// Prefererred docking location of the anchorable pane in windows 
        /// </summary>
        PaneLocation PreferredLocation { get; }
    
        /// <summary>
        /// Preferred width of the anchorable pane
        /// </summary>
        double PreferredWidth { get; }

        /// <summary>
        /// Preferred height of the anchorable pane
        /// </summary>
        double PreferredHeight { get; }     

        /// <summary>
        /// Indicates whether the anchorable pane is visible
        /// </summary>
        bool IsVisible { get; set; }         
        
        /// <summary>
        /// Indicataes whether the anchorable pane is selected on screen
        /// </summary>
        bool IsSelected { get; set; }

        /// <summary>
        /// Close command for the anchorable pane
        /// </summary>
        ICommand CloseCommand { get; }
    }
   
}
