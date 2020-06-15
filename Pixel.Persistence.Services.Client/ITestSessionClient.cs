using Pixel.Persistence.Core.Models;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public interface ITestSessionClient
    {
        Task AddSession(TestSession testSession);
    }
}
