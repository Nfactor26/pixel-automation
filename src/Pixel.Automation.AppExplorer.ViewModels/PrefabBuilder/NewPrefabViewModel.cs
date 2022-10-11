using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Serilog;

namespace Pixel.Automation.AppExplorer.ViewModels.PrefabBuilder
{
    public class NewPrefabViewModel : StagedSmartScreen
    {
        private readonly ILogger logger = Log.ForContext<NewPrefabViewModel>();

        private ApplicationDescriptionViewModel applicationDescriptionViewModel;
        private PrefabProject prefabProject;

        public string PrefabName
        {
            get => prefabProject.PrefabName;
            set
            {
                prefabProject.PrefabName = value ?? string.Empty;              
                NotifyOfPropertyChange(PrefabName);           
                ValidateProperty(nameof(PrefabName));
            }
        }    

        public string GroupName
        {
            get => prefabProject.GroupName;
            set
            {
                prefabProject.GroupName = value;
                NotifyOfPropertyChange();
                ValidateProperty(nameof(GroupName));
            }
        }

        public string Description
        {
            get => prefabProject.Description;
            set
            {
                prefabProject.Description = value;
                NotifyOfPropertyChange();
                ValidateProperty(nameof(Description));
            }
        }

        public NewPrefabViewModel(ApplicationDescriptionViewModel applicationDescriptionViewModel, PrefabProject prefabToolBoxItem)
        {
            this.DisplayName = "(1/4) Create a new Prefab";
            this.applicationDescriptionViewModel = applicationDescriptionViewModel;
            this.prefabProject = prefabToolBoxItem;
            this.prefabProject.PrefabName = "Prefab";
            this.prefabProject.Description = "Description";
        }
       
        public bool CanTryProcessStage
        {
            get => IsValid;
        }

        public override bool TryProcessStage(out string errorDescription)
        {
            errorDescription = string.Empty;
            if(Validate())
            {
                this.PrefabName = this.PrefabName.Trim();
                this.prefabProject.Namespace = $"{Constants.PrefabNameSpacePrefix}.{this.PrefabName.Replace(' ', '.')}";
                logger.Information($"Prefab name is : {PrefabName}. Moving to next screen");            
                return true;
            }
            return false;
        }

        public override object GetProcessedResult()
        {
            return this.prefabProject;
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            logger.Information($"Activate screen is {nameof(NewPrefabViewModel)}");
            return base.OnActivateAsync(cancellationToken);
        }

        #region INotifyDataErrorInfo
      
        private void ValidateProperty(string propertyName)
        {            
            ClearErrors(propertyName);
            switch (propertyName)
            {
                case nameof(PrefabName):
                    ValidateRequiredProperty(nameof(PrefabName), PrefabName);
                    ValidatePattern("^([A-Za-z]|[._ ]){4,}$", nameof(PrefabName), PrefabName, "Name must contain only alphabets or ' ' or '_' and should be atleast 4 characters in length.");
                    if (this.applicationDescriptionViewModel.PrefabsCollection.Any(p => p.PrefabName.Equals(PrefabName)))
                    {
                        AddOrAppendErrors(nameof(PrefabName), $"Prefab with name {PrefabName} already exists for application {this.applicationDescriptionViewModel.ApplicationName}");                  
                    }
                    break;            
                case nameof(GroupName):
                    ValidateRequiredProperty(nameof(GroupName), GroupName);
                    break;
                case nameof(Description):
                    ValidateRequiredProperty(nameof(GroupName), GroupName);
                    break;
            }

            NotifyOfPropertyChange(() => CanTryProcessStage);
            NotifyOfPropertyChange(() => IsValid);           
        }

        #endregion INotifyDataErrorInfo
    }
}
