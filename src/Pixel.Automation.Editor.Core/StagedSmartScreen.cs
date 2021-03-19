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

        public virtual void OnCancelled()
        {
           
        }

        public virtual void OnFinished()
        {
           
        }

        public virtual void OnNextScreen()
        {
            
        }

        public virtual void OnPreviousScreen()
        {
          
        }

        public abstract bool TryProcessStage(out string errorDescription);
       
        public virtual bool Validate()
        {
            return IsValid;
        }


    }
}
