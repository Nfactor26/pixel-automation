using IdentityModel.OidcClient.Browser;
using Microsoft.Web.WebView2.Wpf;

namespace Pixel.OpenId.Authenticator
{
    internal class WebBrowser : IBrowser
    {
        private readonly WebView2 webView;
       
        public WebBrowser(WebView2 webView)
        {
            this.webView = webView; 
        }

        public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
        {
            var browserResult = new BrowserResult()
            {
                ResultType = BrowserResultType.UserCancel
            };           

            using (var semaphoreSlim = new SemaphoreSlim(0, 1))
            {
                webView.NavigationStarting += (s, e) =>
                {
                    var uri = new Uri(e.Uri);
                    if (uri.AbsoluteUri.StartsWith(options.EndUrl))
                    {
                        e.Cancel = true;

                        browserResult = new BrowserResult()
                        {
                            ResultType = BrowserResultType.Success,
                            Response = new Uri(e.Uri).AbsoluteUri
                        };
                        semaphoreSlim.Release();
                    }
                };

                // Initialization
                await webView.EnsureCoreWebView2Async(null);

                // Delete existing Cookies so previous logins won't remembered
                webView.CoreWebView2.CookieManager.DeleteAllCookies();

                // Navigate
                webView.CoreWebView2.Navigate(options.StartUrl);

                await semaphoreSlim.WaitAsync();
            }
            return browserResult;
        }
    }
}
