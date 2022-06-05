using Dawn;
using IdentityModel.Client;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Serilog;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Pixel.OpenId.Authenticator
{
    /// <summary>
    /// An OpenID based sign in manager used to authenticate users and control access to application.
    /// Use with clients configured for Client Credentials Flow for machine to machine communication
    /// </summary>
    public class ClientCredentialSignInManager : ISignInManager
    {
        private readonly ILogger logger = Log.ForContext<ClientCredentialSignInManager>();
        private readonly ApplicationSettings applicationSettings;

        private UserIdentity userIdentity;
        private HttpMessageHandler authenticationHandler;

        public event AsyncEventHandler<SignInCompletedEventArgs> SignInCompletedAsync;


        public ClientCredentialSignInManager(ApplicationSettings applicationSettings)
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
            return IsUserSignedIn();
        }

        public async Task SignInAsync(params object[] signinParameters)
        {
            logger.Information("Sign in has been initiated");
            var client  = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync(applicationSettings.OpenIdConnectSettings.Authority);
            var tokenClient = new TokenClient(client, new TokenClientOptions()
            {
                ClientId = applicationSettings.OpenIdConnectSettings.ClientId,
                ClientSecret = applicationSettings.OpenIdConnectSettings.ClientSecret,
                Address = disco.TokenEndpoint
            });
       
            var tokenResponse = await tokenClient.RequestClientCredentialsTokenAsync(applicationSettings.OpenIdConnectSettings.Scope);
            this.userIdentity = new UserIdentity(applicationSettings.OpenIdConnectSettings.ClientId, new System.Security.Claims.ClaimsPrincipal());
            this.authenticationHandler = new ClientCredentialTokenDelegatingHandler(this.applicationSettings, tokenResponse)
            {
                InnerHandler = new HttpClientHandler()
            };
            logger.Information("Sign in completed with result : {0}", !tokenResponse.IsError);
            if (!tokenResponse.IsError && this.SignInCompletedAsync is not null)
            {
                await this.SignInCompletedAsync(this, new SignInCompletedEventArgs(!tokenResponse.IsError, tokenResponse.Error));
            }
        }

        public Task SignOutAsync(params object[] signoutParameters)
        {
            throw new NotImplementedException();
        }
    }   

    public class ClientCredentialTokenDelegatingHandler : DelegatingHandler
    {
        private readonly ApplicationSettings applicationSettings;
        private TokenResponse accessToken;

        public ClientCredentialTokenDelegatingHandler(ApplicationSettings applicationSettings, TokenResponse accessToken)
        {
            this.applicationSettings = Guard.Argument(applicationSettings).NotNull();
            this.accessToken = Guard.Argument(accessToken).NotNull();            
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.AccessToken);
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
           
            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                return response;
            }

            //TODO : Test runner doesn't uses parallel http requests at the momement. However, we should still consider
            //some locks here to control entry for accessing tokens and refreshing them
            if (await RefreshTokensAsync(cancellationToken) == false)
            {
                return response;
            }

            response.Dispose(); // This 401 response will not be used for anything so is disposed to unblock the socket.

            //logger.Information("Access token was refreshed");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.AccessToken);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        private async Task<bool> RefreshTokensAsync(CancellationToken cancellationToken)
        {
            var tokenClient = await CreateTokenClient();
            accessToken = await tokenClient.RequestClientCredentialsTokenAsync("openid profile roles email offline_access persistence-api");
            return !accessToken.IsError;
        }

        private async Task<TokenClient> CreateTokenClient()
        {
            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync(applicationSettings.OpenIdConnectSettings.Authority);
            return  new TokenClient(client, new TokenClientOptions()
            {
                ClientId = applicationSettings.OpenIdConnectSettings.ClientId,
                ClientSecret = applicationSettings.OpenIdConnectSettings.ClientSecret,
                Address = disco.TokenEndpoint
            });
        }
    }
}
