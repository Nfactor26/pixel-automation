using Caliburn.Micro;
using System.Threading.Tasks;

namespace Pixel.Automation.Editor.Core
{
    public interface IStagedScreen : IScreen
    {
        IStagedScreen NextScreen { get; set; }

        IStagedScreen PreviousScreen { get; set; }

        bool IsValid { get; }

        Task<bool> TryProcessStage();

        object GetProcessedResult();

        bool Validate();

        Task OnNextScreen();

        Task OnPreviousScreen();

        Task OnCancelled();

        Task OnFinished();

    }
}
