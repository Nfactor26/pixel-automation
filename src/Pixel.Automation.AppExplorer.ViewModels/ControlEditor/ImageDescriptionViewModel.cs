using Dawn;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Models;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Pixel.Automation.AppExplorer.ViewModels.ControlEditor
{
    public class ImageDescriptionViewModel : NotifyPropertyChanged
    {
        private readonly ImageDescription imageDescription;
        [Browsable(false)]
        public ImageDescription ImageDescription 
        {
            get => imageDescription;  
        }

        private string displayName;
        [Browsable(false)]
        public string DisplayName
        {
            get => Path.GetFileName(imageDescription.ControlImage);            
        }

        [Display(Name = "Image File", Order = 20)]
        [ReadOnly(true)]
        public string ControlImage
        {
            get => imageDescription.ControlImage;
            set
            {
                imageDescription.ControlImage = value;
                OnPropertyChanged();
            }
        }
          
        [Display(Name = "Pivot Point", Order = 20, GroupName = "Clickable Point")]
        [ReadOnly(true)]
        public Pivots PivotPoint
        {
            get => imageDescription.PivotPoint;
            set
            {
                imageDescription.PivotPoint = value;
                OnPropertyChanged();
            }
        }
        
        [Description("X offset to be added to control top left coordinates if simulating mouse actions on this control")]
        [Display(Name = "X-Offset", Order = 20, GroupName = "Clickable Point")]
        [ReadOnly(true)]
        public double XOffSet
        {
            get
            {
                return imageDescription.XOffSet;
            }
            set
            {
                imageDescription.XOffSet = value;
                OnPropertyChanged();
            }
        }
      
        [Description("Y offset to be added to control top left coordinates if simulating mouse actions on this control")]
        [Display(Name = "Y-Offset", Order = 30, GroupName = "Clickable Point")]
        [ReadOnly(true)]
        public double YOffSet
        {
            get
            {
                return imageDescription.YOffSet;
            }
            set
            {
                imageDescription.YOffSet = value;
                OnPropertyChanged();
            }
        }
      
        [Description("Use this image when screen resolution width matches specified ScreenWidth")]
        [Display(Name = "Screen Width", Order = 30, GroupName = "Match Criteria")]
        public short ScreenWidth
        {
            get => imageDescription.ScreenWidth;
            set
            {
                imageDescription.ScreenWidth = value;
                OnPropertyChanged();
            }
        }
    
        [Description("Use this image when screen resolution height matches specified ScreenWidth")]
        [Display(Name = "Screen Height", Order = 30, GroupName = "Match Criteria")]
        public short ScreenHeight
        {
            get => imageDescription.ScreenHeight;
            set
            {
                imageDescription.ScreenHeight = value;
                OnPropertyChanged();
            }
        }
       
        [Description("Use this image when specified theme on image locator matches this theme")]
        [Display(Name = "Theme", Order = 30, GroupName = "Match Criteria")]      
        public string Theme
        {
            get => imageDescription.Theme;
            set
            {
                imageDescription.Theme = value;
                OnPropertyChanged();
            }
        }

        private ImageSource imageSource;
        [Browsable(false)]
        public ImageSource ImageSource
        {
            get
            {
                if(imageSource == null)
                {
                    if (!string.IsNullOrEmpty(this.ControlImage))
                    {
                        var source = new BitmapImage();
                        source.BeginInit();
                        source.CacheOption = BitmapCacheOption.OnLoad;
                        source.UriSource = new Uri(this.ControlImage, UriKind.Relative);
                        source.EndInit();
                        this.imageSource = source;
                    }
                }
                return imageSource;
            }
        }

        [Browsable(false)]
        public bool IsMarkedForDelete { get; set; }

        [Browsable(false)]
        public bool IsNewAddition { get; set; }
       
        public ImageDescriptionViewModel(ImageDescription imageDescription)
        {
            Guard.Argument(imageDescription).NotNull();
            this.imageDescription = imageDescription;            
        }
    }
}
