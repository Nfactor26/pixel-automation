using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Models
{
    [DataContract]
    [Serializable]
    public abstract class UIControl
    {
        public object TargetControl { get; set; }

        public Rectangle BoundingBox { get; set; }

        public Point ClickablPoint { get; set; }

        public T GetApiControl<T>()
        {
            if (this.TargetControl is T)
            {
                return (T)TargetControl;
            }
            throw new InvalidCastException($"{typeof(T)} is incompatible with {TargetControl.GetType()}");
        }

        public abstract Rectangle GetBoundingBox();

        public abstract void GetClickablePoint(out double x, out double y);
    }
}
