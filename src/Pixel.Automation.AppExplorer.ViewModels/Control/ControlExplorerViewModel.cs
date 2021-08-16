﻿using Caliburn.Micro;
using Dawn;
using Microsoft.Win32;
using Pixel.Automation.Core.Args;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Pixel.Automation.AppExplorer.ViewModels.Control
{
    public class ControlExplorerViewModel : Screen, IHandle<IEnumerable<ScrapedControl>>
    {
        private readonly ILogger logger = Log.ForContext<ControlExplorerViewModel>();
        private readonly IControlEditor controlEditor;
        private readonly IEventAggregator eventAggregator;
        private readonly IWindowManager windowManager;     
        private readonly IApplicationDataManager applicationDataManager;

        private ApplicationDescription activeApplication;

        public BindableCollection<ControlDescriptionViewModel> Controls { get; set; } = new BindableCollection<ControlDescriptionViewModel>();

        private ControlDescriptionViewModel selectedControl;
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

        public ControlExplorerViewModel(IWindowManager windowManager, IEventAggregator eventAggregator, IControlEditor controlEditor,
            IApplicationDataManager applicationDataManager)
        {
            this.windowManager = windowManager;           
            this.eventAggregator = eventAggregator;
            this.eventAggregator.SubscribeOnPublishedThread(this);
            this.controlEditor = controlEditor;
            this.applicationDataManager = applicationDataManager;

            CreateCollectionView();
        }
      
        #region Filter 


        string filterText = string.Empty;
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

        public void ToggleRename(ControlDescriptionViewModel targetControl)
        {
            if (SelectedControl == targetControl)
            {
                CanEdit = !CanEdit;
            }
        }

        /// <summary>
        /// Update the name of ControlToolBoxItem
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
                    if(this.Controls.Except(new[] { controlToRename }).Any(a => a.ControlName.Equals(newName)))
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
        

        public void SetActiveApplication(ApplicationDescription application)
        {
            this.activeApplication = application;
            LoadControlDetails(application);

            this.Controls.Clear();
            this.Controls.AddRange(this.activeApplication.ControlsCollection.Select(a => new ControlDescriptionViewModel(a)));
        }

        private void LoadControlDetails(ApplicationDescription application)
        {
            if (application.ControlsCollection.Count() == 0)
            {
                var controls = this.applicationDataManager.GetAllControls(application).ToList();
                application.ControlsCollection.AddRange(controls);
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

            var copyOfControlToEdit = controlToEdit.ControlDetails.Clone() as IControlIdentity;
            controlEditor.Initialize(copyOfControlToEdit);
            var result = await windowManager.ShowDialogAsync(controlEditor);
            if (result.HasValue && result.Value)
            {
                controlToEdit.ControlDetails = copyOfControlToEdit;
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
            Guard.Argument(controlToClone).NotNull();

            var clonedControl = controlToClone.ControlDescription.Clone() as ControlDescription;
            var controlDescriptionViewModel = new ControlDescriptionViewModel(clonedControl);
            controlDescriptionViewModel.ControlName = Path.GetRandomFileName();
            await SaveControlDetails(controlDescriptionViewModel, true);
            await SaveBitMapSource(controlDescriptionViewModel.ControlDescription, controlDescriptionViewModel.ImageSource);
            this.Controls.Add(controlDescriptionViewModel);           
        }

        private readonly object locker = new object();
        public async Task SaveControlDetails(ControlDescriptionViewModel controlToSave, bool updateApplication)
        {
            await this.applicationDataManager.AddOrUpdateControlAsync(controlToSave.ControlDescription);
            if(updateApplication)
            {
                lock (locker)
                {
                    this.activeApplication.AddControl(controlToSave.ControlDescription);                   
                }
                await this.applicationDataManager.AddOrUpdateApplicationAsync(this.activeApplication);
            }             
         
            var view = CollectionViewSource.GetDefaultView(Controls);
            view.Refresh();
            NotifyOfPropertyChange(() => Controls);

            logger.Information($"Control details saved for {controlToSave.ControlName}");
        }

        private BitmapImage ConvertToImageSource(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            src.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }


        private async Task AddControlAsync(ControlDescriptionViewModel controlItem)
        {
            await SaveBitMapSource(controlItem.ControlDescription, controlItem.ImageSource);
            await SaveControlDetails(controlItem, true);            
            this.Controls.Add(controlItem);         
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
                }
                return;
            }
            throw new ArgumentException($"{nameof(imageSource)} must be a BitmapImage");
        }


        public async Task HandleAsync(IEnumerable<ScrapedControl> scrapedControls, CancellationToken cancellationToken)
        {
            logger.Information("Received {count} scraped controls to process", scrapedControls.Count());
            if (this.activeApplication == null)
            {
                throw new InvalidOperationException("There is no active application in Application explorer");
            }
            foreach (ScrapedControl scrapedControl in scrapedControls)
            {
                using (scrapedControl)
                {
                    try
                    {
                        //update the application id for each control identity in hierarchy
                        IControlIdentity control = scrapedControl.ControlData;
                        control.ApplicationId = this.activeApplication.ApplicationId;
                        IControlIdentity current = control;
                        while (current.Next != null)
                        {
                            current = current.Next;
                            current.ApplicationId = this.activeApplication.ApplicationId;
                        }

                        //create an instance of ControlToolBoxItem to display in the toolbox
                        var controlDescription = new ControlDescription(control);
                        ControlDescriptionViewModel controlDescriptionViewModel = new ControlDescriptionViewModel(controlDescription);
                        controlDescriptionViewModel.ControlName = (this.Controls.Count() + 1).ToString();
                        controlDescriptionViewModel.ImageSource = ConvertToImageSource(scrapedControl.ControlImage);

                        //save the captured control details to file
                        await AddControlAsync(controlDescriptionViewModel);
                        logger.Information($"Added control with details {controlDescription}");
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, ex.Message);
                    }
                }
            }
            await Task.CompletedTask;
        }     
    }
}
