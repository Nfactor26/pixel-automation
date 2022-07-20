using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Devices
{
    /// <summary>
    /// Represents width and height of a application window
    /// </summary>
    [DataContract]
    [Serializable]
    public class Dimension
    {
        /// <summary>
        /// Width
        /// </summary>
        [DataMember]
        public int Width { get; set; }

        /// <summary>
        /// Height
        /// </summary>
        [DataMember]
        public int Height { get; set; }

        /// <summary>
        /// Dimension with 0 width and height
        /// </summary>
        public static Dimension ZeroExtents = new Dimension(0, 0);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Dimension(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }
    }
}
