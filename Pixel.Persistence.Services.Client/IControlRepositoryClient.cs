using Pixel.Automation.Core.Models;
using Pixel.Persistence.Core.Models;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public interface IControlRepositoryClient
    {
        Task<byte[]> GetControls(GetControlDataForApplicationRequest controlDataRequest);
        Task<string> AddOrUpdateControl(ControlDescription controlDescription, string controlFile);
        Task<string> AddOrUpdateControlImage(ControlDescription controlDescription, string imageFile, string resolution);
    }
}
