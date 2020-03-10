using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Enums
{
    [DataContract]
    [Serializable]
    public class Scale
    {
        [DataMember]
        public int ScaleX { get; set; }

        [DataMember]
        public int ScaleY { get; set; }

        public Scale()
        {

        }

        public Scale(int scaleX, int scaleY)
        {
            this.ScaleX = scaleX;
            this.ScaleY = scaleY;
        }
    }
}
