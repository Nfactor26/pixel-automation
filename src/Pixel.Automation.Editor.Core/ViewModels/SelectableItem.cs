using Dawn;
using Pixel.Automation.Core;

namespace Pixel.Automation.Editor.Core.ViewModels
{
    /// <summary>
    /// Wrapper for models that can be marked as selected
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SelectableItem<T> : NotifyPropertyChanged where T:class
    {
        /// <summary>
        /// Item that needs to be marked
        /// </summary>
        public T Item { get; private set; }

        private bool isSelected;
        /// <summary>
        /// Indicates if Item is selected
        /// </summary>
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="item">Item to be marked</param>
        /// <param name="isSelected">Indicate if item is initially selected</param>
        public SelectableItem(T item, bool isSelected)
        {
            this.Item = Guard.Argument(item).NotNull();
            this.IsSelected = isSelected;
        }

    }
}
