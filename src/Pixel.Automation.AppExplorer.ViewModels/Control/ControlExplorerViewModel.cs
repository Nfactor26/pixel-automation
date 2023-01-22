using Caliburn.Micro;
using Dawn;
using Microsoft.Win32;
using Notifications.Wpf.Core;
using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.AppExplorer.ViewModels.Contracts;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Editor.Core.Helpers;
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
        private readonly INotificationManager notificationManager;
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

        public ApplicationScreenCollection ScreenCollection { get; private set; }      

        /// <summary>
        /// Controls belonging to the active application
        /// </summary>
        public BindableCollection<ControlDescriptionViewModel> Controls { get; set; } = new();

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
        public ControlExplorerViewModel(IWindowManager windowManager, INotificationManager notificationManager, IEventAggregator eventAggregator, 
            IControlEditorFactory controlEditorFactory, IApplicationDataManager applicationDataManager)
        {
            this.DisplayName = "Control Explorer";
            this.windowManager = Guard.Argument(windowManager, nameof(windowManager)).Value;
            this.notificationManager = Guard.Argument(notificationManager, nameof(notificationManager)).Value;
            this.eventAggregator = Guard.Argument(eventAggregator, nameof(eventAggregator)).Value;
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
            string currentName = controlToRename.ControlName;
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
                controlToRename.ControlName = currentName;
                CanEdit = false;
                await notificationManager.ShowErrorNotificationAsync(ex);
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
            if(this.ScreenCollection != null)
            {
                this.ScreenCollection.ScreenChanged -= OnScreenChanged;
            }
            this.ActiveApplication = applicationDescriptionViewModel;
            this.ScreenCollection = applicationDescriptionViewModel?.ScreenCollection;            
            this.Controls.Clear();
            if(this.ScreenCollection != null)
            {
                this.ScreenCollection.ScreenChanged += OnScreenChanged;
                OnScreenChanged(this, this.ScreenCollection.SelectedScreen);
            }
            NotifyOfPropertyChange(nameof(this.ScreenCollection));
        }

        private void OnScreenChanged(object sender, string selectedScreen)
        {
            try
            {
                if (!string.IsNullOrEmpty(selectedScreen))
                {
                    var controlsForSelectedScreen = LoadControlDetails(this.ActiveApplication, selectedScreen);
                    this.Controls.Clear();
                    this.Controls.AddRange(controlsForSelectedScreen);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error occured while loading controls for screen : '{0}'", selectedScreen);
                _ = notificationManager.ShowErrorNotificationAsync(ex);
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
                        if(control.IsDeleted)
                        {
                            continue;
                        }
                        var controlDescriptionViewModel = new ControlDescriptionViewModel(control);
                        applicationDescriptionViewModel.AddControl(controlDescriptionViewModel, screenName);
                        controlsList.Add(controlDescriptionViewModel);
                    }
                }
                     
            }          
            return controlsList;
        }
       
        /// Move a control from one screen to another
        /// </summary>
        /// <param name="controlDescription"></param>
        /// <returns></returns>
        public async Task MoveToScreen(ControlDescriptionViewModel controlDescription)
        {
            try
            {
                Guard.Argument(controlDescription, nameof(controlDescription)).NotNull();
                var moveToScreenViewModel = new MoveToScreenViewModel(controlDescription.ControlName, this.ScreenCollection.Screens, this.ScreenCollection.SelectedScreen);
                var result = await windowManager.ShowDialogAsync(moveToScreenViewModel);
                if (result.GetValueOrDefault())
                {
                    this.ActiveApplication.MoveControlToScreen(controlDescription, this.ScreenCollection.SelectedScreen, moveToScreenViewModel.SelectedScreen);
                    await this.applicationDataManager.AddOrUpdateApplicationAsync(this.ActiveApplication.Model);
                    this.Controls.Remove(controlDescription);
                    logger.Information("Moved control : {0} from screen {1} to {2} for application {3}", controlDescription.ControlName, this.ScreenCollection.SelectedScreen, moveToScreenViewModel.SelectedScreen, this.ActiveApplication.ApplicationName);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while moving control : '{0}' to another screen", controlDescription?.ControlName);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }

        /// <summary>
        /// Open the ControlEditor View to allow edtiing the captured automation identifers and search strategy for control.
        /// </summary>
        /// <param name="controlToEdit"></param>
        /// <returns></returns>
        public async Task ConfigureControlAsync(ControlDescriptionViewModel controlToEdit)
        {
            try
            {
                Guard.Argument(controlToEdit, nameof(controlToEdit)).NotNull();

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
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to modify configuration of control : '{0}'", controlToEdit?.ControlName);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }

        /// <summary>
        /// Allow Control description to be modified from PropertyGrid
        /// </summary>
        /// <param name="controlToEdit"></param>
        /// <returns></returns>
        public async Task EditControlAsync(ControlDescriptionViewModel controlToEdit)
        {
            Guard.Argument(controlToEdit, nameof(controlToEdit)).NotNull();
            await this.eventAggregator.PublishOnUIThreadAsync(new PropertyGridObjectEventArgs(controlToEdit, 
                async () => {
                    try
                    {
                        await SaveControlDetails(controlToEdit, false);
                        await notificationManager.ShowSuccessNotificationAsync("Control was saved");
                    }
                    catch(Exception ex)
                    {
                        logger.Error(ex, "There was an error while trying to save control : {0}", controlToEdit.ControlName);
                        await notificationManager.ShowErrorNotificationAsync(ex);
                    }
                }, 
                () => { 
                    return true; 
                }));
        }

        /// <summary>
        /// Delete the control
        /// </summary>
        /// <param name="controlToDelete"></param>
        public async Task DeleteControlAsync(ControlDescriptionViewModel controlToDelete)
        {
            try
            {
                Guard.Argument(controlToDelete, nameof(controlToDelete)).NotNull();
                await this.applicationDataManager.DeleteControlAsync(controlToDelete.ControlDescription);
                this.Controls.Remove(controlToDelete);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to delete control : {0}", controlToDelete.ControlName);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }


        /// <summary>
        /// Show file browse dialog and let user pick a new image for the control.
        /// Existing image will be deleted and replaced with the new image picked by user.
        /// </summary>
        /// <param name="selectedControl"></param>
        /// <returns></returns>
        public async Task ChangeImageFromExistingAsync(ControlDescriptionViewModel selectedControl)
        {
            try
            {
                Guard.Argument(selectedControl, nameof(selectedControl)).NotNull();
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "PNG File (*.Png)|*.Png";
                openFileDialog.InitialDirectory = Environment.CurrentDirectory;
                if (openFileDialog.ShowDialog() == true)
                {
                    string fileName = openFileDialog.FileName;                  
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
                    File.Delete(selectedControl.ControlImage);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to change image for control : {0}", selectedControl.ControlName);
                await notificationManager.ShowErrorNotificationAsync(ex);
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
                Guard.Argument(controlToClone, nameof(controlToClone)).NotNull();
                var clonedControl = controlToClone.ControlDescription.Clone() as ControlDescription;
                var controlDescriptionViewModel = new ControlDescriptionViewModel(clonedControl);
                controlDescriptionViewModel.ControlName = Path.GetRandomFileName();
                if(!string.IsNullOrEmpty(controlDescriptionViewModel.ControlImage))
                {
                    await SaveBitMapSource(controlDescriptionViewModel.ControlDescription, controlDescriptionViewModel.ImageSource);
                }
                await SaveControlDetails(controlDescriptionViewModel, true);
                this.Controls.Add(controlDescriptionViewModel);
                logger.Information("Created a clone of control : {0}", controlToClone.ControlDescription);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to clone control : {0}", selectedControl.ControlName);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }

        /// <summary>
        /// Create a copy of control
        /// </summary>
        /// <param name="controlToRename"></param>
        public async Task CreateRevision(ControlDescriptionViewModel control)
        {
            try
            {
                Guard.Argument(control, nameof(control)).NotNull();
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
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while creating revision of control : {0}", selectedControl.ControlName);
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
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
            Guard.Argument(controlToSave, nameof(controlToSave)).NotNull();
            await this.applicationDataManager.AddOrUpdateControlAsync(controlToSave.ControlDescription);
            lock (locker)
            {
                this.ActiveApplication.AddControl(controlToSave, this.ScreenCollection.SelectedScreen);
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


        /// <summary>
        /// Broadcast a FilterTestMessage which is processed by Test explorer view to filter and show only those test cases
        /// which uses this prefab
        /// </summary>
        /// <param name="targetPrefab"></param>
        /// <returns></returns>
        public async Task ShowUsage(ControlDescriptionViewModel controlDescription)
        {
            Guard.Argument(controlDescription, nameof(controlDescription)).NotNull();
            await this.eventAggregator.PublishOnUIThreadAsync(new TestFilterNotification("control", controlDescription.ControlId));
        }

        #endregion Manage Controls

        /// <summary>
        /// Receive a collection of ScrapedControl and process them and save as ControlDescription
        /// </summary>
        /// <param name="captureControls"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task HandleAsync(IEnumerable<ScrapedControl> captureControls, CancellationToken cancellationToken)
        {
            try
            {
                Guard.Argument(captureControls, nameof(captureControls)).NotNull();
                logger.Information("Received {count} scraped controls to process", captureControls.Count());               
                if (this.ActiveApplication == null)
                {
                    throw new InvalidOperationException("There is no active application in Application explorer");
                }
                foreach (ScrapedControl scrapedControl in captureControls)
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
                        if(scrapedControl.ControlImage != null)
                        {
                            controlDescriptionViewModel.ImageSource = ConvertToImageSource(scrapedControl.ControlImage);
                            await SaveBitMapSource(controlDescriptionViewModel.ControlDescription, controlDescriptionViewModel.ImageSource);
                        }

                        //save the captured control details                            
                        await SaveControlDetails(controlDescriptionViewModel, false);
                        this.Controls.Add(controlDescriptionViewModel);

                        logger.Information("Capture control details for control : '{0}'", controlDescription.ToString());
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "There was an error while trying to capture control details");
                        await notificationManager.ShowErrorNotificationAsync(ex);
                    }

                }
                await this.applicationDataManager.AddOrUpdateApplicationAsync(this.ActiveApplication.Model);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error while trying to capture control details");
                await notificationManager.ShowErrorNotificationAsync(ex);
            }
        }
    }
}
