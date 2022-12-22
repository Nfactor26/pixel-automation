using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Models;
using System.Collections.ObjectModel;

namespace Pixel.Automation.Designer.ViewModels.VersionManager
{
    /// <summary>
    /// View Model wrapper for <see cref="ControlReference"/>
    /// </summary>
    public class ControlReferenceViewModel : NotifyPropertyChanged
    {
        internal readonly ControlReference controlReference;       

        /// <summary>
        /// Name of the Control
        /// </summary>
        public string ControlName { get; private set; }

        /// <summary>
        /// Version of control used in current version of project
        /// </summary>
        public string VersionInUse { get; private set; }

        private Version selectedVersion;
        /// <summary>
        /// Select version of control on view
        /// </summary>
        public Version SelectedVersion
        {
            get => this.selectedVersion;
            set
            {
                if(this.selectedVersion != value)
                {
                    this.selectedVersion = value;
                    this.controlReference.Version = value;
                    this.IsDirty = true;
                }                
                OnPropertyChanged();
            }
        }

        // <summary>
        /// Indicate if version was changed
        /// </summary>
        public bool IsDirty { get; private set; }

        /// <summary>
        /// Available versions of Control
        /// </summary>
        public ObservableCollection<Version> AvailableVersions { get; private set; } = new();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="controlReference"></param>
        /// <param name="controlName"></param>
        /// <param name="versionInUse"></param>
        /// <param name="availableVersions"></param>
        public ControlReferenceViewModel(ControlReference controlReference, IEnumerable<ControlDescription> controls)
        {
            Guard.Argument(controls, nameof(controls)).NotNull().NotEmpty();
            this.controlReference = Guard.Argument(controlReference, nameof(controlReference)).NotNull();
            var controlInUse = controls.FirstOrDefault(a => a.Version.Equals(controlReference.Version));          
            this.ControlName = controlInUse.ControlName;
            this.VersionInUse = controlReference.Version.ToString();
            foreach (var version in controls.Select(s => s.Version))
            {
                this.AvailableVersions.Add(version);
            }
            this.SelectedVersion = this.AvailableVersions.FirstOrDefault(a => a.ToString().Equals(this.VersionInUse)) ?? this.AvailableVersions.First();
            this.IsDirty = false;
        }
    }
}
