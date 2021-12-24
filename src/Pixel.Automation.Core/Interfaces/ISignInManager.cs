using Pixel.Automation.Core.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Interfaces
{
    /// <summary>
    /// EventArgs for sign in completed event
    /// </summary>
    public class SignInCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Indicates whether the authentication was successful
        /// </summary>
        public bool IsSuccess { get; private set; }

        /// <summary>
        /// Error message during the authentication process
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="isSuccess"></param>
        /// <param name="errorMessage"></param>
        public SignInCompletedEventArgs(bool isSuccess, string errorMessage)
        {
            this.IsSuccess = isSuccess;
            this.ErrorMessage = errorMessage;
        }
    }

    /// <summary>
    /// ISignInManager is used to authenticate the user and get user details and
    /// check whether user is authorized to use application, etc.
    /// </summary>
    public interface ISignInManager
    {
        /// <summary>
        /// Get the signed in user details
        /// </summary>
        /// <returns></returns>
        UserIdentity GetSignedInUser();

        /// <summary>
        /// Check if the user is signed in
        /// </summary>
        /// <returns></returns>
        bool IsUserSignedIn();

        /// <summary>
        /// Check if the signed in user is authorized to use application
        /// </summary>
        /// <returns></returns>
        bool IsUserAuthorized();

        /// <summary>
        /// Get user name of the signed in user
        /// </summary>
        /// <returns></returns>
        string GetUserName();


        /// <summary>
        /// Initiate authentication
        /// </summary>
        /// <returns></returns>
        Task SignInAsync(params object[] signinParameters);

        /// <summary>
        /// Initiate sign out
        /// </summary>
        /// <returns></returns>
        Task SignOutAsync(params object[] signoutParameters);

        /// <summary>
        /// Get the <see cref="HttpMessageHandler"/> that can be attached to HttpClient to provide access token 
        /// and refresh token if required.
        /// </summary>
        /// <returns></returns>
        HttpMessageHandler GetAuthenticationHandler();

        /// <summary>
        /// Event handler for sign in completed
        /// </summary>
        event AsyncEventHandler<SignInCompletedEventArgs> SignInCompletedAsync;
    }
}
