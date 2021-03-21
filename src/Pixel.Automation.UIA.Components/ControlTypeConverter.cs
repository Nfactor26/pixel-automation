extern alias uiaComWrapper;
using Pixel.Automation.UIA.Components.Enums;
using System;
using uiaComWrapper::System.Windows.Automation;

namespace Pixel.Automation.UIA.Components
{
    /// <summary>
    /// Convert from a <see cref="WinControlType"/> to a <see cref="ControlType"/> and vice-versa.
    /// This is required to avoid use of controlId which is not user-friendly to change.
    /// </summary>
    public static class ControlTypeConverter
    {
        public static ControlType ToUIAControlType(this WinControlType controlType)
        {           
            switch(controlType)
            {
                case WinControlType.Button:
                    return ControlType.Button;
                case WinControlType.Separator:
                    return ControlType.Separator;
                case WinControlType.Slider:
                    return ControlType.Slider;
                case WinControlType.SplitButton:
                    return ControlType.SplitButton;
                case WinControlType.StatusBar:
                    return ControlType.StatusBar;
                case WinControlType.Tab:
                    return ControlType.Tab;
                case WinControlType.TabItem:
                    return ControlType.TabItem;
                case WinControlType.ScrollBar:
                    return ControlType.ScrollBar;
                case WinControlType.Table:
                    return ControlType.Table;
                case WinControlType.Thumb:
                    return ControlType.Thumb;
                case WinControlType.TitleBar:
                    return ControlType.TitleBar;
                case WinControlType.ToolTip:
                    return ControlType.ToolTip;
                case WinControlType.Tree:
                    return ControlType.Tree;
                case WinControlType.TreeItem:
                    return ControlType.TreeItem;
                case WinControlType.Window:
                    return ControlType.Window;
                case WinControlType.Text:
                    return ControlType.Text;
                 case WinControlType.ProgressBar:
                    return ControlType.ProgressBar;
                case WinControlType.RadioButton:
                    return ControlType.RadioButton;
                case WinControlType.MenuItem:
                    return ControlType.MenuItem;
                case WinControlType.Calendar:
                    return ControlType.Calendar;
                case WinControlType.CheckBox:
                    return ControlType.CheckBox;
                case WinControlType.ComboBox:
                    return ControlType.ComboBox;
                case WinControlType.Custom:
                    return ControlType.Custom;
                case WinControlType.DataGrid:
                    return ControlType.DataGrid;
                case WinControlType.Document:
                    return ControlType.Document;
                case WinControlType.Pane:
                    return ControlType.Pane;
                case WinControlType.Group:
                    return ControlType.Group;
                case WinControlType.Edit:
                    return ControlType.Edit;
                case WinControlType.HeaderItem:
                    return ControlType.HeaderItem;
                case WinControlType.HyperLink:
                    return ControlType.Hyperlink;
                case WinControlType.Image:
                    return ControlType.Image;
                case WinControlType.List:
                    return ControlType.List;
                case WinControlType.ListItem:
                    return ControlType.ListItem;
                case WinControlType.Menu:
                    return ControlType.Menu;
                case WinControlType.MenuBar:
                    return ControlType.MenuBar;
                case WinControlType.Header:
                    return ControlType.Header;        
            }
            throw new ArgumentException($"{controlType} can't be mapped to UIA ControlType");
        }

        public static WinControlType ToWinControlType(this ControlType controlType)
        {
            return Enum.Parse<WinControlType>(controlType.ProgrammaticName.Replace("ControlType.", ""));
        }
    }
}
