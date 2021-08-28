using Caliburn.Micro;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Editor.Core;
using Pixel.Automation.Editor.Core.Interfaces;

namespace Pixel.Automation.Designer.ViewModels
{
    public interface IShell
    {
        BindableCollection<IAnchorable> Anchorables
        {
            get;
        }

        BindableCollection<IFlyOut> FlyOuts
        {
            get;
        }

        BindableCollection<IControlScrapper> ScreenScrappers
        {
            get;
        }
    }
}
