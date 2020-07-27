using Pixel.Automation.Core.Models;
using Pixel.Persistence.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public interface IApplicationRepositoryClient
    {
        Task<ApplicationDescription> GetApplication(string applicationId);

        Task<IEnumerable<ApplicationMetaData>> GetMetaData();

        Task<IEnumerable<ApplicationDescription>> GetApplications(IEnumerable<ApplicationMetaData> applicationsToDownload);
        
        Task<ApplicationDescription> AddApplication(ApplicationDescription applicationDescription, string applicationFile);

        Task<ApplicationDescription> UpdateApplication(ApplicationDescription applicationDescription);

    }
}
