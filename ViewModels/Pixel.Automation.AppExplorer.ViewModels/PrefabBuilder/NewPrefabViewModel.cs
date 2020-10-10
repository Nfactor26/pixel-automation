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
        private PrefabDescription prefabDescription;

        public string PrefabName
        {
            get => prefabDescription.PrefabName;
            set
            {
                prefabDescription.PrefabName = value.Trim();
                prefabDescription.NameSpace = "Pixel.Automation.Prefabs." + value.Trim();
                NotifyOfPropertyChange(PrefabName);           
                ValidateProperty(nameof(PrefabName));
            }
        }    

        public string GroupName
        {
            get => prefabDescription.GroupName;
            set
            {
                prefabDescription.GroupName = value;
                NotifyOfPropertyChange();
                ValidateProperty(nameof(GroupName));
            }
        }

        public string Description
        {
            get => prefabDescription.Description;
            set
            {
                prefabDescription.Description = value;
                NotifyOfPropertyChange();
                ValidateProperty(nameof(Description));
            }
        }



        public NewPrefabViewModel(ApplicationDescription applicationDescription, PrefabDescription prefabToolBoxItem)
        {
            this.applicationDescription = applicationDescription;
            this.prefabDescription = prefabToolBoxItem;
            this.prefabDescription.PrefabName = "Prefab";
            this.prefabDescription.Description = "Description";
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
            return this.prefabDescription;
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
