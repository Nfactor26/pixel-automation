using RestSharp;
using Pixel.Automation.Core;

namespace Pixel.Persistence.Services.Client.Interfaces
{
    /// <summary>
    /// Factory for creating a <see cref="IRestClient"/>
    /// </summary>
    public interface IRestClientFactory
    {       
        IRestClient GetOrCreateClient();
    }
}
