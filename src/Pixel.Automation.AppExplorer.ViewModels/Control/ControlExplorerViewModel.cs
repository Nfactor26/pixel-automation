﻿using Caliburn.Micro;
using Dawn;
using Microsoft.Win32;
using Notifications.Wpf.Core;
using Pixel.Automation.AppExplorer.ViewModels.Application;
using Pixel.Automation.AppExplorer.ViewModels.Contracts;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Helpers;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client;
using Serilog;
using System.ComponentModel;
using System.Diagnostics;
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
                if(a is ControlDescriptionViewModel controlDescription)
                {
                    return controlDescription.ControlName.Contains(this.filterText, StringComparison.OrdinalIgnoreCase) ||
                            controlDescription.GroupName.Contains(this.filterText, StringComparison.OrdinalIgnoreCase);
                }
                return false;
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
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(RenameControl), ActivityKind.Internal))
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
                        activity?.SetTag("CurrentControlName", controlToRename.ControlName);
                        activity?.SetTag("NewControlName", newName);
                        controlToRename.ControlName = newName;
                        CanEdit = false;
                        await UpdateControlDetails(controlToRename);
                        await notificationManager.ShowSuccessNotificationAsync($"Control : '{currentName}' was renamed to : '{newName}'");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while renaming control : {0}.", controlToRename.ControlName);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    controlToRename.ControlName = currentName;
                    CanEdit = false;
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
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

        private void OnScreenChanged(object sender, ApplicationScreen selectedScreen)
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(OnScreenChanged), ActivityKind.Internal))
            {
                try
                {
                    this.Controls.Clear();
                    activity?.SetTag("SelectedScreen", selectedScreen?.ScreenName ?? string.Empty);
                    if (selectedScreen != null)
                    {  
                        var controlsForSelectedScreen = LoadControlDetails(this.ActiveApplication, selectedScreen);                       
                        this.Controls.AddRange(controlsForSelectedScreen);                   
                    }    
                    else
                    {
                        foreach (var screen in this.activeApplication.Screens)
                        {
                            var controlsForScren = LoadControlDetails(this.ActiveApplication, screen);
                            this.Controls.AddRange(controlsForScren);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "An error occured while loading controls for screen : '{0}'", selectedScreen);
                    _ = notificationManager.ShowErrorNotificationAsync(ex);
                }
            }           
        }

        private IEnumerable<ControlDescriptionViewModel> LoadControlDetails(ApplicationDescriptionViewModel applicationDescriptionViewModel, ApplicationScreen selectedScreen)
        {
            List<ControlDescriptionViewModel> controlsList = new List<ControlDescriptionViewModel>();
            if (applicationDescriptionViewModel.ContainsScreen(selectedScreen.ScreenName))
            {
                var controlIdentifers = selectedScreen.AvailableControls;
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
                    var controls = this.applicationDataManager.GetControlsForScreen(applicationDescriptionViewModel.Model, selectedScreen.ScreenName).ToList();
                    foreach (var control in controls)
                    {
                        if(control.IsDeleted)
                        {
                            continue;
                        }
                        var controlDescriptionViewModel = new ControlDescriptionViewModel(control);
                        applicationDescriptionViewModel.AddControl(controlDescriptionViewModel, selectedScreen.ScreenName);
                        controlsList.Add(controlDescriptionViewModel);
                    }
                }
                     
            }          
            return controlsList;
        }
       
        /// <summary>
        /// Move a control from one screen to another
        /// </summary>
        /// <param name="controlDescription"></param>
        /// <returns></returns>
        public async Task MoveToScreen(ControlDescriptionViewModel controlDescription)
        {
            Guard.Argument(controlDescription, nameof(controlDescription)).NotNull();
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(MoveToScreen), ActivityKind.Internal))
            {
                try
                {
                    activity?.SetTag("ControlName", controlDescription.ControlName);
                    var moveToScreenViewModel = new MoveToScreenViewModel(controlDescription.ControlName, this.ScreenCollection.Screens.Select(s => s.ScreenName), this.ScreenCollection.SelectedScreen.ScreenName);
                    var result = await windowManager.ShowDialogAsync(moveToScreenViewModel);
                    if (result.GetValueOrDefault())
                    {
                        await this.applicationDataManager.MoveControlToScreen(controlDescription.ControlDescription, this.ActiveApplication[moveToScreenViewModel.SelectedScreen].ScreenId);
                        this.ActiveApplication.MoveControlToScreen(controlDescription, this.ScreenCollection.SelectedScreen.ScreenName, moveToScreenViewModel.SelectedScreen);                       
                        this.Controls.Remove(controlDescription);                       
                        this.applicationDataManager.SaveApplicationToDisk(this.ActiveApplication.Model);
                        logger.Information("Moved control : {0} from screen {1} to {2} for application {3}", controlDescription.ControlName, this.ScreenCollection.SelectedScreen, moveToScreenViewModel.SelectedScreen, this.ActiveApplication.ApplicationName);
                        await notificationManager.ShowSuccessNotificationAsync($"Control : '{controlDescription.ControlName}' was moved to screen : '{moveToScreenViewModel.SelectedScreen}'");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while moving control : '{0}' to another screen", controlDescription?.ControlName);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }          
        }

        /// <summary>
        /// Open the ControlEditor View to allow edtiing the captured automation identifers and search strategy for control.
        /// </summary>
        /// <param name="controlToEdit"></param>
        /// <returns></returns>
        public async Task ConfigureControlAsync(ControlDescriptionViewModel controlToEdit)
        {
            Guard.Argument(controlToEdit, nameof(controlToEdit)).NotNull();
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(ConfigureControlAsync), ActivityKind.Internal))
            {
                try
                {
                    var availableVersions = this.applicationDataManager.GetAllVersionsOfControl(controlToEdit.ApplicationId, controlToEdit.ControlId);
                    if(availableVersions.Count() > 1)
                    {
                        var versionPicker = new ControlVersionPickerViewModel(availableVersions);
                        var versionPickerResult = await windowManager.ShowDialogAsync(versionPicker);
                        if (versionPickerResult.HasValue && versionPickerResult.Value)
                        {
                            var versionToEdit = versionPicker.SelectedVersion;
                            var targetControl = availableVersions.Single(a => a.Version.Equals(versionToEdit));
                            await ConfigureControlDescriptionAsync(new ControlDescriptionViewModel(targetControl));
                        }                        
                    }   
                    else
                    {
                        await ConfigureControlDescriptionAsync(controlToEdit);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while trying to configure control : '{0}'", controlToEdit.ControlName);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }             
        }

        private async Task ConfigureControlDescriptionAsync(ControlDescriptionViewModel controlToEdit)
        {
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(ConfigureControlDescriptionAsync), ActivityKind.Internal))
            {
                try
                {
                    activity?.SetTag("ControlName", controlToEdit.ControlName);
                    
                    //Make a copy of ControlDescription that is opened for edit
                    var copyOfControlToEdit = controlToEdit.ControlDescription.Clone() as ControlDescription;
                    copyOfControlToEdit.ControlId = controlToEdit.ControlId;
                    var controlEditor = controlEditorFactory.CreateControlEditor(copyOfControlToEdit.ControlDetails);
                    controlEditor.Initialize(copyOfControlToEdit);
                    var result = await windowManager.ShowDialogAsync(controlEditor);
                    //if save was clicked, assign back changes in ControlDetails to controlToEdit.
                    //Editor only allows editing ControlDetails. Description won't be modified.
                    if (result.HasValue && result.Value)
                    {
                        controlToEdit.ControlDetails = copyOfControlToEdit.ControlDetails;
                        await UpdateControlDetails(controlToEdit);
                        await this.eventAggregator.PublishOnBackgroundThreadAsync(new ControlUpdatedEventArgs(controlToEdit.ControlId));
                        await notificationManager.ShowSuccessNotificationAsync($"Configuration updated for version : '{controlToEdit.Version}' of control : '{controlToEdit.ControlName}'");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while trying to configure version : '{0}' of control : '{1}'", controlToEdit.Version, controlToEdit.ControlName);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
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
                    using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(EditControlAsync), ActivityKind.Internal))
                    {
                        try
                        {
                            activity?.SetTag("ControlName", controlToEdit.ControlName);
                            await UpdateControlDetails(controlToEdit);
                            await notificationManager.ShowSuccessNotificationAsync($"Control : '{controlToEdit.ControlName}' was updated");
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, "There was an error while trying to save control : {0}", controlToEdit.ControlName);
                            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                            await notificationManager.ShowErrorNotificationAsync(ex);
                        }
                    }                     
                }, 
                () => { 
                    return true; 
                }));
        }

        /// <summary>
        /// Mark all the versions of control as deleted.
        /// </summary>
        /// <param name="controlToDelete"></param>
        public async Task DeleteControlAsync(ControlDescriptionViewModel controlToDelete)
        {
            Guard.Argument(controlToDelete, nameof(controlToDelete)).NotNull();
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(DeleteControlAsync), ActivityKind.Internal))
            {
                try
                {
                    activity?.SetTag("ControlName", controlToDelete.ControlName);
                    await this.applicationDataManager.DeleteControlAsync(controlToDelete.ControlDescription);
                    this.ActiveApplication.DeleteControl(controlToDelete, this.ActiveApplication.ScreenCollection.SelectedScreen.ScreenName);
                    this.Controls.Remove(controlToDelete);
                    this.applicationDataManager.SaveApplicationToDisk(this.ActiveApplication.Model);
                    await notificationManager.ShowSuccessNotificationAsync($"Control : '{controlToDelete.ControlName}' was deleted");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while trying to delete control : {0}", controlToDelete.ControlName);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
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
            Guard.Argument(selectedControl, nameof(selectedControl)).NotNull();
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(ChangeImageFromExistingAsync), ActivityKind.Internal))
            {
                try
                {
                    var availableVersions = this.applicationDataManager.GetAllVersionsOfControl(selectedControl.ApplicationId, selectedControl.ControlId);
                    if (availableVersions.Count() > 1)
                    {
                        var versionPicker = new ControlVersionPickerViewModel(availableVersions);
                        var versionPickerResult = await windowManager.ShowDialogAsync(versionPicker);
                        if (versionPickerResult.HasValue && versionPickerResult.Value)
                        {
                            var versionToEdit = versionPicker.SelectedVersion;
                            var targetControl = availableVersions.Single(a => a.Version.Equals(versionToEdit));
                            await ChangeImageForControlAsync(new ControlDescriptionViewModel(targetControl));
                        }
                    }
                    else
                    {
                        await ChangeImageForControlAsync(selectedControl);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while trying to change image for control : {0}", selectedControl.ControlName);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }
        }

        private async Task ChangeImageForControlAsync(ControlDescriptionViewModel selectedControl)
        {
            Guard.Argument(selectedControl, nameof(selectedControl)).NotNull();
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(ChangeImageForControlAsync), ActivityKind.Internal))
            {
                try
                {
                    activity?.SetTag("ControlName", selectedControl.ControlName);
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "PNG File (*.Png)|*.Png";
                    openFileDialog.InitialDirectory = Environment.CurrentDirectory;
                    if (openFileDialog.ShowDialog() == true)
                    {
                        string fileName = openFileDialog.FileName;
                        using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                        {
                            string oldImage = selectedControl.ControlImage;
                            //we can't reuse the same control image name due to caching issues with bitmap which will keep using old file
                            //unless file name changes
                            selectedControl.ControlImage = await this.applicationDataManager.AddOrUpdateControlImageAsync(selectedControl.ControlDescription, fs);
                            await UpdateControlDetails(selectedControl);
                            File.Delete(oldImage);
                            // This will force reload image on control explorer
                            selectedControl.ImageSource = null;
                            // This will force reload image on process designer
                            await this.eventAggregator.PublishOnBackgroundThreadAsync(new ControlUpdatedEventArgs(selectedControl.ControlId));
                        }                       
                        await notificationManager.ShowSuccessNotificationAsync($"Image was updated for version : '{selectedControl.Version}' of control : '{selectedControl.ControlName}'");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while trying to change image for version : '{0}' of control : '{1}'", selectedControl.Version, selectedControl.ControlName); ;
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }
        }

        /// <summary>
        /// Create a copy of control
        /// </summary>
        /// <param name="controlToRename"></param>
        public async Task CloneControl(ControlDescriptionViewModel controlToClone)
        {
            Guard.Argument(controlToClone, nameof(controlToClone)).NotNull();
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(CloneControl), ActivityKind.Internal))
            {
                try
                {
                    activity?.SetTag("ControlName", controlToClone.ControlName);
                    var clonedControl = controlToClone.ControlDescription.Clone() as ControlDescription;
                    var controlDescriptionViewModel = new ControlDescriptionViewModel(clonedControl);
                    controlDescriptionViewModel.ControlName = Path.GetRandomFileName();
                    if (!string.IsNullOrEmpty(controlDescriptionViewModel.ControlImage))
                    {
                        await SaveBitMapSource(controlDescriptionViewModel.ControlDescription, controlDescriptionViewModel.ImageSource);
                    }
                    await AddControlDetails(this.ScreenCollection.SelectedScreen.ScreenName, controlDescriptionViewModel);
                 
                    logger.Information("Created a clone of control : {0}", controlToClone.ControlDescription);
                    await notificationManager.ShowSuccessNotificationAsync($"Copy of control : '{controlToClone.ControlName}' was created");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while trying to clone control : {0}", selectedControl.ControlName);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }            
        }

        /// <summary>
        /// Create a copy of control
        /// </summary>
        /// <param name="controlToRename"></param>
        public async Task CreateRevision(ControlDescriptionViewModel control)
        {
            Guard.Argument(control, nameof(control)).NotNull();
            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(CreateRevision), ActivityKind.Internal))
            {
                try
                {                   
                    activity?.SetTag("ControlName", control.ControlName);
                    var clonedControl = control.ControlDescription.Clone() as ControlDescription;
                    clonedControl.ControlId = control.ControlId;
                    clonedControl.Version = new Version(control.Version.Major + 1, 0);
                 
                    var controlDescriptionViewModel = new ControlDescriptionViewModel(clonedControl);
                    await AddControlDetails(this.ScreenCollection.SelectedScreen.ScreenName, controlDescriptionViewModel);
                    await SaveBitMapSource(controlDescriptionViewModel.ControlDescription, controlDescriptionViewModel.ImageSource);
                    this.Controls.Remove(control); //Remove current version from controls collection

                    logger.Information("Created a new revision for control : {0}", control.ControlDescription);
                    await notificationManager.ShowSuccessNotificationAsync($"Version of control : '{control.ControlName}' was incremented to : '{clonedControl.Version}'");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while creating revision of control : {0}", selectedControl.ControlName);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }            
        }
       
        /// <summary>
        /// Save changes to an existing control
        /// </summary>
        /// <param name="controlToSave"></param>   
        /// <returns></returns>
        internal async Task UpdateControlDetails(ControlDescriptionViewModel controlToSave)
        {
            Guard.Argument(controlToSave, nameof(controlToSave)).NotNull();
            await this.applicationDataManager.UpdateControlAsync(controlToSave.ControlDescription);         
          
            var view = CollectionViewSource.GetDefaultView(Controls);
            view.Refresh();
            NotifyOfPropertyChange(() => Controls);

            logger.Information("Control '{0}' was updated for application : '{1}'", controlToSave.ControlName, this.ActiveApplication.ApplicationName);
        }

        private readonly object locker = new object();

        /// <summary>
        /// Add a new control detail to selected screen of application
        /// </summary>
        /// <param name="screenName"></param>
        /// <param name="controlToAdd"></param>
        /// <returns></returns>
        internal async Task AddControlDetails(string screenName, ControlDescriptionViewModel controlToAdd)
        {                                
            await this.applicationDataManager.AddControlToScreenAsync(controlToAdd.ControlDescription, this.ActiveApplication[screenName].ScreenId);
           
            lock (locker)
            {
                this.ActiveApplication.AddControl(controlToAdd, screenName);            
                this.Controls.Add(controlToAdd);
            }
            this.applicationDataManager.SaveApplicationToDisk(this.ActiveApplication.Model);
          
            var view = CollectionViewSource.GetDefaultView(Controls);
            view.Refresh();
            NotifyOfPropertyChange(() => Controls);

            logger.Information("Control '{0}' was added to application : '{1}'", controlToAdd.ControlName, this.ActiveApplication.ApplicationName);
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
            Guard.Argument(captureControls, nameof(captureControls)).NotNull();
            if (this.ActiveApplication == null)
            {
                throw new InvalidOperationException("There is no active application in Application explorer");
            }

            using (var activity = Telemetry.DefaultSource?.StartActivity(nameof(HandleAsync), ActivityKind.Internal))
            {
                try
                {
                    logger.Information("Received {count} scraped controls to process", captureControls.Count());
                    activity?.SetTag("ApplicationName", this.activeApplication.ApplicationName);
                    activity?.SetTag("ControlCount", captureControls.Count());               
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
                            if (scrapedControl.ControlImage != null)
                            {
                                controlDescriptionViewModel.ImageSource = ConvertToImageSource(scrapedControl.ControlImage);
                                await SaveBitMapSource(controlDescriptionViewModel.ControlDescription, controlDescriptionViewModel.ImageSource);
                            }

                            //save the captured control details                            
                            await AddControlDetails(this.ScreenCollection.SelectedScreen.ScreenName, controlDescriptionViewModel);                          

                            logger.Information("Capture control details for control : '{0}'", controlDescription.ToString());
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, "There was an error while trying to capture control details");
                            await notificationManager.ShowErrorNotificationAsync(ex);
                        }

                    }
                   // await this.applicationDataManager.AddOrUpdateApplicationAsync(this.ActiveApplication.Model);                  
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "There was an error while trying to capture control details");
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    await notificationManager.ShowErrorNotificationAsync(ex);
                }
            }                
        }
    }
}
