using Caliburn.Micro;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Models;

namespace Pixel.Automation.Designer.ViewModels
{
    /// <summary>
    /// View Model for <see cref="automationProject"/>
    /// </summary>
    public class AutomationProjectViewModel : NotifyPropertyChanged
    {
        private readonly AutomationProject automationProject;

        /// <summary>
        /// Automation Project
        /// </summary>
        public AutomationProject Project
        {
            get => automationProject;
        }

        /// <summary>
        /// Identifier of the Project
        /// </summary>
        public string ProjectId
        {
            get => automationProject.ProjectId;
        }

        /// <summary>
        /// Name of the project
        /// </summary>
        public string Name
        {
            get => automationProject.Name;
            set
            {
               automationProject.Name = value;
               OnPropertyChanged();
            }
        }

        /// <summary>
        /// Editable versions available for the project. Editable versions are those that are not yet deployed.
        /// </summary>
        public BindableCollection<ProjectVersion> EditableVersions { get; private set; } = new();

        private ProjectVersion selectedVersion;
        /// <summary>
        /// Selected version to open on the UI
        /// </summary>
        public ProjectVersion SelectedVersion
        {
            get => selectedVersion;
            set
            {
                selectedVersion = value;
                OnPropertyChanged();
            }
        }

        private bool isOpenInEditor;
        /// <summary>
        /// Indicate if the project is open in editor
        /// </summary>
        public bool IsOpenInEditor
        {
            get => this.isOpenInEditor;
            set
            {
                this.isOpenInEditor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="automationProject"></param>
        public AutomationProjectViewModel(AutomationProject automationProject)
        {
            this.automationProject = automationProject;
            this.RefreshVersions();
        }

        public void RefreshVersions()
        {
            this.EditableVersions.Clear();
            this.EditableVersions.AddRange(automationProject.ActiveVersions);
            OnPropertyChanged(nameof(EditableVersions));
            this.SelectedVersion = automationProject.LatestActiveVersion;
            OnPropertyChanged(nameof(SelectedVersion));
            this.Refresh();
        }
    }
}
