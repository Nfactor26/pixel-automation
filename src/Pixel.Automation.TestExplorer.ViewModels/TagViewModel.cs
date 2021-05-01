using Caliburn.Micro;
using Pixel.Automation.Core.TestData;
using Pixel.Automation.Editor.Core;
using System;
using System.Windows.Input;

namespace Pixel.Automation.TestExplorer.ViewModels
{
    public class TagViewModel : PropertyChangedBase
    {
        private readonly Tag tag;

        public string Key { get; set; }

        public string Value { get; set; }

        private bool isDeleted;
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
        public ICommand SaveCommand
        {
            get { return saveCommand ?? (saveCommand = new RelayCommand<bool>(p => Save(), p => true)); }
        }


        public TagViewModel(Tag tag)
        {
            this.tag = tag;
            this.Key = tag.Key;
            this.Value = tag.Value;
        }

        public void Edit()
        {
            IsEditing = true;
        }

        public void Delete()
        {
            IsDeleted = true;
            IsEditing = false;
            NotifyOfPropertyChange(() => IsDeleted);
        }

        public void Save()
        {
            if (!string.IsNullOrEmpty(this.Key) && !string.IsNullOrEmpty(this.Value))
            {
                this.tag.Key = this.Key;
                this.tag.Value = this.Value;
                IsEditing = false;
            }
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(this.Key) && !string.IsNullOrEmpty(this.Value);
        }

        public override string ToString()
        {
            return $"{Key ?? String.Empty} : {Value ?? String.Empty}";
        }
    }
}
