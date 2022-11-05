using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Pixel.Persistence.Core.Models;

/// <summary>
/// A tag can be associated with a test fixture or project and can be used to filter them
/// </summary>
[DataContract]
[Serializable]
public class Tag
{
    /// <summary>
    /// Key identifier of the tag
    /// </summary>
    [DataMember(IsRequired = true, Order = 10)]
    public string Key { get; set; }

    /// <summary>
    /// value of the tag
    /// </summary>
    [DataMember(IsRequired = true, Order = 20)]
    public string Value { get; set; }
}

/// <summary>
/// Collection of <see cref="Tag"/>
/// </summary>
[DataContract]
[Serializable]
public class TagCollection : ICollection<Tag>
{
    /// <summary>
    /// List of Tag
    /// </summary>
    [DataMember(IsRequired = true, Order = 10)]
    public List<Tag> Tags { get; set; } = new List<Tag>(4);

    /// <summary>
    /// Number of tags in collection
    /// </summary>
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

    /// <summary>
    /// constructor
    /// </summary>
    public TagCollection()
    {

    }

    /// <summary>
    /// constructor
    /// </summary>
    public TagCollection(IEnumerable<Tag> tags)
    {      
        this.Tags.AddRange(tags);
    }

    /// <summary>
    /// Add a new Tag from key and value to TagCollection
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
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

    /// <summary>
    /// Delete a tag given it's key
    /// </summary>
    /// <param name="key"></param>
    public void Delete(string key)
    {
        var tag = this.Tags.FirstOrDefault(t => t.Key.Equals(key));
        if (tag != null)
        {
            this.Tags.Remove(tag);
        }
    }

    /// <summary>
    /// Indicates if the collection contains a tag with a given key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Contains(string key)
    {
        return this.Tags.Any(t => t.Key.Equals(key));
    }

    /// <summary>
    /// clear all the tags in the collection
    /// </summary>
    public void Clear()
    {
        this.Tags.Clear();
    }

    ///</inheritdoc>
    public IEnumerator<Tag> GetEnumerator()
    {
        return this.Tags.GetEnumerator();
    }

    ///</inheritdoc>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.Tags.GetEnumerator();
    }

    /// <summary>
    /// Add a new Tag to the collection
    /// </summary>
    /// <param name="item"></param>
    public void Add(Tag item)
    {        
        this.Tags.Add(item);
    }

    /// <summary>
    /// Indicates if the collection contains the specified Tag
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Contains(Tag item)
    {
        return this.Tags.Contains(item);
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void CopyTo(Tag[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Remove specified tag from the collection
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Remove(Tag item)
    {
        if (Contains(item))
        {
            return this.Tags.Remove(item);
        }
        return false;
    }
}
