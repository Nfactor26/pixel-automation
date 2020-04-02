using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core;
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
        private readonly string applicationsRepository = "ApplicationsRepository";
        private readonly string controlsDirectory = "Controls";


        private readonly ILogger logger = Log.ForContext<ControlExplorerViewModel>();
        private readonly ISerializer serializer;
        private readonly ITypeProvider typeProvider;       
        private readonly IControlEditor controlEditor;
        private readonly IEventAggregator eventAggregator;
        private readonly IWindowManager windowManager;


        private ApplicationDescription activeApplication;

        public BindableCollection<ControlDescription> Controls { get; set; } = new BindableCollection<ControlDescription>();

        private ControlDescription selectedControl;
        public ControlDescription SelectedControl
        {
            get => selectedControl;
            set
            {
                selectedControl = value;
                CanEdit = false;
            }
        }

        public ControlExplorerViewModel(IWindowManager windowManager,IEventAggregator eventAggregator, IControlEditor controlEditor, ISerializer serializer, ITypeProvider typeProvider)
        {
            this.windowManager = windowManager;
            this.serializer = serializer;
            this.typeProvider = typeProvider;
            this.eventAggregator = eventAggregator;
            this.eventAggregator.SubscribeOnPublishedThread(this);
            this.controlEditor = controlEditor;

            CreateCollectionView();
        }
                

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

        public void ToggleRename(ControlDescription targetControl)
        {
            if (SelectedControl == targetControl)
                CanEdit = !CanEdit;
        }

        /// <summary>
        /// Update the name of ControlToolBoxItem
        /// </summary>
        /// <param name="context"></param>
        /// <param name="controlToRename"></param>
        public void RenameControl(ActionExecutionContext context, ControlDescription controlToRename)
        {
            try
            {
                var keyArgs = context.EventArgs as KeyEventArgs;
                if (keyArgs != null && keyArgs.Key == Key.Enter)
                {
                    string newName = (context.Source as System.Windows.Controls.TextBox).Text;
                    controlToRename.ControlName = newName;
                    CanEdit = false;                 
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                CanEdit = false;
            }
        }


        #endregion Edit ControlToolBoxItem

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
            groupedItems.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ControlDescription.GroupName)));
            groupedItems.SortDescriptions.Add(new SortDescription(nameof(ControlDescription.GroupName), ListSortDirection.Ascending));
            groupedItems.SortDescriptions.Add(new SortDescription(nameof(ControlDescription.ControlName), ListSortDirection.Ascending));          
            groupedItems.Filter = new Predicate<object>((a) =>
            {
                return (a as ControlDescription).ControlName.ToLower().Contains(this.filterText.ToLower());
            });
        }

        #endregion Filter


        public void SetActiveApplication(ApplicationDescription application)
        {
            this.activeApplication = application;
            LoadControlDetails(application);
            
            this.Controls.Clear();
            this.Controls.AddRange(this.activeApplication.ControlsCollection);              
        }     

        private void LoadControlDetails(ApplicationDescription application)
        {
            if (application.ControlsCollection.Count() == 0)
            {
                foreach (var controlId in application.AvailableControls)
                {
                    string controlFile = Path.Combine("ApplicationsRepository", application.ApplicationId, "Controls", controlId, $"{controlId}.dat");
                 
                    if(File.Exists(controlFile))
                    {
                        ControlDescription controlDescription = serializer.Deserialize<ControlDescription>(controlFile);
                        application.ControlsCollection.Add(controlDescription);
                    }
                    logger.Warning("Prefab file : {0} doesn't exist", controlFile);
                }
            }          
        }

        /// <summary>
        /// Delete the control
        /// </summary>
        /// <param name="controlToDelete"></param>
        public void DeleteControl(ControlDescription controlToDelete)
        {
            Guard.Argument(controlToDelete).NotNull();

            try
            {
                //delete the .control file
                string controlDirectory = GetControlDirectory(controlToDelete);
                if (Directory.Exists(controlDirectory))
                {
                    Directory.Delete(controlDirectory, true);
                }

                this.Controls.Remove(controlToDelete);

                OnControlDeleted(controlToDelete);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
            }

        }


        public async void EditControl(ControlDescription controlToEdit)
        {
            Guard.Argument(controlToEdit).NotNull();

            var copyOfControlToEdit = controlToEdit.ControlDetails.Clone() as IControlIdentity;
            controlEditor.Initialize(copyOfControlToEdit);
            var result = await windowManager.ShowDialogAsync(controlEditor);
            if (result.HasValue && result.Value)
            {
                controlToEdit.ControlDetails = copyOfControlToEdit;
                SaveControlDetails(controlToEdit);
            }
        }

        /// <summary>
        /// Create a copy of control
        /// </summary>
        /// <param name="controlToRename"></param>
        public void CloneControl(ControlDescription controlToClone)
        {
            Guard.Argument(controlToClone).NotNull();

            var clonedControl = controlToClone.Clone() as ControlDescription;
            clonedControl.ControlName = Path.GetRandomFileName();
            Directory.CreateDirectory(GetControlDirectory(clonedControl));
            SaveBitMapSource((controlToClone as ControlDescriptionEx).ImageSource, GetControlImageFile(clonedControl));
            SaveControlDetails(clonedControl);
            this.Controls.Add(clonedControl);
            OnControlCreated(clonedControl);
        }


        private void AddControl(ControlDescription controlItem)
        {
            Directory.CreateDirectory(GetControlDirectory(controlItem));
            SaveBitMapSource((controlItem as ControlDescriptionEx).ImageSource, GetControlImageFile(controlItem));
            SaveControlDetails(controlItem);
            this.Controls.Add(controlItem);
         
            OnControlCreated(controlItem);
        }


        private void SaveControlDetails(ControlDescription controlToSave)
        {
            string fileToCreate = GetControlFile(controlToSave);
            if(File.Exists(fileToCreate))
            {
                File.Delete(fileToCreate);
            }
            serializer.Serialize(fileToCreate, controlToSave);
        }

        private string GetControlFile(ControlDescription controlItem)
        {
           return Path.Combine(GetControlDirectory(controlItem), $"{controlItem.ControlId}.dat");
        }

        private string GetControlImageFile(ControlDescription controlItem)
        {
            return Path.Combine(GetControlDirectory(controlItem), "ScreenShot.Png");
        }

        private string GetControlDirectory(ControlDescription controlItem)
        {
            return Path.Combine(applicationsRepository, this.activeApplication.ApplicationId, controlsDirectory , controlItem.ControlId);
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

        private void SaveBitMapSource(ImageSource imageSource, string saveLocation)
        {
            if (imageSource is BitmapImage image)
            {               
                using (var fileStream = new FileStream(saveLocation, FileMode.Create))
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(fileStream);
                }
                return;
            }
            throw new ArgumentException($"{nameof(imageSource)} must be a BitmapImage");
        }


        public async Task HandleAsync(IEnumerable<ScrapedControl> scrapedControls, CancellationToken cancellationToken)
        {
            Log.Information("Received {count} scraped controls to process", scrapedControls.Count());
            if (this.activeApplication == null)
            {
                throw new InvalidOperationException("There is no active application in Application explorer");
            }
            foreach (ScrapedControl scrapedControl in scrapedControls)
            {
                using (scrapedControl)
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
                    ControlDescriptionEx controlItem = new ControlDescriptionEx(control);
                    controlItem.ControlName = (this.Controls.Count() + 1).ToString();
                    controlItem.ControlImage = GetControlImageFile(controlItem);
                    controlItem.ImageSource = ConvertToImageSource(scrapedControl.ControlImage);

                    control.ControlImage = controlItem.ControlImage;
                    //save the captured control details to file
                    AddControl(controlItem);
                }
            }
            await Task.CompletedTask;
        }

        public event EventHandler<ControlDescription> ControlCreated = delegate { };
        protected virtual void OnControlCreated(ControlDescription createdPrefab)
        {
            this.ControlCreated(this, createdPrefab);
        }

        public event EventHandler<ControlDescription> ControlDeleted = delegate { };
        protected virtual void OnControlDeleted(ControlDescription deletedPrefab)
        {
            this.ControlDeleted(this, deletedPrefab);
        }     
    }
}
