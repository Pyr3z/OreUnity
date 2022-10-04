/*! @file       Runtime/HashMap.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-28
 *
 *  Hashing Function:
 *    Default for type TKey, or user-defined via custom IComparator<TKey>.
 *
 *  Load Factor Grow Threshold:
 *    0.72 by default, or user-defined via HashMapParams
 *
 *  Collision Resolution Policy:
 *    Linear probing (as it is the most flexible for all use cases).
**/

using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

using Array = System.Array;
using Type = System.Type;


namespace Ore
{

  [System.Serializable] // only *actually* serializable if subclassed!
  public partial class HashMap<TKey,TValue> : IDictionary<TKey,TValue>, IDictionary
  {
    public Type KeyType => typeof(TKey);
    public Type ValueType => typeof(TValue);
    public IComparator<TKey> KeyComparator
    {
      get => m_KeyComparator;
      set => m_KeyComparator = value ?? Comparator<TKey>.Default;
    }
    public IComparator<TValue> ValueComparator
    {
      get => m_ValueComparator;
      set => m_ValueComparator = value; // null is allowed
    }
    public int Count => m_Count;
    public int Capacity
    {
      get => m_LoadLimit;
      set => _ = EnsureCapacity(value);
    }
    public HashMapParams Parameters => m_Params;
    public int Version => m_Version;
    public TValue this[TKey key]
    {
      get
      {
        _ = Find(key, out TValue result);
        return result;
      }
      set  => _ = TryInsert(key, value, overwrite: true, out _ );
    }
    public bool IsFixedSize => m_Params.IsFixedSize;



    public HashMap()
    {
      MakeBuckets();
    }

    public HashMap(HashMapParams parms)
    {
      if (parms.Check())
      {
        m_Params = parms;
      }
      else
      {
        Orator.Warn("Bad HashMapParams passed into ctor.");
      }

      MakeBuckets();
    }



    /// <summary>
    /// Fast search for the existence of the given key in this map.
    /// </summary>
    /// <param name="key">A valid key to search for.</param>
    /// <returns>
    /// true   if the HashMap contains the key.
    /// false  if it doesn't.
    /// </returns>
    [Pure]
    public bool HasKey(TKey key)
    {
      return FindBucket(key) >= 0;
    }

    /// <summary>
    /// Finds the value mapped to the given key, if it exists in the HashMap.
    /// </summary>
    /// <param name="key">A valid key to search for.</param>
    /// <param name="value">The return parameter containing the found value (if true is returned).</param>
    /// <returns>
    /// true   if a value was found mapped to the key.
    /// false  if no value was found.
    /// </returns>
    [Pure]
    public bool Find(TKey key, out TValue value)
    {
      int i = FindBucket(key);
      if (i > -1)
      {
        value = m_Buckets[i].Value;
        return true;
      }

      value = default;
      return false;
    }

    /// <summary>
    /// For syntactic sugar and familiarity, however no different from Map(),
    /// aside from the void return.
    /// </summary>
    public void Add(TKey key, TValue val)
    {
      _ = TryInsert(key, val, overwrite: false, out _ );
    }

    /// <summary>
    /// Registers a new key-value mapping in the HashMap iff there isn't already
    /// a mapping at the given key.
    /// </summary>
    /// <returns>
    /// true   if the value was successfully mapped to the given key,
    /// false  if there was already a value mapped to this key,
    ///        or there was an error.
    /// </returns>
    public bool Map(TKey key, TValue val)
    {
      return TryInsert(key, val, overwrite: false, out _ );
    }

    /// <summary>
    /// Like Map(), but allows the user to overwrite preexisting values.
    /// </summary>
    /// <returns>
    /// true   if the value is successfully mapped,
    /// false  if the value is identical to a preexisting mapping,
    ///        or there was an error.
    /// </returns>
    public bool Remap(TKey key, TValue val)
    {
      return TryInsert(key, val, overwrite: true, out _ );
    }

    /// <summary>
    /// Used in case you care what happens to previously mapped values at certain keys.
    /// </summary>
    /// <param name="key">The key to map the new value to.</param>
    /// <param name="val">The value to be mapped.</param>
    /// <param name="preexisting">Conditional output value, which is only valid if false is returned.</param>
    /// <returns>
    /// true   if new value was mapped successfully,
    /// false  if new value was NOT mapped because there is a preexisting value,
    /// null   if null key (likely), or map state error (unlikely).
    /// </returns>
    public bool? TryMap(TKey key, TValue val, out TValue preexisting)
    {
      preexisting = default;

      if (TryInsert(key, val, overwrite: false, out int i))
      {
        return true;
      }

      if (i >= 0 && !m_Buckets[i].IsFree(m_KeyComparator))
      {
        preexisting = m_Buckets[i].Value;
        return false;
      }

      return null;
    }


    public bool Unmap(TKey key)
    {
      int i = FindBucket(key);

      if (i >= 0)
      {
        m_Buckets[i].Smear();
        --m_Count;
        ++m_Version;
        return true;
      }

      return false;
    }

    public void Remove(TKey key)
    {
      _ = Unmap(key);
    }

    public bool Clear()
    {
      bool alreadyClear = m_Count == 0;

      m_Collisions = m_Count = 0;
      // always reallocate, in case we have dirty buckets
      m_Buckets = new Bucket[m_Buckets.Length];

      #if UNITY_INCLUDE_TESTS
      ++LifetimeAllocs;
      #endif

      if (!alreadyClear)
      {
        ++m_Version;
        return true;
      }

      return false;
    }

    public bool ClearNoAlloc()
    {
      bool alreadyClear = m_Count == 0;

      m_Collisions = m_Count = 0;
      System.Array.Clear(m_Buckets, 0, m_Buckets.Length);

      if (!alreadyClear)
      {
        ++m_Version;
        return true;
      }

      return false;
    }


    /// <summary>
    /// Tries to ensure the HashMap can hold at least loadLimit items.
    /// </summary>
    /// <param name="loadLimit">The minimum quantity of items to ensure capacitance for.</param>
    /// <returns>
    /// true   if the HashMap can now hold at least loadLimit items.
    /// false  if the HashMap failed to reallocate enough space to hold loadLimit items.
    /// </returns>
    public bool EnsureCapacity(int loadLimit)
    {
      OAssert.False(loadLimit < 0, "provided a negative loadLimit");

      if (!m_Params.IsFixedSize && loadLimit > m_LoadLimit)
      {
        Rehash(m_Params.StoreLoadLimit(loadLimit));
      }

      return m_LoadLimit >= loadLimit;
    }


    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }


  #region IDictionary<TKey,TValue>

    public ICollection<TKey> Keys     { get; } // TODO
    ICollection IDictionary.Keys      { get; } // TODO

    public ICollection<TValue> Values { get; } // TODO
    ICollection IDictionary.Values    { get; } // TODO

    public bool IsReadOnly => false;
    object IDictionary.this[object key]
    {
      get => this[(TKey)key];
      set => this[(TKey)key] = (TValue)value;
    }


    public bool TryGetValue(TKey key, out TValue value)
    {
      return Find(key, out value);
    }

    public bool ContainsKey(TKey key)
    {
      return HasKey(key);
    }

    public bool Remove(KeyValuePair<TKey, TValue> kvp)
    {
      int i = FindBucket(kvp.Key);
      if (i < 0)
        return false;

      if (m_ValueComparator is {} && !m_ValueComparator.Equals(kvp.Value, m_Buckets[i].Value))
        return false;

      m_Buckets[i].Smear();
      --m_Count;
      ++m_Version;
      return true;
    }

    bool IDictionary<TKey, TValue>.Remove(TKey key)
    {
      return Unmap(key);
    }

    public void Add(KeyValuePair<TKey, TValue> kvp)
    {
      Add(kvp.Key, kvp.Value);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Clear()
    {
      _ = Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> kvp)
    {
      return Find(kvp.Key, out var value) && (m_ValueComparator is null ||
                                              m_ValueComparator.Equals(kvp.Value, value));
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int start)
    {
      // TODO
      throw new System.NotImplementedException();
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey,TValue>>.GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new Enumerator(this);
    }

  #endregion IDictionary<TKey,TValue>

  #region IDictionary

    bool ICollection.IsSynchronized => false;
    object ICollection.SyncRoot => this;

    void ICollection.CopyTo(Array array, int start)
    {
      if (array is KeyValuePair<TKey,TValue>[] arr)
      {
        CopyTo(arr, start);
      }
    }

    void IDictionary.Add(object key, object value)
    {
      if (key is TKey k && value is TValue v)
      {
        Add(k, v);
      }
    }

    void IDictionary.Clear()
    {
      _ = Clear();
    }

    bool IDictionary.Contains(object key)
    {
      return key is TKey k && HasKey(k);
    }

    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
      return new Enumerator(this);
    }

    void IDictionary.Remove(object key)
    {
      if (key is TKey k)
      {
        Remove(k);
      }
    }

  #endregion IDictionary

  } // end partial class HashMap

}