using Dawn;
using IdentityModel.OidcClient;
using Microsoft.Web.WebView2.Wpf;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Serilog;
using System.Net.Http;

namespace Pixel.OpenId.Authenticator
{
    /// <summary>
    /// An OpenID based sign in manager used to authenticate users and control access to application.
    /// Use with clients configured for Authorization Code Flow with Proof Key for Code Exchange (PKCE)
    /// </summary>
    public class AuthorizationCodeSignInManager : ISignInManager
    {
        private readonly ILogger logger = Log.ForContext<AuthorizationCodeSignInManager>();
        private readonly ApplicationSettings applicationSettings;
        private readonly string requiredRole = "PixelEditorUser";

        private UserIdentity userIdentity;
        private HttpMessageHandler authenticationHandler;

        public event AsyncEventHandler<SignInCompletedEventArgs> SignInCompletedAsync;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="applicationSettings"></param>
        public AuthorizationCodeSignInManager(ApplicationSettings applicationSettings)
        {
            this.applicationSettings = Guard.Argument(applicationSettings).NotNull();
        }

        public UserIdentity GetSignedInUser()
        {
            return this.userIdentity;
        }

        public HttpMessageHandler GetAuthenticationHandler()
        {
            return this.authenticationHandler ?? throw new InvalidOperationException("Authenticaton handler is not available");
        }

        public string GetUserName()
        {
            return this.userIdentity.UserName;
        }

        public bool IsUserSignedIn()
        {
            return this.userIdentity is not null;
        }

        public bool IsUserAuthorized()
        {
            return IsUserSignedIn() && this.userIdentity.ClaimsPrincipal.IsInRole(requiredRole);
        }

        public async Task SignInAsync(params object[] signinParameters)
        {          
            logger.Information("Sign in has been initiated");
            if (signinParameters.Any() && signinParameters.First() is WebView2 webView)
            {
                var options = new OidcClientOptions()
                {
                    Authority = applicationSettings.OpenIdConnectSettings.Authority,
                    ClientId = applicationSettings.OpenIdConnectSettings.ClientId,
                    Scope = applicationSettings.OpenIdConnectSettings.Scope,
                    RedirectUri = $"http://127.0.0.1/{applicationSettings.OpenIdConnectSettings.ClientId}",
                    Browser = new WebBrowser(webView),
                    Policy = new Policy
                    {
                        RequireIdentityTokenSignature = false
                    },
                    RefreshTokenInnerHttpHandler = new HttpClientHandler()
                };
                var oidcClient = new OidcClient(options);
                var result = await oidcClient.LoginAsync();
                logger.Information("Sign in completed with result : {0}", !result.IsError);
                this.authenticationHandler = result.RefreshTokenHandler;
                this.userIdentity = new UserIdentity(result.User.Identity?.Name, result.User);
                if(this.SignInCompletedAsync is not null)
                {
                    await this.SignInCompletedAsync(this, new SignInCompletedEventArgs(!result.IsError, result.Error));
                }
                return;
            }
            throw new ArgumentException("Required argument of type WebView2 was not provided.");
        }

        public async Task SignOutAsync(params object[] signoutParameters)
        {
            throw new NotImplementedException();
        }      

    }
}
