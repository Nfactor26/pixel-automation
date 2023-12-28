using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Pixel.Persistence.Core.Models;

/// <summary>
/// Pair of key and a collection
/// </summary>
/// <typeparam name="T"></typeparam>
public class KeyCollectionPair<T> where T : class
{
    /// <summary>
    /// Name of the group
    /// </summary>
    public string GroupName { get; set; }

    /// <summary>
    /// Collection belonging to the group
    /// </summary>
    public List<T> Collection { get; set; } = new();

    /// <summary>
    /// constructor
    /// </summary>
    public KeyCollectionPair()
    {

    }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="groupName"></param>
    public KeyCollectionPair(string groupName)
    {
        this.GroupName = groupName;
    }

    /// <inheritdoc/> 
    public override bool Equals(object obj)
    {
        if (obj is KeyCollectionPair<T> keyCollectionPair)
        {
            return keyCollectionPair.GroupName.Equals(this.GroupName) && keyCollectionPair.Collection.SequenceEqual(this.Collection);
        }
        return base.Equals(obj);
    }

    /// <inheritdoc/> 
    public override int GetHashCode()
    {
        return HashCode.Combine(GroupName.GetHashCode());
    }
}

/// <summary>
/// GroupedCollection<typeparamref name="T"/> is a collection of <see cref="KeyCollectionPair{T}"/>. Only <see cref="KeyCollectionPair{T}"/> with unique group name can be added to this collection.
/// </summary>
/// <typeparam name="T"></typeparam>
public class GroupedCollection<T> : IList<KeyCollectionPair<T>> where T : class
{
    private List<KeyCollectionPair<T>> collection = new();

    /// <inheritdoc/> 
    public KeyCollectionPair<T> this[int index]
    {
        get => collection[index];
        set => collection[index] = value;
    }

    /// <inheritdoc/> 
    public int Count => collection.Count;

    /// <inheritdoc/> 
    public bool IsReadOnly => false;

    /// <inheritdoc/> 
    public void Add(KeyCollectionPair<T> item)
    {
        if (!collection.Any(c => c.GroupName.Equals(item.GroupName)))
        {
            collection.Add(item);
            return;
        }
        throw new ArgumentException($"KeyCollectionPair already exists with group name : '{item.GroupName}'");
    }

    /// <inheritdoc/> 
    public void Clear()
    {
        collection.Clear();
    }

    /// <inheritdoc/> 
    public bool Contains(KeyCollectionPair<T> item)
    {
        return collection.Contains(item);
    }

    /// <inheritdoc/> 
    public void CopyTo(KeyCollectionPair<T>[] array, int arrayIndex)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/> 
    public IEnumerator<KeyCollectionPair<T>> GetEnumerator()
    {
        return collection.GetEnumerator();
    }

    /// <inheritdoc/> 
    public int IndexOf(KeyCollectionPair<T> item)
    {
        return collection.IndexOf(item);
    }

    /// <inheritdoc/> 
    public void Insert(int index, KeyCollectionPair<T> item)
    {
        collection.Insert(index, item);
    }

    /// <inheritdoc/> 
    public bool Remove(KeyCollectionPair<T> item)
    {
        return collection.Remove(item);
    }

    /// <inheritdoc/> 
    public void RemoveAt(int index)
    {
        collection.RemoveAt(index);
    }

    /// <inheritdoc/> 
    IEnumerator IEnumerable.GetEnumerator()
    {
        return (collection as IEnumerable).GetEnumerator();
    }
}