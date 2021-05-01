using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core.TestData;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Pixel.Automation.TestExplorer.ViewModels
{
    public class TagCollectionViewModel : PropertyChangedBase
    {
        public ObservableCollection<TagViewModel> Tags { get; private set; } = new ObservableCollection<TagViewModel>();

        private TagViewModel selectedTag;
        public TagViewModel SelectedTag
        {
            get => selectedTag;
            set
            {
                selectedTag = value;
                NotifyOfPropertyChange(() => SelectedTag);
                NotifyOfPropertyChange(() => CanEdit);
                NotifyOfPropertyChange(() => CanDelete);
            }
        }

        private bool hasErrors;
        public bool HasErrors
        {
            get => hasErrors;
            set
            {
                hasErrors = value;
                NotifyOfPropertyChange(() => HasErrors);
            }
        }

        public TagCollectionViewModel()
        {

        }

        public void AddNew()
        {
            var newTag = new TagViewModel(new Tag()) { IsEditing = true };
            Add(newTag);
            SelectedTag = newTag;
        }

        public bool CanEdit
        {
            get => this.SelectedTag != null && !this.Tags.Any(t => t.IsEditing);
        }

        public void EditSelected()
        {
            this.SelectedTag?.Edit();
        }

        public bool CanDelete
        {
            get => this.SelectedTag != null;
        }

        public void DeleteSelected()
        {
            if (this.SelectedTag != null)
            {
                this.SelectedTag.Delete();           
                this.Tags.Remove(this.SelectedTag);
                this.SelectedTag = null;
            }
        }

        public void Add(TagViewModel item)
        {
            Guard.Argument(item).NotNull();
            this.Tags.Add(item);
        }

        public bool Validate(out List<string> validationErrors)
        {
            validationErrors = new List<string>();
            if (this.Tags.Any(t => !t.IsValid()))
            {
                validationErrors.Add("All tags must have a key and value.");
            }
            if (this.Tags.Any(t => t.IsEditing))
            {
                validationErrors.Add("All tags open for edit must be saved.");
            }
            if (this.Tags.Select(t => t.Key).Distinct().Count() < this.Tags.Count)
            {
                validationErrors.Add("Tag keys must be unique");
            }
            HasErrors = !validationErrors.Any();
            return HasErrors;
        }

    }

}
