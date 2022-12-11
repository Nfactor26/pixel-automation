using System.Threading.Tasks;

namespace Pixel.Automation.Editor.Core;

public abstract class StagedSmartScreen : SmartScreen, IStagedScreen
{
    public IStagedScreen NextScreen
    {
        get; set;
    }

    public IStagedScreen PreviousScreen
    {
        get; set;
    }

    public virtual bool IsValid
    {
        get
        {
            return !HasErrors;
        }
    }

    public abstract object GetProcessedResult();

    public virtual async Task OnCancelled()
    {
        await Task.CompletedTask;
    }
 
    public virtual async Task OnFinished()
    {
        await Task.CompletedTask;
    }

    public virtual async Task OnNextScreen()
    {
        await Task.CompletedTask;
    }

    public virtual async Task OnPreviousScreen()
    {
        await Task.CompletedTask;
    }

    public virtual async Task<bool> TryProcessStage()
    {
        return await Task.FromResult(true);
    }
   
    public virtual bool Validate()
    {
        return IsValid;
    }
}
