using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace Pixel.Automation.Designer.ViewModels.VersionManager
{
    public class PrefabReferenceViewModel : NotifyPropertyChanged
    {
        internal readonly PrefabReference prefabReference;

        public string PrefabName { get; private set; }

        public string VersionInUse { get; private set; }

        private PrefabVersion selectedVersion;
        public PrefabVersion SelectedVersion
        {
            get => this.selectedVersion;
            set
            {
                if(this.selectedVersion != value)
                {
                    this.selectedVersion = value;
                    this.prefabReference.Version = value;
                    this.IsDirty = true;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsDirty { get; private set; }

        public ObservableCollection<PrefabVersion> AvailableVersions { get; private set; } = new();

        public PrefabReferenceViewModel(PrefabProject prefabProject, PrefabReference prefabReference)
        {
            Guard.Argument(prefabProject).NotNull();
            this.prefabReference = Guard.Argument(prefabReference).NotNull();
            this.PrefabName = prefabProject.PrefabName;
            this.VersionInUse = prefabReference.Version.ToString();
            foreach (var version in prefabProject.PublishedVersions)
            {
                this.AvailableVersions.Add(version);
            }
            this.SelectedVersion = this.AvailableVersions.FirstOrDefault(a => a.Equals(prefabReference.Version))
                ?? this.AvailableVersions.First();
            this.IsDirty = false;
        }
    }
}
