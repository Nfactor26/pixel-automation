using Pixel.Automation.Core.Arguments;
using Pixel.Automation.Core.Components;
using RestSharp;
using RestSharp.Authenticators.OAuth2;
using RestSharp.Authenticators;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Exceptions;
using Pixel.Automation.RestApi.Shared;

namespace Pixel.Automation.RestApi.Components;

[ImportReferencesFrom(typeof(ScriptReferencesProvider))]
public class RestApiApplicationEntity : ApplicationEntity
{
    /// <summary>
    /// Optional argument which can be used to override the base url configured on application.
    /// </summary>
    [DataMember(Order = 1000)]
    [Display(Name = "Base Url", GroupName = "Overrides", Order = 10, Description = "[Optional] Override the base url defined on application")]
    public Argument BaseUrlOverride { get; set; } = new InArgument<string>() { CanChangeType = false, Mode = ArgumentMode.Default };

    /// <summary>
    /// Custom initialized RestSharp->RestClient
    /// </summary>
    [DataMember(Order = 1010)]
    [Display(Name = "Rest Client Options", GroupName = "Overrides", Order = 20, Description = "[Optional] Custom RestClientOptions")]
    public Argument RestClientOptions { get; set; } = new InArgument<RestClientOptions>() { CanChangeType = false, AllowedModes = ArgumentMode.DataBound|ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// Argument to set authentication mode of application.
    /// </summary>
    [DataMember(Order = 1020)]
    [Display(Name = "Authentication Mode", GroupName = "Authentication", Order = 30, Description = "Authentication mode for the application")]
    public Argument AuthenticationMode { get; set; } = new InArgument<AuthenticationMode>() { CanChangeType = false, Mode = ArgumentMode.Default };

    /// <summary>
    /// Implementation of AuthenticatorBase depending on selected AuthenticationMode
    /// </summary>
    [DataMember(Order = 1030)]
    [Display(Name = "Authenticator", GroupName = "Authentication", Order = 40, Description = "Authenticator used by the RestClient")]
    [AllowedTypes(typeof(HttpBasicAuthenticator), typeof(OAuth1Authenticator), typeof(OAuth2UriQueryParameterAuthenticator), typeof(OAuth2AuthorizationRequestHeaderAuthenticator), typeof(JwtAuthenticator))]
    public Argument Authenticator { get; set; } = new InArgument<AuthenticatorBase>() { CanChangeType = false, AllowedModes = ArgumentMode.DataBound | ArgumentMode.Scripted, Mode = ArgumentMode.DataBound };

    /// <summary>
    /// When supplied, the function will be called before configuring RestRequest on http request actors.
    /// This can be used to configure common settings on all the http request actors without duplicating it on each of them.
    /// </summary>
    [DataMember(Order = 1040)]
    [Display(Name = "Configure Request", GroupName = "Extension", Order = 40, Description = "[Optional] When supplied, this function will be called before configuring RestRequest on http request actors")]
    public Argument ConfigureRequest { get; set; } = new FuncArgument<Func<RestRequest, ValueTask>>();

    /// </inheritdoc>
    public override async Task LaunchAsync()
    {
        var applicationDetails = this.GetTargetApplicationDetails<RestApiApplication>();
        
        var authenticationMode = Shared.AuthenticationMode.None;
        if (this.AuthenticationMode.IsConfigured())
        {
            authenticationMode = await this.ArgumentProcessor.GetValueAsync<AuthenticationMode>(this.AuthenticationMode);
        }

        Uri targetUri = new Uri(applicationDetails.BaseUrl);
        RestClientOptions restClientOptions;
        if (this.RestClientOptions.IsConfigured())
        {
            restClientOptions = await this.ArgumentProcessor.GetValueAsync<RestClientOptions>(this.RestClientOptions);
        }
        else
        {
            restClientOptions = new RestClientOptions($"{targetUri.Scheme}://{targetUri.Host}");
        }

        switch (authenticationMode)
        {
            case Shared.AuthenticationMode.None:
                break;
            default:
                if (this.Authenticator.IsConfigured())
                {
                    restClientOptions.Authenticator = await this.ArgumentProcessor.GetValueAsync<AuthenticatorBase>(this.Authenticator);
                    break;
                }
                throw new ConfigurationException($"Authentication mode is {authenticationMode}. However, Authenticator argument is not configured.");
        }

        RestClient restClient = new RestClient(restClientOptions);
        applicationDetails.RestClient = restClient;
    }

    /// </inheritdoc>
    public override async Task CloseAsync()
    {
        var applicationDetails = this.GetTargetApplicationDetails<RestApiApplication>();
        applicationDetails.RestClient?.Dispose();
        applicationDetails.RestClient = null;
        await Task.CompletedTask;
    }

    /// </inheritdoc>
    public override async Task CaptureScreenShotAsync(string filePath)
    {
        await Task.CompletedTask;
    }
}
