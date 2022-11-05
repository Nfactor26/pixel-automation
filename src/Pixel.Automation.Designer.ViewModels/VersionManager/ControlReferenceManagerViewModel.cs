using Dawn;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client;
using Pixel.Scripting.Reference.Manager;
using Pixel.Scripting.Reference.Manager.Contracts;
using System.Collections.ObjectModel;

namespace Pixel.Automation.Designer.ViewModels.VersionManager
{
    /// <summary>
    /// Control Reference Manager allows to manage the version of control used in a project. Multiple revisions of same control can be created over the life time of project.
    /// This screen will allow to manage revisions of control used in a given version of project.
    /// </summary>
    public class ControlReferenceManagerViewModel : Caliburn.Micro.Screen, IVersionManager
    {       
        private readonly IReferenceManager referenceManager;
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
        public ControlReferenceManagerViewModel(IApplicationDataManager applicationDataManager, IReferenceManager referenceManager)
        {
            this.DisplayName = "Manage Control References";          
            this.referenceManager = Guard.Argument(referenceManager, nameof(referenceManager)).NotNull().Value;
            this.controlReferences = this.referenceManager.GetControlReferences();
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
        public async Task SaveAsync()
        {
            var modifiedReferences = this.References.Where(r => r.IsDirty).Select(r => r.controlReference) ?? Enumerable.Empty<ControlReference>();
            if(modifiedReferences.Any())
            {
                await this.referenceManager.UpdateControlReferencesAsync(modifiedReferences);
            }
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
