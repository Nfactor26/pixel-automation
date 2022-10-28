using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Editor.Core.Interfaces;
using System.Diagnostics;

namespace Pixel.Automation.Designer.ViewModels.Shell
{
    public class MainWindowViewModel : Conductor<IShell>.Collection.OneActive
    {       
        public BindableCollection<IFlyOut> FlyOuts { get; private set; } = new();

        public ApplicationSettings ApplicationSettings { get; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="shells"></param>
        /// <param name="flyOuts"></param>
        public MainWindowViewModel(IEnumerable<IShell> shells, IEnumerable<IFlyOut> flyOuts, ApplicationSettings settings)
        {
            Guard.Argument(shells, nameof(shells)).NotNull().NotEmpty();
            Guard.Argument(flyOuts, nameof(flyOuts)).NotNull().NotEmpty();

            this.DisplayName = "Pixel Automation";
            this.Items.AddRange(shells);
            this.FlyOuts.AddRange(flyOuts);
            this.ApplicationSettings = Guard.Argument(settings, nameof(settings)).NotNull();
            _ = this.ActivateItemAsync(this.Items.OfType<DesignerWindowViewModel>().FirstOrDefault());           
        }

        /// <summary>
        /// Toggle the flyout window open state
        /// </summary>
        /// <param name="index"></param>
        public void ToggleFlyout(int index)
        {
            var flyout = this.FlyOuts[index];
            flyout.IsOpen = !flyout.IsOpen;
        }

        /// <summary>
        /// Open the repository page on github in browser
        /// </summary>
        public void OpenGithubRepo()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/Nfactor26/pixel-automation",                
                UseShellExecute = true
            });
        }
    }
}
