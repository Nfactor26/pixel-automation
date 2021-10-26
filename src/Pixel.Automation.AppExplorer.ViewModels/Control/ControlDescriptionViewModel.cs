using Pixel.Automation.Core;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Interfaces;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Pixel.Automation.AppExplorer.ViewModels.Control
{
    /// <summary>
    /// View Model for <see cref="ControlDescription"/>
    /// </summary>
    public class ControlDescriptionViewModel : NotifyPropertyChanged
    {
        private readonly ControlDescription controlDescription;

        [Browsable(false)]
        public ControlDescription ControlDescription
        {
            get => this.controlDescription;
        }
        
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="controlDescription"></param>
        public ControlDescriptionViewModel(ControlDescription controlDescription)
        {
            this.controlDescription = controlDescription;
        }
             
        /// <summary>
        /// Id of the owner application
        /// </summary>
        [Browsable(false)]
        public string ApplicationId
        {
            get => this.controlDescription.ApplicationId;           
        }

        /// <summary>
        /// Id of the control
        /// </summary>
        [Browsable(false)]
        public string ControlId
        {
            get => this.controlDescription.ControlId;          
        }
     
        /// <summary>
        /// Group assigned to control
        /// </summary>
        public string GroupName
        {
            get => this.controlDescription.GroupName;
            set
            {
                this.controlDescription.GroupName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Name of the control
        /// </summary>
        public string ControlName
        {
            get => this.controlDescription.ControlName;
            set
            {
                this.controlDescription.ControlName = value;
                OnPropertyChanged();
            }
        }
      
        /// <summary>
        /// Image file path for the control
        /// </summary>
        [Browsable(false)]
        public string ControlImage
        {
            get => this.controlDescription.ControlImage;
            set => this.controlDescription.ControlImage = value;
        }

        /// <summary>
        /// Wrapped IControlIdentity
        /// </summary>
        [Browsable(false)]
        public IControlIdentity ControlDetails
        {
            get => this.controlDescription.ControlDetails;
            set => this.controlDescription.ControlDetails = value;
        }

      
        ImageSource imageSource;
        /// <summary>
        /// ImageSource created from ControlImage
        /// </summary>
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

        ///<inheritdoc/>
        public override string ToString()
        {
            return "Control Details";
        }
    }
}
