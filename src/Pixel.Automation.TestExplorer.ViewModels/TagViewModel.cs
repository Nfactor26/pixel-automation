using Caliburn.Micro;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core;
using System;
using System.Windows.Input;

namespace Pixel.Automation.TestExplorer.ViewModels
{
    /// <summary>
    /// View Model for <see cref="Tag"/>
    /// </summary>
    public class TagViewModel : PropertyChangedBase
    {
        /// <summary>
        /// Underlying Model for view
        /// </summary>
        private readonly Tag tag;

        /// <summary>
        /// Key identifying the Tag
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Value assigned to the Tag
        /// </summary>
        public string Value { get; set; }

        private bool isDeleted;
        /// <summary>
        /// Indicates if the Tag has been marked to be deleted
        /// </summary>
        public bool IsDeleted
        {
            get => isDeleted;
            set
            {
                isDeleted = value;
                NotifyOfPropertyChange(() => IsDeleted);
            }
        }

        private bool isEditing;
        /// <summary>
        /// Indicates if the Tag is currently being edited
        /// </summary>
        public bool IsEditing
        {
            get => isEditing;
            set
            {
                isEditing = value;
                NotifyOfPropertyChange(() => IsEditing);
            }
        }

        private ICommand saveCommand;
        /// <summary>
        /// Save Command
        /// </summary>
        public ICommand SaveCommand
        {
            get { return saveCommand ?? (saveCommand = new RelayCommand<bool>(p => Save(), p => true)); }
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="tag"></param>
        public TagViewModel(Tag tag)
        {
            this.tag = tag;
            this.Key = tag.Key;
            this.Value = tag.Value;
        }

        /// <summary>
        /// Set IsEditing to true
        /// </summary>
        public void Edit()
        {
            IsEditing = true;
        }

        /// <summary>
        /// Mark the Tag to be deleted and stop editing it
        /// </summary>
        public void Delete()
        {
            IsDeleted = true;
            IsEditing = false;
            NotifyOfPropertyChange(() => IsDeleted);
        }

        /// <summary>
        /// Apply the changes in key and value of tag and stop editing
        /// </summary>
        public void Save()
        {
            if (IsValid())
            {
                this.tag.Key = this.Key;
                this.tag.Value = this.Value;
                IsEditing = false;
            }
        }

        /// <summary>
        /// Validate the Key and Value associated with a Tag
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(this.Key) && !string.IsNullOrEmpty(this.Value);
        }

        ///<inheritdoc/>
        public override string ToString()
        {
            return $"{Key ?? String.Empty} : {Value ?? String.Empty}";
        }
    }
}
