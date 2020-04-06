using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
using System.Linq;

namespace Pixel.Automation.AppExplorer.ViewModels.PrefabBuilder
{
    public class NewPrefabViewModel : StagedSmartScreen
    {
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
                NotifyOfPropertyChange(NameSpace);
                ValidateProperty(nameof(PrefabName));
            }
        }

        public string NameSpace
        {
            get => prefabDescription.NameSpace;           
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
            return Validate();
        }

        public override object GetProcessedResult()
        {
            return this.prefabDescription;
        }

        #region INotifyDataErrorInfo


        private void ValidateProperty(string propertyName)
        {            
            ClearErrors(propertyName);
            switch (propertyName)
            {
                case nameof(PrefabName):
                    ValidateRequiredProperty(nameof(PrefabName), PrefabName);
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
