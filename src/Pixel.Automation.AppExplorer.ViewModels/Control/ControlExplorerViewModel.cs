using Caliburn.Micro;
using Dawn;
using Microsoft.Win32;
using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.AppExplorer.ViewModels.Contracts;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client;
using Serilog;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Pixel.Automation.AppExplorer.ViewModels.Control
{
    /// <summary>
    /// View model for displaying and managing controls belonging to an applicatoin
    /// </summary>
    public class ControlExplorerViewModel : Screen, IApplicationAware, IHandle<IEnumerable<ScrapedControl>>
    {
        private readonly ILogger logger = Log.ForContext<ControlExplorerViewModel>();
        private readonly IControlEditorFactory controlEditorFactory;
        private readonly IEventAggregator eventAggregator;
        private readonly IWindowManager windowManager;
        private readonly IApplicationDataManager applicationDataManager;

        private ApplicationDescriptionViewModel activeApplication;

        public ApplicationDescriptionViewModel ActiveApplication
        {
            get => this.activeApplication;
            set
            {
                this.activeApplication = value;
                NotifyOfPropertyChange();
            }
        }


        public BindableCollection<string> Screens { get; set; } = new BindableCollection<string>();

        private string selectedScreen;
        public string SelectedScreen
        {
            get => selectedScreen;
            set
            {
                selectedScreen = value;
                NotifyOfPropertyChange(() => SelectedScreen);
                var controlsForSelectedScreen = LoadControlDetails(this.ActiveApplication, value);
                this.Controls.Clear();
                this.Controls.AddRange(controlsForSelectedScreen);
            }
        }

        /// <summary>
        /// Controls belonging to the active application
        /// </summary>
        public BindableCollection<ControlDescriptionViewModel> Controls { get; set; } = new BindableCollection<ControlDescriptionViewModel>();

        private ControlDescriptionViewModel selectedControl;
        /// <summary>
        /// Selected control on the view
        /// </summary>
        public ControlDescriptionViewModel SelectedControl
        {
            get => selectedControl;
            set
            {
                selectedControl = value;
                CanEdit = false;
                //Notification for property grid to display selected application details
                this.eventAggregator.PublishOnUIThreadAsync(new PropertyGridObjectEventArgs(value, true));
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="windowManager"></param>
        /// <param name="eventAggregator"></param>
        /// <param name="controlEditor"></param>
        /// <param name="applicationDataManager"></param>
        public ControlExplorerViewModel(IWindowManager windowManager, IEventAggregator eventAggregator, IControlEditorFactory controlEditorFactory,
            IApplicationDataManager applicationDataManager)
        {
            this.DisplayName = "Control Explorer";
            this.windowManager = windowManager;
            this.eventAggregator = eventAggregator;
            this.eventAggregator.SubscribeOnPublishedThread(this);
            this.controlEditorFactory = controlEditorFactory;
            this.applicationDataManager = applicationDataManager;

            CreateCollectionView();
        }

        #region Filter 


        string filterText = string.Empty;
        /// <summary>
        /// Filter text is used to apply a filter for visible controls on view
        /// </summary>
        public string FilterText
        {
            get
            {
                return filterText;
            }
            set
            {
                filterText = value;

                var view = CollectionViewSource.GetDefaultView(Controls);
                view.Refresh();
                NotifyOfPropertyChange(() => Controls);

            }
        }

        private void CreateCollectionView()
        {
            var groupedItems = CollectionViewSource.GetDefaultView(Controls);
            groupedItems.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ControlDescriptionViewModel.GroupName)));
            groupedItems.SortDescriptions.Add(new SortDescription(nameof(ControlDescriptionViewModel.GroupName), ListSortDirection.Ascending));
            groupedItems.SortDescriptions.Add(new SortDescription(nameof(ControlDescriptionViewModel.ControlName), ListSortDirection.Ascending));
            groupedItems.Filter = new Predicate<object>((a) =>
            {
                return (a as ControlDescriptionViewModel).ControlName.ToLower().Contains(this.filterText.ToLower());
            });
        }

        #endregion Filter

        #region Edit ControlToolBoxItem


        bool canEdit;
        /// <summary>
        /// Indicates if selected control can be edited
        /// </summary>
        public bool CanEdit
        {
            get
            {
                return canEdit;
            }
            set
            {
                canEdit = value;
                NotifyOfPropertyChange(() => CanEdit);
            }
        }

        /// <summary>
        /// Double click on control name to toggle visibility of textbox which can be used to edit the name
        /// </summary>
        /// <param name="targetControl"></param>
        public void ToggleRename(ControlDescriptionViewModel targetControl)
        {
            if (SelectedControl == targetControl)
            {
                CanEdit = !CanEdit;
            }
        }

        /// <summary>
        /// Press enter when in edit mode to apply the changed name of control after control name is edited in text box
        /// </summary>
        /// <param name="context"></param>
        /// <param name="controlToRename"></param>
        public async Task RenameControl(ActionExecutionContext context, ControlDescriptionViewModel controlToRename)
        {
            try
            {
                var keyArgs = context.EventArgs as KeyEventArgs;
                if (keyArgs != null && keyArgs.Key == Key.Enter)
                {
                    string newName = (context.Source as System.Windows.Controls.TextBox).Text;
                    if (this.Controls.Except(new[] { controlToRename }).Any(a => a.ControlName.Equals(newName)))
                    {
                        return;
                    }
                    controlToRename.ControlName = newName;
                    CanEdit = false;
                    await SaveControlDetails(controlToRename, false);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                CanEdit = false;
            }
        }


        #endregion Edit ControlToolBoxItem        
      
        #region Manage Controls

        /// <summary>
        /// Set the application for which control collection should be loaded and displayed
        /// </summary>
        /// <param name="applicationDescriptionViewModel"></param>
        public void SetActiveApplication(ApplicationDescriptionViewModel applicationDescriptionViewModel)
        {
            this.ActiveApplication = applicationDescriptionViewModel;
            this.Controls.Clear();
            this.Screens.Clear();
            if (applicationDescriptionViewModel != null)
            {                            
                this.Screens.AddRange(this.ActiveApplication.ScreensCollection);
                this.SelectedScreen = this.Screens.First();               
            }
        }

        private IEnumerable<ControlDescriptionViewModel> LoadControlDetails(ApplicationDescriptionViewModel applicationDescriptionViewModel, string screenName)
        {
            List<ControlDescriptionViewModel> controlsList = new List<ControlDescriptionViewModel>();
            if (applicationDescriptionViewModel.AvailableControls.ContainsKey(screenName))
            {
                var controlIdentifers = applicationDescriptionViewModel.AvailableControls[screenName];
                if (controlIdentifers.Any() && applicationDescriptionViewModel.ControlsCollection.Any(a => a.ControlId.Equals(controlIdentifers.First())))
                {
                    foreach (var controlId in controlIdentifers)
                    {
                        var controls = applicationDescriptionViewModel.ControlsCollection.Where(a => a.ControlId.Equals(controlId));
                        if (controls.Any())
                        {
                            controlsList.AddRange(controls);
                        }
                    }
                }
                else
                {
                    var controls = this.applicationDataManager.GetControlsForScreen(applicationDescriptionViewModel.Model, screenName).ToList();
                    foreach (var control in controls)
                    {
                        var controlDescriptionViewModel = new ControlDescriptionViewModel(control);                       
                        applicationDescriptionViewModel.AddControl(controlDescriptionViewModel, screenName);
                        controlsList.Add(controlDescriptionViewModel);
                    }
                }
                     
            }          
            return controlsList;
        }

        /// <summary>
        /// Create a new screen for the application. Screens are used to group application controls.
        /// </summary>
        /// <returns></returns>
        public async Task CreateScreen()
        {
            var applicationScreenViewModel = new ApplicationScreenViewModel(this.ActiveApplication);
            var result = await windowManager.ShowDialogAsync(applicationScreenViewModel);
            if (result.GetValueOrDefault())
            {
                await this.applicationDataManager.AddOrUpdateApplicationAsync(this.ActiveApplication.Model);
                var lastSelectedScreen = this.selectedScreen;
                this.Screens.Clear();
                this.Screens.AddRange(this.ActiveApplication.ScreensCollection);
                this.SelectedScreen = lastSelectedScreen;
                logger.Information("Added screen {0} to application {1}", applicationScreenViewModel.ScreenName, this.ActiveApplication.ApplicationName);
            }
        }

        /// <summary>
        /// Rename screen to a new value
        /// </summary>
        /// <param name="selectedScreen"></param>
        /// <returns></returns>
        public async Task RenameScreen()
        {
            var renameScreenViewModel = new RenameScreenViewModel(this.ActiveApplication, this.selectedScreen);
            var result = await windowManager.ShowDialogAsync(renameScreenViewModel);
            if (result.GetValueOrDefault())
            {
                await this.applicationDataManager.AddOrUpdateApplicationAsync(this.ActiveApplication.Model);               
                this.Screens.Clear();
                this.Screens.AddRange(this.ActiveApplication.ScreensCollection);           
                this.SelectedScreen = renameScreenViewModel.NewScreenName;
                logger.Information("Renamed screen {0} to {1} for application {2}", renameScreenViewModel.ScreenName, renameScreenViewModel.NewScreenName, this.ActiveApplication.ApplicationName);
            }
        }

        /// <summary>
        /// Move a control from one screen to another
        /// </summary>
        /// <param name="controlDescription"></param>
        /// <returns></returns>
        public async Task MoveToScreen(ControlDescriptionViewModel controlDescription)
        {
            var moveToScreenViewModel = new MoveToScreenViewModel(this.ActiveApplication, controlDescription, this.selectedScreen, this.Screens);
            var result = await windowManager.ShowDialogAsync(moveToScreenViewModel);
            if (result.GetValueOrDefault())
            {
                await this.applicationDataManager.AddOrUpdateApplicationAsync(this.ActiveApplication.Model);
                this.Controls.Remove(controlDescription);
                logger.Information("Moved control : {0} from screen {1} to {2} for application {3}", controlDescription.ControlName, this.selectedScreen, moveToScreenViewModel.SelectedScreen, this.ActiveApplication.ApplicationName);
            }
        }

        /// <summary>
        /// Open the ControlEditor View to allow edtiing the captured automation identifers and search strategy for control.
        /// </summary>
        /// <param name="controlToEdit"></param>
        /// <returns></returns>
        public async Task ConfigureControlAsync(ControlDescriptionViewModel controlToEdit)
        {
            Guard.Argument(controlToEdit).NotNull();

            //Make a copy of ControlDescription that is opened for edit
            var copyOfControlToEdit = controlToEdit.ControlDescription.Clone() as ControlDescription;
            copyOfControlToEdit.ControlId = controlToEdit.ControlId;
            var controlEditor = controlEditorFactory.CreateControlEditor(controlToEdit.ControlDetails);
            controlEditor.Initialize(copyOfControlToEdit);
            var result = await windowManager.ShowDialogAsync(controlEditor);
            //if save was clicked, assign back changes in ControlDetails to controlToEdit.
            //Editor only allows editing ControlDetails. Description won't be modified.
            if (result.HasValue && result.Value)
            {
                controlToEdit.ControlDetails = copyOfControlToEdit.ControlDetails;
                await SaveControlDetails(controlToEdit, false);
                await this.eventAggregator.PublishOnBackgroundThreadAsync(new ControlUpdatedEventArgs(controlToEdit.ControlId));
            }
        }

        /// <summary>
        /// Allow Control description to be modified from PropertyGrid
        /// </summary>
        /// <param name="controlToEdit"></param>
        /// <returns></returns>
        public async Task EditControlAsync(ControlDescriptionViewModel controlToEdit)
        {
            await this.eventAggregator.PublishOnUIThreadAsync(new PropertyGridObjectEventArgs(controlToEdit, () => { _ = SaveControlDetails(controlToEdit, false); }, () => { return true; }));
        }

        /// <summary>
        /// Show file browse dialog and let user pick a new image for the control.
        /// Existing image will be deleted and replaced with the new image picked by user.
        /// </summary>
        /// <param name="selectedControl"></param>
        /// <returns></returns>
        public async Task ChangeImageFromExistingAsync(ControlDescriptionViewModel selectedControl)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PNG File (*.Png)|*.Png";
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = openFileDialog.FileName;
                File.Delete(selectedControl.ControlImage);
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    //we can't reuse the same control image name due to caching issues with bitmap which will keep using old file
                    //unless file name changes
                    selectedControl.ControlImage = await this.applicationDataManager.AddOrUpdateControlImageAsync(selectedControl.ControlDescription, fs);
                    await SaveControlDetails(selectedControl, false);
                    // This will force reload image on control explorer
                    selectedControl.ImageSource = null;
                    // This will force reload image on process designer
                    await this.eventAggregator.PublishOnBackgroundThreadAsync(new ControlUpdatedEventArgs(selectedControl.ControlId));
                }
            }
        }

        /// <summary>
        /// Create a copy of control
        /// </summary>
        /// <param name="controlToRename"></param>
        public async Task CloneControl(ControlDescriptionViewModel controlToClone)
        {
            try
            {
                Guard.Argument(controlToClone).NotNull();

                var clonedControl = controlToClone.ControlDescription.Clone() as ControlDescription;
                var controlDescriptionViewModel = new ControlDescriptionViewModel(clonedControl);
                controlDescriptionViewModel.ControlName = Path.GetRandomFileName();
                await SaveBitMapSource(controlDescriptionViewModel.ControlDescription, controlDescriptionViewModel.ImageSource);
                await SaveControlDetails(controlDescriptionViewModel, true);
                this.Controls.Add(controlDescriptionViewModel);
                logger.Information("Created a clone of control : {0}", controlToClone.ControlDescription);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to clone the control.");
            }
        }

        /// <summary>
        /// Create a copy of control
        /// </summary>
        /// <param name="controlToRename"></param>
        public async Task CreateRevision(ControlDescriptionViewModel control)
        {
            Guard.Argument(control).NotNull();

            var clonedControl = control.ControlDescription.Clone() as ControlDescription;
            clonedControl.ControlId = control.ControlId;
            clonedControl.Version = new Version(control.Version.Major + 1, 0);
            var controlDescriptionViewModel = new ControlDescriptionViewModel(clonedControl);
            await SaveBitMapSource(controlDescriptionViewModel.ControlDescription, controlDescriptionViewModel.ImageSource);
            await SaveControlDetails(controlDescriptionViewModel, true);
            //we remove the visible version and add the new revised version in explorer.
            this.Controls.Remove(control);
            this.Controls.Add(controlDescriptionViewModel);
            logger.Information("Created a new revision for control : {0}", control.ControlDescription);
        }

        private readonly object locker = new object();
        /// <summary>
        /// Save the ControlDescription optionally specifying whether to save the parent ApplicationDescription as well
        /// </summary>
        /// <param name="controlToSave"></param>
        /// <param name="updateApplication"></param>
        /// <returns></returns>
        public async Task SaveControlDetails(ControlDescriptionViewModel controlToSave, bool updateApplication)
        {
            await this.applicationDataManager.AddOrUpdateControlAsync(controlToSave.ControlDescription);
            lock (locker)
            {
                this.ActiveApplication.AddControl(controlToSave, this.SelectedScreen);
            }
            if (updateApplication)
            {
                await this.applicationDataManager.AddOrUpdateApplicationAsync(this.ActiveApplication.Model);
            }

            var view = CollectionViewSource.GetDefaultView(Controls);
            view.Refresh();
            NotifyOfPropertyChange(() => Controls);

            logger.Information($"Control details saved for {controlToSave.ControlName}");
        }

        private BitmapImage ConvertToImageSource(byte[] controlImage)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = new MemoryStream(controlImage);
            image.EndInit();
            return image;
        }

        private async Task SaveBitMapSource(ControlDescription controlDescription, ImageSource imageSource)
        {
            if (imageSource is BitmapImage image)
            {
                using (var stream = new MemoryStream())
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(stream);
                    controlDescription.ControlImage = await this.applicationDataManager.AddOrUpdateControlImageAsync(controlDescription, stream);
                    if (controlDescription.ControlDetails is IImageControlIdentity imageControlIdentity)
                    {
                        var imageDescription = new ImageDescription()
                        {
                            ControlImage = controlDescription.ControlImage,
                            PivotPoint = Core.Enums.Pivots.Center,
                            ScreenWidth = (short)SystemParameters.PrimaryScreenWidth,
                            ScreenHeight = (short)SystemParameters.PrimaryScreenHeight
                        };
                        imageControlIdentity.AddImage(imageDescription);
                    }
                }
                return;
            }
            throw new ArgumentException($"{nameof(imageSource)} must be a BitmapImage");
        }

        #endregion Manage Controls

        /// <summary>
        /// Receive a collection of ScrapedControl and process them and save as ControlDescription
        /// </summary>
        /// <param name="scrapedControls"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task HandleAsync(IEnumerable<ScrapedControl> scrapedControls, CancellationToken cancellationToken)
        {
            logger.Information("Received {count} scraped controls to process", scrapedControls.Count());
            if (!scrapedControls.Any())
            {
                return;
            }
            if (this.ActiveApplication == null)
            {
                throw new InvalidOperationException("There is no active application in Application explorer");
            }
            foreach (ScrapedControl scrapedControl in scrapedControls)
            {
                try
                {

                    IControlIdentity control;

                    //The plugin can already provide the scraped data as IControlIdentity
                    if (scrapedControl.ControlData is IControlIdentity controlIdentity)
                    {
                        control = controlIdentity;
                    }
                    //the scraped control data needs to be processed by a IControlIdentityBuilder of active application to IControlIdentity
                    else
                    {
                        var ownerApplication = this.ActiveApplication.ApplicationDetails;
                        var controlBuilderType = TypeDescriptor.GetAttributes(ownerApplication.GetType()).OfType<BuilderAttribute>()?.FirstOrDefault()?.Builder
                            ?? throw new Exception("No control builder available to process scraped control");
                        var controlBuilder = Activator.CreateInstance(controlBuilderType) as IControlIdentityBuilder;
                        control = controlBuilder.CreateFromData(scrapedControl.ControlData);
                    }

                    //update the application id for each control identity in hierarchy
                    control.ApplicationId = this.ActiveApplication.ApplicationId;
                    IControlIdentity current = control;
                    while (current.Next != null)
                    {
                        current = current.Next;
                        current.ApplicationId = this.ActiveApplication.ApplicationId;
                    }

                    //create an instance of ControlToolBoxItem to display in the toolbox
                    var controlDescription = new ControlDescription(control)
                    {
                        Version = new Version(1, 0)
                    };
                    ControlDescriptionViewModel controlDescriptionViewModel = new ControlDescriptionViewModel(controlDescription);
                    controlDescriptionViewModel.ControlName = (this.Controls.Count() + 1).ToString();
                    controlDescriptionViewModel.ImageSource = ConvertToImageSource(scrapedControl.ControlImage);

                    //save the captured control details                        
                    await SaveBitMapSource(controlDescriptionViewModel.ControlDescription, controlDescriptionViewModel.ImageSource);
                    await SaveControlDetails(controlDescriptionViewModel, false);
                    this.Controls.Add(controlDescriptionViewModel);

                    logger.Information($"Added control with details {controlDescription}");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, ex.Message);
                }

            }
            await this.applicationDataManager.AddOrUpdateApplicationAsync(this.ActiveApplication.Model);
        }
    }
}
