using Pixel.Automation.Core.Models;
using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public interface IApplicationRepositoryClient
    {
        Task<ApplicationDescription> GetApplication(string applicationId);        

        Task<IEnumerable<ApplicationDescription>> GetApplications(IEnumerable<ApplicationMetaData> applicationsToDownload);
        
        Task<ApplicationDescription> AddOrUpdateApplication(ApplicationDescription applicationDescription, string applicationFile);

    }
}
