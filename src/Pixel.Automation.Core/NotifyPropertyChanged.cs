using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core
{
    /// <summary>
    /// Implementation of <see cref="INotifyPropertyChanged"/>. This acts as a base class for lot of 
    /// data models which are made available on property grid for configuration.
    /// </summary>
    [DataContract]
    [Serializable]
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {        
        public void Refresh()
        {
            OnPropertyChanged(string.Empty);
        }

        [field: NonSerializedAttribute()]
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected virtual void OnPropertyChanged([CallerMemberName]string name = "")
        {
            this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
        }

        [OnDeserialized]
        public void Initialize(StreamingContext context)
        {
            this.PropertyChanged = delegate { };
        }

    }
}
