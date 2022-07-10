using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Controls
{
    [DataContract]
    [Serializable]
    public class BoundingBox
    {
        public static BoundingBox Empty = new BoundingBox(0, 0, 0, 0);

        public int X { get; set; }

        public int Y { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public BoundingBox()
        {

        }

        public BoundingBox(int x, int y, int width, int height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public override bool Equals(object obj)
        {
            if(obj is BoundingBox other)
            {
                return other.X == this.X && other.Y == this.Y && other.Width == this.Width && other.Height == this.Height;
            }
            return false;
        }
    }
}
