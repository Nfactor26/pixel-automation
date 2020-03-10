using Pixel.Automation.Core.Models;

namespace Pixel.Automation.Designer.ViewModels
{
    public interface INewProject
    {

        AutomationProject NewProject
        {
            get;
            set;
        }

        void CreateNewProject();

    }
}
