using Pixel.Automation.Core.Attributes;
using Pixel.Automation.Core.Components;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;

namespace Pixel.Automation.Image.Matching.Components
{
    [DataContract]
    [Serializable]
    [ContainerEntity(typeof(ImageControlEntity))]
    public class ImageControlIdentity : ControlIdentity
    {
        [NonSerialized]

        OpenCvSharp.TemplateMatchModes matchStrategy = OpenCvSharp.TemplateMatchModes.CCoeffNormed;
        [DataMember]
        [Display(Name = "Match Strategy", GroupName = "Configuration")]
        [Description("Specifies the way the template must be compared with image regions")]
        public OpenCvSharp.TemplateMatchModes MatchStrategy
        {
            get => matchStrategy;
            set => matchStrategy = value;
        }

        float thresHold = 0.9f;
        [DataMember]
        [Display(Name = "Threshold", GroupName = "Configuration")]
        [Description("Minimum match percentage for acceptance")]    
        public float ThreshHold
        {
            get => thresHold;
            set => thresHold = value;
        }

        public ImageControlIdentity() : base()
        {

        }

        /// <summary>
        /// Get the template image for a given screen resolution
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public string GetTemplateImage(Size size)
        {
            return this.ControlImage;
        }

        public override object Clone()
        {
            ImageControlIdentity clone = new ImageControlIdentity()
            {
                Name = this.Name,
                ApplicationId = this.ApplicationId,
                ControlImage = this.ControlImage,
                BoundingBox = this.BoundingBox,
                PivotPoint = this.PivotPoint,
                XOffSet = this.XOffSet,
                YOffSet = this.YOffSet,
                RetryAttempts = this.RetryAttempts,
                RetryInterval = this.RetryInterval,
                SearchScope = this.SearchScope,
                MatchStrategy = this.MatchStrategy,
                ThreshHold = this.ThreshHold,
                Next = this.Next?.Clone() as ImageControlIdentity

            };
            return clone;
        }

        public override string ToString()
        {
            return $"{this.Name} -> MatchStrategy:{this.matchStrategy}|Threshold:{this.thresHold}|ControlImage:{Path.GetFileName(this.ControlImage)}";
        }

    }
}
