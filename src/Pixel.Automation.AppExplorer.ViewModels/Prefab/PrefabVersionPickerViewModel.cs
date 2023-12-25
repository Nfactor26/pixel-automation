using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core.Models;

namespace Pixel.Automation.AppExplorer.ViewModels.Prefab
{
    /// <summary>
    /// View model that handles selecting the version of prefab to be edited
    /// </summary>
    public class PrefabVersionPickerViewModel : Screen
    {
        /// <summary>
        /// Collection of PrefabVersion that can be edited.
        /// </summary>
        public BindableCollection<VersionInfo> EditableVersions { get; private set; } = new();

        /// <summary>
        /// Selected Version to be edited on the UI
        /// </summary>
        public VersionInfo SelectedVersion { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="prefabProject"></param>
        public PrefabVersionPickerViewModel(PrefabProject prefabProject)
        {
            Guard.Argument(prefabProject, nameof(prefabProject)).NotNull();
            this.DisplayName = "Select version to edit";
            foreach(var version in prefabProject.ActiveVersions)
            {
                this.EditableVersions.Add(version);
            }
            this.SelectedVersion = prefabProject.LatestActiveVersion;
        }

        /// <summary>
        /// Close the screen with true result
        /// </summary>
        /// <returns></returns>
        public async Task OpenAsync()
        {
            await this.TryCloseAsync(true);
        }

        /// <summary>
        /// Close the screen with false result
        /// </summary>
        /// <returns></returns>
        public async Task CloseAsync()
        {
            await this.TryCloseAsync(false);
        }
    }
}
