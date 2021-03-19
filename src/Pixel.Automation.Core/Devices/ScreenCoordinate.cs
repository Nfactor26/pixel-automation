using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Devices
{
    [DataContract]
    [Serializable]
    public class ScreenCoordinate
    {
        [DataMember]
        public int XCoordinate { get; set; }

        [DataMember]
        public int YCoordinate { get; set; }
      

        public ScreenCoordinate()
        {

        }

        public ScreenCoordinate(int x, int y)
        {
            this.XCoordinate = x;
            this.YCoordinate = y;           
        }
       

        public ScreenCoordinate(double x, double y)
        {
            this.XCoordinate = (int)x;
            this.YCoordinate = (int)y;           
        }
     
        public override string ToString()
        {
            return $"{this.XCoordinate},{this.YCoordinate}";
        }
    }
}
