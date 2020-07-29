using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Pixel.Automation.Core.Models;

namespace Pixel.Persistence.Services.Client
{
    public interface IApplicationDataManager
    {
        Task AddOrUpdateApplicationAsync(ApplicationDescription applicationDescription);
        
        Task DownloadApplicationsDataAsync();
       
        IEnumerable<ApplicationDescription> GetAllApplications();

        Task AddOrUpdateControlAsync(ControlDescription controlDescription);

        Task<string> AddOrUpdateControlImageAsync(ControlDescription controlDescription, Stream imageDataStream, string imageResolution);

        IEnumerable<ControlDescription> GetAllControls(ApplicationDescription applicationDescription);

    }
}