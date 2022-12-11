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

        public virtual async Task GoToNext()
        {
            var currentStage = ActiveItem as IStagedScreen;
            if (currentStage.Validate())
            {
                bool success = await currentStage.TryProcessStage();
                if (success && currentStage.NextScreen != null)
                {
                    await currentStage.OnNextScreen();                   
                    await ActivateItemAsync(currentStage.NextScreen, CancellationToken.None);
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

        public virtual async Task GoToPrevious()
        {
            var currentStage = ActiveItem as IStagedScreen;
            if (currentStage.PreviousScreen != null)
            {
                await currentStage.OnPreviousScreen();
                await ActivateItemAsync(currentStage.PreviousScreen, CancellationToken.None);
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
            if (screen.Validate())
            {
                bool success = await screen.TryProcessStage();
                if(success)
                {
                    foreach (var stagedScreen in this.stagedScreens)
                    {
                        await stagedScreen.OnFinished();
                    }
                }              
                await this.TryCloseAsync(true);
            }
        }

        public virtual async Task Cancel()
        {
            foreach (var stagedScreen in this.stagedScreens)
            {
                await stagedScreen.OnCancelled();
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
