using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Editor.Core.Interfaces;
using Serilog;

namespace Pixel.Automation.Designer.ViewModels.Shell
{
    public class MainWindowViewModel : Conductor<IShell>.Collection.OneActive
    {
        private readonly ILogger logger = Log.ForContext<MainWindowViewModel>();       
        public BindableCollection<IFlyOut> FlyOuts { get; } = new BindableCollection<IFlyOut>();

        public MainWindowViewModel(IEnumerable<IShell> shells, IEnumerable<IFlyOut> flyOuts)
        {
            Guard.Argument(shells, nameof(shells)).NotNull().NotEmpty();
            Guard.Argument(flyOuts, nameof(flyOuts)).NotNull().NotEmpty();

            this.DisplayName = "Pixel Automation";

            this.Items.AddRange(shells);
            this.FlyOuts.AddRange(flyOuts);                  
            _ = this.ActivateItemAsync(this.Items.OfType<DesignerWindowViewModel>().FirstOrDefault());           
        }

        public void ToggleFlyout(int index)
        {
            var flyout = this.FlyOuts[index];
            flyout.IsOpen = !flyout.IsOpen;
        }
    }
}
