using Dawn;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Automation.Core.TestData
{
    [DataContract]
    [Serializable]
    public class Tag
    {
        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public string Value { get; set; }
    }

    [DataContract]
    [Serializable]
    public class TagCollection : ICollection<Tag>
    {
        [DataMember]
        public List<Tag> Tags { get; set; } = new List<Tag>();

        public int Count => this.Tags.Count;

        public bool IsReadOnly => false;

        public string this[string key]
        {
            get
            {
                var tag = this.Tags.FirstOrDefault(t => t.Key.Equals(key));
                return tag?.Value ?? string.Empty;
            }
            set
            {
                Add(key, value);
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

        public void Add(string key, string value)
        {
            var tag = this.Tags.FirstOrDefault(t => t.Key.Equals(key));
            if (tag != null)
            {
                tag.Value = value;
                return;
            }
            this.Tags.Add(new Tag() { Key = key, Value = value });
        }

        public void Delete(string key)
        {
            var tag = this.Tags.FirstOrDefault(t => t.Key.Equals(key));
            if (tag != null)
            {
                this.Tags.Remove(tag);
            }
        }

        public bool Contains(string key)
        {
            return this.Tags.Any(t => t.Key.Equals(key));
        }

        public void Clear()
        {
            this.Tags.Clear();
        }

        public IEnumerator<Tag> GetEnumerator()
        {
            return this.Tags.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Tags.GetEnumerator();
        }

        public void Add(Tag item)
        {
            Guard.Argument(item).NotNull();
            this.Tags.Add(item);
        }

        public bool Contains(Tag item)
        {
            return this.Tags.Contains(item);
        }

        public void CopyTo(Tag[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(Tag item)
        {
            if(Contains(item))
            {
                return this.Tags.Remove(item);                
            }
            return false;
        }
    }
}
