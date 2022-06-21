using System;
using System.Drawing;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Pixel.Automation.Core.Controls
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

        public abstract Task<Rectangle> GetBoundingBoxAsync();

        public abstract Task<(double,double)> GetClickablePointAsync();
    }
}
