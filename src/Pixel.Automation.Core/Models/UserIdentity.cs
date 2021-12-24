using System.Security.Claims;

namespace Pixel.Automation.Core.Models
{
    /// <summary>
    /// Details of the authenticated user
    /// </summary>
    public class UserIdentity
    {
        /// <summary>
        /// Name of the user
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// Claims prinicipal associated with the user
        /// </summary>
        public ClaimsPrincipal ClaimsPrincipal { get; private set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="claimsPrincipal"></param>
        public UserIdentity(string userName, ClaimsPrincipal claimsPrincipal)
        {
            UserName = userName;
            ClaimsPrincipal = claimsPrincipal;
        }
    }
}
