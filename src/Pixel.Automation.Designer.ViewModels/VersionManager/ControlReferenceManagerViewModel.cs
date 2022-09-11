using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client;
using System.Collections.ObjectModel;
using System.IO;

namespace Pixel.Automation.Designer.ViewModels.VersionManager
{
    /// <summary>
    /// Control Reference Manager allows to manage the version of control used in a project. Multiple revisions of same control can be created over the life time of project.
    /// This screen will allow to manage revisions of control used in a given version of project.
    /// </summary>
    public class ControlReferenceManagerViewModel : Caliburn.Micro.Screen, IVersionManager
    {
        private readonly IFileSystem fileSystem;
        public readonly ControlReferences controlReferences;

        /// <summary>
        /// Collection of <see cref="ControlReference"/> used in the project
        /// </summary>
        public ObservableCollection<ControlReferenceViewModel> References { get; private set; } = new();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="projectFileSystem"></param>
        /// <param name="applicationDataManager"></param>
        public ControlReferenceManagerViewModel(IFileSystem projectFileSystem, IApplicationDataManager applicationDataManager)
        {
            this.DisplayName = "Manage Control References";
            this.fileSystem = Guard.Argument(projectFileSystem).NotNull().Value;
            this.controlReferences = this.fileSystem.LoadFile<ControlReferences>(fileSystem.ControlReferencesFile);
            foreach(var reference in this.controlReferences.References)
            {
                var controls = applicationDataManager.GetControlsById(reference.ApplicationId, reference.ControlId);
                var controlInUse = controls.FirstOrDefault(a => a.Version.Equals(reference.Version));
                this.References.Add(new ControlReferenceViewModel( reference, controlInUse.ControlName , reference.Version.ToString(), controls.Select(s => s.Version)));
            }
        }

        /// <summary>
        /// Save all the changes done on screen.
        /// </summary>
        public async void SaveAsync()
        {
            this.fileSystem.SaveToFile<ControlReferences>(this.controlReferences, Path.GetDirectoryName(fileSystem.ControlReferencesFile), Path.GetFileName(fileSystem.ControlReferencesFile));
            await this.TryCloseAsync(true);
        }

        /// <summary>
        /// Close the screen without saving changes.
        /// </summary>
        /// <returns></returns>
        public async Task CloseAsync()
        {
            await this.TryCloseAsync(true);
        }
    }
}
