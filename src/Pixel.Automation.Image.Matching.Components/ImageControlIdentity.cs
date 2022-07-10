using Pixel.Automation.Core;
using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Controls;
using Pixel.Automation.Core.Enums;
using Pixel.Automation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.Image.Matching.Components
{
    [DataContract]
    [Serializable]
    [ContainerEntity(typeof(ImageControlEntity))]
    public class ImageControlIdentity : NotifyPropertyChanged, IImageControlIdentity
    {
        #region  General 

        [DataMember(Order = 10)]
        [Browsable(false)]
        public string ApplicationId { get; set; }

        [DataMember(Order = 20)]
        [Browsable(false)]
        public string Name { get; set; }

        protected BoundingBox boundingBox;
        [DataMember(Order = 40)]       
        [Browsable(false)]
        public BoundingBox BoundingBox
        {
            get => boundingBox;
            set => boundingBox = value;
        }

        #endregion General

        #region Retry Attempts

        [DataMember(Order = 50)]
        [Description("Number of times to retry if control can't be located in first attempt")]
        [Display(Name = "Retry Attempts", Order = 10, GroupName = "Retry Settings")]
        public int RetryAttempts { get; set; } = 5;

        [DataMember(Order = 60)]
        [Description("Interval in seconds between each retry attempt")]
        [Display(Name = "Retry Interval", Order = 20, GroupName = "Retry Settings")]
        public int RetryInterval { get; set; } = 2;

        #endregion Retry Attempts

        #region Clickable Point Offset
      
        [DataMember(Order = 70, IsRequired = false)]
        [Browsable(false)]
        public  Pivots PivotPoint { get; set; }     

       
        [DataMember(Order = 80, IsRequired = false)]
        [Browsable(false)]
        public double XOffSet { get; set; } 

       
        [DataMember(Order = 90, IsRequired = false)]
        [Browsable(false)]
        public  double YOffSet { get; set; }
        
        #endregion Clickable Point Offset

        #region Search Strategy

        [DataMember(Order = 100, IsRequired = false)]
        [Browsable(false)]
        public LookupType LookupType { get; set; } = LookupType.Default;

        [DataMember(Order = 110, IsRequired = false)]
        [Browsable(false)]
        public SearchScope SearchScope { get; set; } = SearchScope.Descendants;

        [DataMember(Order = 120, IsRequired = false)]
        [Browsable(false)]
        public int Index { get; set; } = 1;

        #endregion Search Strategy

        [NonSerialized]
        OpenCvSharp.TemplateMatchModes matchStrategy = OpenCvSharp.TemplateMatchModes.CCoeffNormed;
        [DataMember(Order = 210)]
        [Display(Name = "Match Strategy", GroupName = "Configuration")]
        [Description("Specifies the way the template must be compared with image regions")]
        public OpenCvSharp.TemplateMatchModes MatchStrategy
        {
            get => matchStrategy;
            set => matchStrategy = value;
        }

        float thresHold = 0.9f;
        [DataMember(Order = 220)]
        [Display(Name = "Threshold", GroupName = "Configuration")]
        [Description("Minimum match percentage for acceptance")]    
        public float ThreshHold
        {
            get => thresHold;
            set => thresHold = value;
        }

        [DataMember(Order = 230)]
        [Browsable(false)]
        public List<ImageDescription> ControlImages { get; set; } = new ();

        [DataMember(Order = 1000, IsRequired = false)]
        [Browsable(false)]
        public IControlIdentity Next { get; set; }

        public ImageControlIdentity() : base()
        {

        }

        public IEnumerable<ImageDescription> GetImages()
        {
            return this.ControlImages;
        }

        public void AddImage(ImageDescription imageDescription)
        {            
            if(!this.ControlImages.Any(a => a.ControlImage.Equals(imageDescription.ControlImage)))
            {
                this.ControlImages.Add(imageDescription);
            }
        }

        public void DeleteImage(ImageDescription imageDescription)
        {
            var imageToDelete = this.ControlImages.FirstOrDefault(a => a.ControlImage.Equals(imageDescription.ControlImage));
            if(imageToDelete != null)
            {
                this.ControlImages.Remove(imageToDelete);
            }
        }

        public string GetControlName()
        {
            return this.Name;
        }

        public object Clone()
        {
            ImageControlIdentity clone = new ImageControlIdentity()
            {
                Name = this.Name,
                ApplicationId = this.ApplicationId,              
                BoundingBox = this.BoundingBox,
                PivotPoint = this.PivotPoint,
                XOffSet = this.XOffSet,
                YOffSet = this.YOffSet,
                RetryAttempts = this.RetryAttempts,
                RetryInterval = this.RetryInterval,
                SearchScope = this.SearchScope,
                MatchStrategy = this.MatchStrategy,
                ThreshHold = this.ThreshHold,
                ControlImages = new List<ImageDescription>()
            };
            foreach(var imageDescription in this.ControlImages)
            {
                clone.ControlImages.Add(imageDescription.Clone() as ImageDescription);
            }

            return clone;
        }

        public override string ToString()
        {
            return $"{this.Name} -> MatchStrategy:{this.matchStrategy}|Threshold:{this.thresHold}";
        }       
    }
}
