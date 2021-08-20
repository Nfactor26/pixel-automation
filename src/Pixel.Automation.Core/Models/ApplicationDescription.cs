using Pixel.Automation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Models
{

    [Serializable]
    [DataContract]
    public class ApplicationDescription : NotifyPropertyChanged
    {
        public string ApplicationId
        {
            get => ApplicationDetails.ApplicationId;
        }
             
        public string ApplicationName
        {
            get => ApplicationDetails.ApplicationName;
            set
            {
                ApplicationDetails.ApplicationName = value;
                OnPropertyChanged();
            }
        }

        [DataMember]
        public string ApplicationType { get; set; }

        [DataMember]
        public IApplication ApplicationDetails { get; set; }

        [DataMember]
        public List<string> AvailableControls { get; private set; } = new List<string>();
  
        public List<ControlDescription> ControlsCollection { get; set; } = new List<ControlDescription>();

        [DataMember]
        public List<string> AvailablePrefabs { get; private set; } = new List<string>();
      
        public List<PrefabProject> PrefabsCollection { get; set; } = new List<PrefabProject>();

        public ApplicationDescription()
        {

        }

        public ApplicationDescription(IApplication applicationDetails)
        {
            this.ApplicationDetails = applicationDetails;          
        }

        public void AddPrefab(PrefabProject prefabProject)
        {
            if(!this.AvailablePrefabs.Contains(prefabProject.PrefabId))
            {
                this.AvailablePrefabs.Add(prefabProject.PrefabId);
            }
            if(!this.PrefabsCollection.Contains(prefabProject))
            {
                this.PrefabsCollection.Add(prefabProject);
            }
        }

        public void DeletePrefab(PrefabProject prefabProject)
        {
            if (this.AvailablePrefabs.Contains(prefabProject.PrefabId))
            {
                this.AvailablePrefabs.Remove(prefabProject.PrefabId);
            }
            if (this.PrefabsCollection.Contains(prefabProject))
            {
                this.PrefabsCollection.Remove(prefabProject);
            }
        }

        public void AddControl(ControlDescription controlDescription)
        {
            if (!this.AvailableControls.Contains(controlDescription.ControlId))
            {
                this.AvailableControls.Add(controlDescription.ControlId);
            }
            if (!this.ControlsCollection.Contains(controlDescription))
            {
                this.ControlsCollection.Add(controlDescription);
            }
        }

        public void DeleteControl(ControlDescription controlDescription)
        {
            if (this.AvailableControls.Contains(controlDescription.ControlId))
            {
                this.AvailableControls.Remove(controlDescription.ControlId);
            }
            if (this.ControlsCollection.Contains(controlDescription))
            {
                this.ControlsCollection.Remove(controlDescription);
            }
        }
    }
}
