using System;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Controls
{
    /// <summary>
    /// A BoundBox indicates a rectangular area on screen that can be used to describe the location of a control on screen
    /// </summary>
    [DataContract]
    [Serializable]  
    public class BoundingBox
    {
        public static BoundingBox Empty = new BoundingBox(0, 0, 0, 0);

        /// <summary>
        /// X-Coordinate from top left of screen
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Y-Coordinate from top left of screen
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Width of the rectangular area
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Height of the rectangular area
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public BoundingBox()
        {

        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="x">X-Coordinate from top left of screen</param>
        /// <param name="y">Y-Coordinate from top left of screen</param>
        /// <param name="width"> Width of the rectangular area</param>
        /// <param name="height"> Height of the rectangular area</param>
        public BoundingBox(int x, int y, int width, int height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        /// <summary>
        /// Check if specified coordinate is contained within the bounding box
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Contains(double x, double y)
        {           
            return x > this.X && x < this.X + this.Width && y > this.Y && y < this.Y + this.Height;
        }

        /// <summary>
        /// Two bounding box are equal if they represent the same rectangula area on screen
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
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
