using Pixel.Automation.Core.Models;
using System.Threading.Tasks;

namespace Pixel.Automation.Designer.ViewModels
{
    public interface INewProject
    {
        AutomationProject NewProject
        {
            get;           
        }

        Task CreateNewProject();
    }
}
