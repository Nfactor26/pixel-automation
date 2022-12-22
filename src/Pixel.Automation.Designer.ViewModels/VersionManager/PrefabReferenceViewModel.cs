using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Models;
using System.Collections.ObjectModel;

namespace Pixel.Automation.Designer.ViewModels.VersionManager
{ 
    /// <summary>
    /// View Model wrapper for <see cref="PrefabReference"/>
    /// </summary>
    public class PrefabReferenceViewModel : NotifyPropertyChanged
    {
        internal readonly PrefabReference prefabReference;

        /// <summary>
        /// Name of the Prefab
        /// </summary>
        public string PrefabName { get; private set; }

        /// <summary>
        /// Version of prefaab used in current version of project
        /// </summary>
        public string VersionInUse { get; private set; }

        private PrefabVersion selectedVersion;
        /// <summary>
        /// Select version of prefab on view
        /// </summary>
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

        /// <summary>
        /// Indicate if version was changed
        /// </summary>
        public bool IsDirty { get; private set; }

        /// <summary>
        /// Available versions of Prefab
        /// </summary>
        public ObservableCollection<PrefabVersion> AvailableVersions { get; private set; } = new();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="prefabProject"></param>
        /// <param name="prefabReference"></param>
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
