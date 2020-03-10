using Pixel.Automation.Core.Interfaces;
using Pixel.Automation.Core.Models;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Pixel.Automation.AppExplorer.ViewModels.Control
{
    public class ControlDescriptionEx : ControlDescription
    {

        public ControlDescriptionEx()
        {

        }

        public ControlDescriptionEx(IControlIdentity controlIdentity) : base(controlIdentity)
        {

        }

        [NonSerialized]
        ImageSource imageSource;

        [IgnoreDataMember]
        public ImageSource ImageSource
        {
            get
            {
                if (imageSource == null)
                {
                    if (!string.IsNullOrEmpty(this.ControlImage))
                    {
                        if (!File.Exists(this.ControlImage))
                            return null;  //TODO : Provide a default image
                        var source = new BitmapImage();
                        source.BeginInit();
                        source.CacheOption = BitmapCacheOption.OnLoad;
                        source.UriSource = new Uri(this.ControlImage, UriKind.Relative);
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
