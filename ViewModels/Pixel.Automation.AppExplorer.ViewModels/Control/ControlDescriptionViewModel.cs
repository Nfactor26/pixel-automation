using Pixel.Automation.Core;
using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Pixel.Automation.AppExplorer.ViewModels.Control
{
    public class ControlDescriptionViewModel : NotifyPropertyChanged
    {
        private readonly ControlDescription controlDescription;

        [Browsable(false)]
        public ControlDescription ControlDescription
        {
            get => this.controlDescription;
        }
        
        public ControlDescriptionViewModel(ControlDescription controlDescription)
        {
            this.controlDescription = controlDescription;
        }
              
        [Browsable(false)]
        public string ApplicationId
        {
            get => this.controlDescription.ApplicationId;
            set => this.controlDescription.ApplicationId = value;
        }

        [Browsable(false)]
        public string ControlId
        {
            get => this.controlDescription.ControlId;
            set => this.controlDescription.ControlId = value;
        }
     
        public string GroupName
        {
            get => this.controlDescription.GroupName;
            set => this.controlDescription.GroupName = value;
        }

        public string ControlName
        {
            get => this.controlDescription.ControlName;
            set => this.controlDescription.ControlName = value;
        }
      
        [Browsable(false)]
        public string ControlImage
        {
            get => this.controlDescription.ControlImage;
            set => this.controlDescription.ControlImage = value;
        }

        [Browsable(false)]
        public IControlIdentity ControlDetails
        {
            get => this.controlDescription.ControlDetails;
            set => this.controlDescription.ControlDetails = value;
        }

      
        ImageSource imageSource;

        [Browsable(false)]
        public ImageSource ImageSource
        {
            get
            {
                if (imageSource == null)
                {
                    if (!string.IsNullOrEmpty(this.controlDescription.ControlImage))
                    {
                        if (!File.Exists(this.controlDescription.ControlImage))
                        {
                            return null;  //TODO : Provide a default image
                        }                     
                        var source = new BitmapImage();
                        source.BeginInit();
                        source.CacheOption = BitmapCacheOption.OnLoad;
                        source.UriSource = new Uri(this.controlDescription.ControlImage, UriKind.Relative);
                        source.EndInit();
                        imageSource = source;
                    }
                }
                return imageSource;
            }
            set
            {
                imageSource = value;
                OnPropertyChanged();
            }
        }
    }
}
