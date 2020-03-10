using System;

namespace Pixel.Automation.Core.Args
{
    public class PropertyGridObjectEventArgs : EventArgs
    {
        public object ObjectToDisplay { get; set; }

        public bool IsReadOnly { get; set; }

        public PropertyGridObjectEventArgs(Object objectToDisplay):base()
        {
            this.ObjectToDisplay = objectToDisplay;
        }

        public PropertyGridObjectEventArgs(Object objectToDisplay, bool isReadOnly) : base()
        {
            this.ObjectToDisplay = objectToDisplay;
            this.IsReadOnly = isReadOnly;
        }
    }
}
