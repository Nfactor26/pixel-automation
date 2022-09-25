using Caliburn.Micro;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Automation.Editor.Core
{
    public abstract class Wizard : Conductor<IScreen>.Collection.OneActive
    {
        protected readonly List<IStagedScreen> stagedScreens = new List<IStagedScreen>();
      
        public bool HasNext
        {
            get => (this.ActiveItem as IStagedScreen).NextScreen != null;
        }

        public virtual bool CanGoToNext
        {
            get
            {
                var current = this.ActiveItem as IStagedScreen;
                return current.IsValid && current.NextScreen != null;
            }
        }

        public virtual void GoToNext()
        {
            var currentStage = ActiveItem as IStagedScreen;
            if (currentStage.Validate() && currentStage.TryProcessStage(out string errorDescription))
            {
                if (currentStage.NextScreen != null)
                {
                    currentStage.OnNextScreen();                   
                    ActivateItemAsync(currentStage.NextScreen, CancellationToken.None);
                    if (!string.IsNullOrEmpty(currentStage.NextScreen.DisplayName))
                    {
                        this.DisplayName = currentStage.NextScreen.DisplayName;
                    }
                    NotifyOfPropertyChange(() => HasNext);
                    NotifyOfPropertyChange(() => CanGoToPrevious);
                    NotifyOfPropertyChange(() => CanGoToNext);
                    NotifyOfPropertyChange(() => CanFinish);                    
                }               
            }
        }

        public virtual bool CanGoToPrevious
        {
            get
            {
                return (this.ActiveItem as IStagedScreen).PreviousScreen != null;
            }
        }

        public virtual void GoToPrevious()
        {
            var currentStage = ActiveItem as IStagedScreen;
            if (currentStage.PreviousScreen != null)
            {
                currentStage.OnPreviousScreen();
                ActivateItemAsync(currentStage.PreviousScreen, CancellationToken.None);
                if (!string.IsNullOrEmpty(currentStage.PreviousScreen.DisplayName))
                {
                    this.DisplayName = currentStage.PreviousScreen.DisplayName;
                }
                NotifyOfPropertyChange(() => HasNext);
                NotifyOfPropertyChange(() => CanGoToPrevious);
                NotifyOfPropertyChange(() => CanGoToNext);
                NotifyOfPropertyChange(() => CanFinish);                
            }
        }

        public virtual bool CanFinish
        {
            get
            {
                if (this.stagedScreens.Last() == (this.ActiveItem as IStagedScreen))
                {
                    foreach (IStagedScreen stagedScreen in this.stagedScreens)
                    {
                        if (stagedScreen.IsValid)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public virtual async Task Finish()
        {
            IStagedScreen screen = this.ActiveItem as IStagedScreen;
            if (screen.Validate() && screen.TryProcessStage(out string errorDescription))
            {
                foreach(var stagedScreen in this.stagedScreens)
                {
                    stagedScreen.OnFinished();
                }
                await this.TryCloseAsync(true);
            }
        }

        public virtual async Task Cancel()
        {
            foreach (var stagedScreen in this.stagedScreens)
            {
                stagedScreen.OnCancelled();
            }
            await this.TryCloseAsync(false);
        }

        public virtual void DismissErrorsPanel()
        {
            (this.ActiveItem as SmartScreen)?.DismissModelErrors();
        }


        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {        
            if (this.ActiveItem == null)
            {
                await ActivateItemAsync(this.stagedScreens.First(), CancellationToken.None);
            }
            await base.OnActivateAsync(cancellationToken);
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {       
            foreach (var screen in this.stagedScreens)
            {
                await screen.TryCloseAsync(true);
            }
            await base.OnDeactivateAsync(close, cancellationToken);
        }

    }
}
