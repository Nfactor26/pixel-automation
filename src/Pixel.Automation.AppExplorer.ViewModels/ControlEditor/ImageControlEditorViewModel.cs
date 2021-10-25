using Dawn;
using Microsoft.Win32;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using Pixel.Automation.Editor.Core.Interfaces;
using Pixel.Persistence.Services.Client;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Pixel.Automation.AppExplorer.ViewModels.ControlEditor
{
    public  class ImageControlEditorViewModel : ControlEditorBaseViewModel, IImageControlEditor
    {
        private readonly ILogger logger = Log.ForContext<ImageControlEditorViewModel>();
        private readonly IApplicationDataManager applicationDataManager;

        private ControlDescription controlDescription;
        private IImageControlIdentity controlIdentity;          
     
        public ObservableCollection<ImageDescriptionViewModel> ControlImages { get; private set; } = new ();

        private object selectedObject;
        public object SelectedObject
        {
            get => selectedObject;
            set
            {
                selectedObject = value;
                NotifyOfPropertyChange(nameof(SelectedObject));
            }
        }

        ImageDescriptionViewModel selectedImage;
        public ImageDescriptionViewModel SelectedImage
        {
            get => selectedImage;
            set
            {
                selectedImage = value;    
                if(value != null)
                {
                    ShowOffSetPointer();
                    IsOffsetPoiniterVisible = Visibility.Visible;
                    SelectedObject = value;                 
                }
                else
                {
                    IsOffsetPoiniterVisible = Visibility.Hidden;
                }
                NotifyOfPropertyChange(nameof(SelectedImage));
                NotifyOfPropertyChange(nameof(CanDeleteImage));
                NotifyOfPropertyChange(nameof(IsImageSelected));
            }
        }   
        
        public bool IsImageSelected
        {
            get => this.selectedImage != null;
        }

        public ImageControlEditorViewModel(IApplicationDataManager applicationDataManager)
        {
            this.DisplayName = "Control Editor";
            this.applicationDataManager = Guard.Argument(applicationDataManager).NotNull().Value;
            CreateCollectionView();
        }

        private void CreateCollectionView()
        {
            var groupedItems = CollectionViewSource.GetDefaultView(ControlImages);          
            groupedItems.Filter = new Predicate<object>((a) =>
            {
                if(a is ImageDescriptionViewModel imageDescription)
                {
                    return !imageDescription.IsMarkedForDelete;
                }
                return false;
            });
        }

        public void ConfigureControl()
        {
            this.SelectedImage = null;
            this.SelectedObject = this.controlIdentity;
        }
     
        public void AddImageFromFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PNG File (*.Png)|*.Png";
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = openFileDialog.FileName;
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    var imageDescription = new ImageDescription()
                    {
                        ControlImage = fileName,
                        PivotPoint = Pivots.Center
                    };                                  
                    this.ControlImages.Add(new ImageDescriptionViewModel(imageDescription) { IsNewAddition = true });                    
                }
            }
        }

        public bool CanDeleteImage
        {
            get => this.SelectedImage != null && this.ControlImages.Where(a => !a.IsMarkedForDelete).Count() > 1;
        }

        public void DeleteImage()
        { 
            if(this.SelectedImage != null && this.ControlImages.Where(a => !a.IsMarkedForDelete).Count() > 1)
            {
                var imageDescription = this.SelectedImage;
                if (this.ControlImages.Contains(imageDescription))
                {
                    int index = this.ControlImages.IndexOf(imageDescription);
                    this.SelectedImage = this.ControlImages[Math.Max(index - 1, 0)];
                    imageDescription.IsMarkedForDelete = true;
                    var view = CollectionViewSource.GetDefaultView(this.ControlImages);
                    view.Refresh();
                    NotifyOfPropertyChange(nameof(CanDeleteImage));
                }
            }       
        }       

        protected override void OnViewReady(object view)
        {
            this.view = view as FrameworkElement;          
            if (this.ControlImages.Count > 0)
            {                   
                this.SelectedImage = this.ControlImages.ElementAt(0);
            }
            base.OnViewReady(view);
        }

        public void Initialize(ControlDescription controlDescription)
        {
            this.controlDescription = Guard.Argument(controlDescription).NotNull();
            if(controlDescription.ControlDetails is IImageControlIdentity imageControlIdentity)
            {
                this.controlIdentity = imageControlIdentity;            
                foreach (var imageDescription in this.controlIdentity.GetImages())
                {
                    this.ControlImages.Add(new ImageDescriptionViewModel(imageDescription));
                }
            }           
        }

        protected override (double width, double height) GetImageDimension()
        {
            return (SelectedImage.ImageSource.Width, SelectedImage.ImageSource.Height);
        }

        protected override System.Drawing.Point GetOffSet()
        {
            return new System.Drawing.Point((int)this.SelectedImage.XOffSet, (int)this.SelectedImage.YOffSet);
        }

        protected override void SetOffSet(System.Drawing.Point offSet)
        {
            this.SelectedImage.XOffSet = offSet.X;
            this.SelectedImage.YOffSet = offSet.Y;
        }

        protected override Pivots GetPivotPoint()
        {
            return this.SelectedImage.PivotPoint;
        }

        protected override void SetPivotPoint(Pivots pivotPoint)
        {
            this.SelectedImage.PivotPoint = pivotPoint;
        }
       
        public async void Save()
        {
            try
            {
                foreach (var image in this.ControlImages)
                {
                    if (image.IsNewAddition)
                    {
                        using (FileStream fs = new FileStream(image.ControlImage, FileMode.Open, FileAccess.Read))
                        {
                            image.ControlImage = await this.applicationDataManager.AddOrUpdateControlImageAsync(this.controlDescription, fs);
                        }
                        this.controlIdentity.AddImage(image.ImageDescription);
                    }
                    if (image.IsMarkedForDelete)
                    {
                        await this.applicationDataManager.DeleteControlImageAsync(this.controlDescription, image.ControlImage);
                        this.controlIdentity.DeleteImage(image.ImageDescription);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error occured while saving changes in image control editor", ex);
            }
            await this.TryCloseAsync(true);
        }

        public async void Cancel()
        {           
            await this.TryCloseAsync(false);
        }
    }
}
