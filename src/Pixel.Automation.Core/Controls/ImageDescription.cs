using Pixel.Automation.Core.Enums;
using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Controls
{
    /// <summary>
    /// <see cref="ImageDescription"/> stores the details of image of a control.
    /// </summary>
    [DataContract]
    [Serializable]
    public class ImageDescription : ICloneable
    {
        /// <summary>
        /// Relative path of the image
        /// </summary>
        [DataMember(Order = 10)]
        public string ControlImage { get; set; }
    
        /// <summary>
        /// Pivot point used for offset
        /// </summary>
        [DataMember(Order = 20)]     
        public Pivots PivotPoint { get; set; } = Pivots.Center;

        /// <summary>
        /// X offset to be added to control top left coordinates if simulating mouse actions on this control
        /// </summary>
        [DataMember(Order = 30)]    
        public double XOffSet { get; set; } = 0.0f;

        /// <summary>
        /// Y offset to be added to control top left coordinates if simulating mouse actions on this control
        /// </summary>
        [DataMember(Order = 40)]
        public double YOffSet { get; set; } = 0.0f;

        /// <summary>
        /// Use this image when screen resolution width matches specified ScreenWidth
        /// </summary>
        [DataMember(Order = 50)]    
        public short ScreenWidth { get; set; } = 0;
        
        /// <summary>
        /// Use this image when screen resolution height matches specified ScreenWidth
        /// </summary>
        [DataMember(Order = 60)]      
        public short ScreenHeight { get; set; } = 0;

        /// <summary>
        /// Use this image when specified theme on image locator matches this theme
        /// </summary>
        [DataMember(Order = 70)]
        public string Theme { get; set; } = string.Empty;
        
        /// <summary>
        /// Indicates if this is the default image amongst multiple configured images
        /// </summary>
        public  bool IsDefault
        {
            get => string.IsNullOrEmpty(this.Theme) && this.ScreenHeight == 0 && this.ScreenWidth == 0;
        }

        /// <inheritdoc/>
        public object Clone()
        {
            return new ImageDescription()
            {
                ControlImage = this.ControlImage,
                PivotPoint = this.PivotPoint,
                XOffSet = this.XOffSet,
                YOffSet = this.YOffSet,
                ScreenHeight = this.ScreenHeight,
                ScreenWidth = this.ScreenWidth,
                Theme = this.Theme
            };
        }
    }
}
