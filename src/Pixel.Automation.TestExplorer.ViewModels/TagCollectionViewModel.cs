using Caliburn.Micro;
using Dawn;
using Pixel.Automation.Core.TestData;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Pixel.Automation.TestExplorer.ViewModels
{
    /// <summary>
    /// View Model for a collection of <see cref="Tag"/>
    /// </summary>
    public class TagCollectionViewModel : PropertyChangedBase
    {
        /// <summary>
        /// Collection of Tags 
        /// </summary>
        public ObservableCollection<TagViewModel> Tags { get; } = new ObservableCollection<TagViewModel>();

        private TagViewModel selectedTag;
        /// <summary>
        /// Selected Tag on the view
        /// </summary>
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
        /// <summary>
        /// Indiactes if there are any validation errors
        /// </summary>
        public bool HasErrors
        {
            get => hasErrors;
            set
            {
                hasErrors = value;
                NotifyOfPropertyChange(() => HasErrors);
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        public TagCollectionViewModel()
        {

        }

        /// <summary>
        /// Add a new Tag to the collection and set it selected
        /// </summary>
        public void AddNew()
        {
            var newTag = new TagViewModel(new Tag()) { IsEditing = true };
            Add(newTag);
            SelectedTag = newTag;
        }

        /// <summary>
        /// Guard method to determine if we can edit the Tag
        /// </summary>
        public bool CanEdit
        {
            get => this.SelectedTag != null && !this.Tags.Any(t => t.IsEditing);
        }

        /// <summary>
        /// Put selected tag in edit mode
        /// </summary>
        public void EditSelected()
        {
            this.SelectedTag?.Edit();
        }

        /// <summary>
        /// Guard method to determing if tag can be deleted
        /// </summary>
        public bool CanDelete
        {
            get => this.SelectedTag != null;
        }

        /// <summary>
        /// Delete selected Tag
        /// </summary>
        public void DeleteSelected()
        {
            if (this.SelectedTag != null)
            {
                this.SelectedTag.Delete();           
                this.Tags.Remove(this.SelectedTag);
                this.SelectedTag = null;
            }
        }

        /// <summary>
        /// Add a new Tag
        /// </summary>
        /// <param name="item"></param>
        public void Add(TagViewModel item)
        {
            Guard.Argument(item).NotNull();
            this.Tags.Add(item);
        }

        /// <summary>
        /// Check tags for any validation errors
        /// </summary>
        /// <param name="validationErrors"></param>
        /// <returns></returns>
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
            HasErrors = validationErrors.Any();
            return !HasErrors;
        }

    }

}
