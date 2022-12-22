using Dawn;
using Notifications.Wpf.Core;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Reference.Manager;
using Pixel.Automation.Reference.Manager.Contracts;
using Pixel.Persistence.Services.Client;
using Serilog;
using System.Collections.ObjectModel;

namespace Pixel.Automation.Designer.ViewModels.VersionManager
{
    /// <summary>
    /// Control Reference Manager allows to manage the version of control used in a project. Multiple revisions of same control can be created over the life time of project.
    /// This screen will allow to manage revisions of control used in a given version of project.
    /// </summary>
    public class ControlReferenceManagerViewModel : Caliburn.Micro.Screen, IVersionManager
    {
        private readonly ILogger logger = Log.ForContext<ControlReferenceManagerViewModel>();
        private readonly IReferenceManager referenceManager;
        private readonly INotificationManager notificationManager;
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
        public ControlReferenceManagerViewModel(IApplicationDataManager applicationDataManager, IReferenceManager referenceManager,
            INotificationManager notificationManager)
        {
            this.DisplayName = "Manage Control References";          
            this.referenceManager = Guard.Argument(referenceManager, nameof(referenceManager)).NotNull().Value;
            this.notificationManager = Guard.Argument(notificationManager, nameof(notificationManager)).NotNull().Value;
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
            try
            {
                var modifiedReferences = this.References.Where(r => r.IsDirty).Select(r => r.controlReference) ?? Enumerable.Empty<ControlReference>();
                if (modifiedReferences.Any())
                {
                    await this.referenceManager.UpdateControlReferencesAsync(modifiedReferences);
                }
                await this.TryCloseAsync(true);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to save changes to control references");
                await notificationManager.ShowErrorNotificationAsync(ex.Message);
            }
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
