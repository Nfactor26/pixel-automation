using Pixel.Automation.Core;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using Serilog;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Pixel.Automation.AppExplorer.ViewModels.PrefabBuilder
{
    public class NewPrefabViewModel : StagedSmartScreen
    {
        private readonly ILogger logger = Log.ForContext<NewPrefabViewModel>();

        private ApplicationDescription applicationDescription;
        private PrefabProject prefabProject;

        public string PrefabName
        {
            get => prefabProject.PrefabName;
            set
            {
                prefabProject.PrefabName = value.Trim();
                prefabProject.NameSpace = $"{Constants.PrefabDataModelName}.{this.prefabProject.GetPrefabName()}";
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



        public NewPrefabViewModel(ApplicationDescription applicationDescription, PrefabProject prefabToolBoxItem)
        {
            this.applicationDescription = applicationDescription;
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

        Regex isValidNameSpace = new Regex(@"([A-Za-z_]{1,})((\.){1}([A-Za-z_]{1,}))*", RegexOptions.Compiled);
        private void ValidateProperty(string propertyName)
        {            
            ClearErrors(propertyName);
            switch (propertyName)
            {
                case nameof(PrefabName):
                    ValidateRequiredProperty(nameof(PrefabName), PrefabName);
                    if(!isValidNameSpace.IsMatch(PrefabName))
                    {
                        AddOrAppendErrors(nameof(PrefabName), $"Value is not in expected format. Only characters, underscore and dots are allowed");
                    }
                    if (this.applicationDescription.PrefabsCollection.Any(p => p.PrefabName.Equals(PrefabName)))
                    {
                        AddOrAppendErrors(nameof(PrefabName), $"Prefab with name {PrefabName} already exists for application {this.applicationDescription.ApplicationName}");                  
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
