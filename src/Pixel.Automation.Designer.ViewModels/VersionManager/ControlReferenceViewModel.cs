using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Pixel.Automation.Designer.ViewModels.VersionManager
{
    /// <summary>
    /// View Model wrapper for <see cref="ControlReference"/>
    /// </summary>
    public class ControlReferenceViewModel : NotifyPropertyChanged
    {
        private readonly ControlReference controlReference;

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
                this.selectedVersion = value;
                this.controlReference.Version = value;
                OnPropertyChanged();
            }
        }

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
        public ControlReferenceViewModel(ControlReference controlReference, string controlName, string versionInUse, IEnumerable<Version> availableVersions)
        {
            this.controlReference = Guard.Argument(controlReference).NotNull();         
            this.ControlName = controlName;
            this.VersionInUse = versionInUse;
            foreach (var version in availableVersions)
            {
                this.AvailableVersions.Add(version);
            }
            this.SelectedVersion = this.AvailableVersions.FirstOrDefault(a => a.ToString().Equals(versionInUse)) ?? this.AvailableVersions.First();
        }
    }
}
