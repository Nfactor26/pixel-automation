using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Models
{
    [DataContract]
    [Serializable]
    public class BoundingBox
    {
        public int X { get; set; }

        public int Y { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public BoundingBox()
        {

        }

        public BoundingBox(int x, int y , int width, int height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public Rectangle GetBoundingBoxAsRectangle()
        {
            return new Rectangle(X, Y, Width, Height);
        }
    }
}
