using RestSharp;

namespace Pixel.Persistence.Services.Client.Interfaces
{
    /// <summary>
    /// Factory for creating a <see cref="RestClient"/>
    /// </summary>
    public interface IRestClientFactory
    {       
        RestClient GetOrCreateClient();
    }
}
