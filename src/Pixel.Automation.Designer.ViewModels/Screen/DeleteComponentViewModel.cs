using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Automation.Editor.Core.ViewModels;
using Serilog;
using System.IO;
using Screen = Caliburn.Micro.Screen;

namespace Pixel.Automation.Designer.ViewModels
{
    /// <summary>
    /// Delete Component View is present as a popup window while deleting a component.
    /// It serves as a confirmation and allows marking if scripts associated with component should be deleted as well.
    /// </summary>
    public class DeleteComponentViewModel : Screen
    {
        private readonly ILogger logger = Log.ForContext<DeleteComponentViewModel>();
        private readonly IProjectManager projectManager;

        /// <summary>
        /// Component being deleted
        /// </summary>
        private readonly ComponentViewModel componentViewModel;
        
        /// <summary>
        /// Scripts associated with the component
        /// </summary>
        public BindableCollection<SelectableItem<string>> Scripts { get; private set; } = new ();

        /// <summary>
        /// Indicates if there are any script files associated with the component to delete or it's descendants
        /// </summary>
        public bool HasScripts
        {
            get => this.Scripts.Any();
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="componentViewModel"></param>
        /// <param name="scripts"></param>
        public DeleteComponentViewModel(ComponentViewModel componentViewModel, IEnumerable<ScriptStatus> scripts, IProjectManager projectManager)
        {
            this.DisplayName = "Delete Component";
            this.componentViewModel = Guard.Argument(componentViewModel, nameof(componentViewModel)).NotNull();
            Guard.Argument(scripts).NotNull();
            this.Scripts.AddRange(scripts.Select(s => new SelectableItem<string>(s.ScriptName, true)));
            this.projectManager = Guard.Argument(projectManager, nameof(projectManager)).NotNull().Value;
        }

        /// <summary>
        /// Handler for click event of Delete button
        /// </summary>
        /// <returns></returns>
        public async Task Delete()
        {
            var fileSystem = this.componentViewModel.Model.EntityManager.GetCurrentFileSystem();
            foreach (var script in Scripts)
            {
                try
                {
                    if(script.IsSelected)
                    {
                        await this.projectManager.DeleteDataFileAsync(Path.Combine(fileSystem.WorkingDirectory, script.Item));
                        logger.Information("Script File '{0}' was deleted", script.Item);

                    }                    
                }
                catch (Exception ex)
                {
                    logger.Error($"There was an error while trying to delete file : {script.Item}", ex);
                }
            }
            componentViewModel.Parent.RemoveComponent(componentViewModel);
            await this.TryCloseAsync(true);         
        }

        /// <summary>
        /// Handler for click event of Cancel button
        /// </summary>
        /// <returns></returns>
        public async Task Cancel()
        {
            await this.TryCloseAsync(false);
        }
    }

}
