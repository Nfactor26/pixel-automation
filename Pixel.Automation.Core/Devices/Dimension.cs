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
        [DataMember]
        public int Width { get; set; }

        [DataMember]
        public int Height { get; set; }

        public static Dimension ZeroExtents = new Dimension(0, 0);

        public Dimension(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }
    }
}
