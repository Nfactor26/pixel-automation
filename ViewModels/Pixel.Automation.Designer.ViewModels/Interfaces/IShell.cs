using Caliburn.Micro;
using Pixel.Automation.Editor.Core;

namespace Pixel.Automation.Designer.ViewModels
{
    public interface IShell
    {
        BindableCollection<IToolBox> Tools
        {
            get;
        }
    }
}
