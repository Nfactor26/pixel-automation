using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.Models
{
    [DataContract]
    [Serializable]
    public abstract class UIControl
    {
        protected object TargetControl { get; set; }

        public virtual T GetApiControl<T>()
        {
            if (this.TargetControl is T requiredControl)
            {
                return requiredControl;
            }
            throw new InvalidCastException($"{typeof(T)} is incompatible with {TargetControl.GetType()}");
        }

        public abstract Rectangle GetBoundingBox();

        public abstract void GetClickablePoint(out double x, out double y);
    }
}
