namespace Pixel.Automation.Editor.Core
{
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

        public abstract bool TryProcessStage(out string errorDescription);
       
        public virtual bool Validate()
        {
            return IsValid;
        }
    }
}
