using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core.Controls;

namespace Pixel.Automation.AppExplorer.ViewModels.Control
{
    public class ControlVersionPickerViewModel : Screen
    {
        /// <summary>
        /// Collection of PrefabVersion that can be edited.
        /// </summary>
        public BindableCollection<Version> EditableVersions { get; private set; } = new();

        /// <summary>
        /// Selected Version to be edited on the UI
        /// </summary>
        public Version SelectedVersion { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="prefabProject"></param>
        public ControlVersionPickerViewModel(IEnumerable<ControlDescription> controls)
        {
            Guard.Argument(controls, nameof(controls)).NotNull();
            this.DisplayName = "Select version to edit";
            foreach (var version in controls.Select(c => c.Version))
            {
                this.EditableVersions.Add(version);
            }
            this.SelectedVersion = this.EditableVersions.Max();
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
