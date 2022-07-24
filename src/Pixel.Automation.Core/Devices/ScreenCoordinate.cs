using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Devices
{
    /// <summary>
    /// <see cref="ScreenCoordinate"/> identifes a point on screen
    /// </summary>
    [DataContract]
    [Serializable]
    public class ScreenCoordinate
    {
        /// <summary>
        /// XCoordinate from top left of the screen
        /// </summary>
        [DataMember]
        public int XCoordinate { get; set; }

        /// <summary>
        /// YCoordinate from top left of the screen
        /// </summary>
        [DataMember]
        public int YCoordinate { get; set; }
      
        /// <summary>
        /// Default constructor
        /// </summary>
        public ScreenCoordinate()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public ScreenCoordinate(int x, int y)
        {
            this.XCoordinate = x;
            this.YCoordinate = y;           
        }
       
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public ScreenCoordinate(double x, double y)
        {
            this.XCoordinate = (int)x;
            this.YCoordinate = (int)y;           
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{this.XCoordinate},{this.YCoordinate}";
        }
    }
}
