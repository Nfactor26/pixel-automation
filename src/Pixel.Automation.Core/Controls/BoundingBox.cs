using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Controls
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

        public BoundingBox(Rectangle rectangle)
        {
            this.X = rectangle.X;
            this.Y = rectangle.Y;
            this.Width = rectangle.Width;
            this.Height = rectangle.Height;
        }

        public Rectangle GetBoundingBoxAsRectangle()
        {
            return new Rectangle(X, Y, Width, Height);
        }
    }
}
