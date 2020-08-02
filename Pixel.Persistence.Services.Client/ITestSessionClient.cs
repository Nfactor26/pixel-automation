using Pixel.Persistence.Core.Models;
using System.Threading.Tasks;

namespace Pixel.Persistence.Services.Client
{
    public interface ITestSessionClient
    {
        /// <summary>
        /// Save details of the TestSession in to Db
        /// </summary>
        /// <param name="testSession"></param>
        /// <returns></returns>
        Task AddSession(TestSession testSession);
    }
}
