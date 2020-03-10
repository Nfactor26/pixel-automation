using Caliburn.Micro;

namespace Pixel.Automation.Editor.Core
{
    public interface IStagedScreen : IScreen
    {
        IStagedScreen NextScreen { get; set; }

        IStagedScreen PreviousScreen { get; set; }

        bool IsValid { get; }

        bool TryProcessStage(out string errorDescription);

        object GetProcessedResult();

        bool Validate();
    }
}
