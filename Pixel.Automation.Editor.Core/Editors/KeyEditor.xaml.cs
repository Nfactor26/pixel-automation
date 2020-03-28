using Pixel.Automation.Core;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Pixel.Automation.Editor.Core.Editors
{
    /// <summary>
    /// Interaction logic for KeyEditor.xaml
    /// </summary>
    public partial class KeyEditor : UserControl , ITypeEditor , INotifyPropertyChanged
    {

        public static readonly DependencyProperty ActorComponentProperty = DependencyProperty.Register("ActorComponent", typeof(ActorComponent),
            typeof(KeyEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
    
        public ActorComponent ActorComponent
        {
            get { return (ActorComponent)GetValue(ActorComponentProperty); }
            set { SetValue(ActorComponentProperty, value); }
        }
     
        public bool IsModifierKeysRequired { get; private set; } = true;
     
        public bool AppendToExistingKey { get; private set; } = false;

        public KeyEditor()
        {
            InitializeComponent();
        }

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            this.ActorComponent = propertyItem.Instance as ActorComponent;

            switch(this.ActorComponent.Tag)
            {
                case "HotKey":
                    IsModifierKeysRequired = true;
                    AppendToExistingKey = false;
                    break;
                case "KeyPress":
                    IsModifierKeysRequired = false;
                    AppendToExistingKey = true;
                    break;
            }
            OnPropertyChanged("IsModifierKeysRequired");
            OnPropertyChanged("AppendToExistingKey");
            return this;           
        }

        #region INotifyPropertyChanged

        [field: NonSerializedAttribute()]
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion INotifyPropertyChanged

    }
}
