using Dawn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.TestData
{
    [DataContract]
    [Serializable]
    public class Tag
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }

    [DataContract]
    [Serializable]
    public class TagCollection
    {
        public List<Tag> Tags { get; } = new List<Tag>();

        public string this[string key]
        {
            get
            {
                var tag = this.Tags.FirstOrDefault(t => t.Key.Equals(key));
                return tag?.Value ?? string.Empty;
            }
            set
            {
                AddTag(key, value);
            }
        }

        public TagCollection()
        {

        }

        public TagCollection(IEnumerable<Tag> tags)
        {
            Guard.Argument(tags).NotNull();
            this.Tags.AddRange(tags);
        }

        public void AddTag(string key, string value)
        {
            var tag = this.Tags.FirstOrDefault(t => t.Key.Equals(key));
            if (tag != null)
            {
                tag.Value = value;
                return;
            }
            this.Tags.Add(new Tag() { Key = key, Value = value });
        }

        public void DeleteTag(string key)
        {
            var tag = this.Tags.FirstOrDefault(t => t.Key.Equals(key));
            if (tag != null)
            {
                this.Tags.Remove(tag);
            }
        }

        public bool HasTag(string key)
        {
            return this.Tags.Any(t => t.Key.Equals(key));
        }
    }
}
