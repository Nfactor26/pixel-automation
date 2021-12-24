using Caliburn.Micro;
using Dawn;
using Microsoft.Web.WebView2.Wpf;
using Pixel.Automation.Core.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Automation.Designer.ViewModels.Shell
{
    internal class LoginWindowViewModel : Screen, IShell
    {      
        private readonly ISignInManager signInManager;
       
        public WebView2 WebView { get; private set; } = new WebView2();

        public LoginWindowViewModel(ISignInManager signInManager)
        {          
            this.signInManager = Guard.Argument(signInManager).NotNull().Value;         
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            await base.OnInitializeAsync(cancellationToken);          

            _ = this.signInManager.SignInAsync(this.WebView);
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            await base.OnDeactivateAsync(close, cancellationToken);
            if(close)
            {
                //this.WebView?.Dispose();
                this.WebView = null;
            }
        }
    }
}
