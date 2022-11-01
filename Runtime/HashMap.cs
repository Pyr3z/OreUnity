/*! @file       Runtime/HashMap.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-28
 *
 *  Hashing Function:
 *    Default for type TKey, or user-defined via custom IComparator<TKey>.
 *
 *  Load Factor Grow Threshold:
 *    0.72 by default, or user-defined via HashMapParams.
 *
 *  Collision Resolution Policy:
 *    Closed hashing w/ jump probing.
 *
 *  @remarks
 *    System.Collections.Generic.Dictionary is now a closed-hashed table, with
 *    indirected chaining for collision resolution. It used to be implemented as
 *    a red/black tree (bAcK iN mY dAy). This HashMap can occasionally beat
 *    Dictionary at sanitary speed tests, however where HashMap really wins is
 *    by supplying a far more flexible API for algorithmic optimization, which
 *    is where the biggest speed gains are actually won in practice.
 *
 *    Some examples of "flexible API for algorithmic optimization":
 *      - Map() doesn't overwrite and can be used like HashSet.Add().
 *      - TryMap() allows algorithms to either insert or update existing
 *        entries, for the cost of only 1 lookup.
 *
 *    That said, this HashMap still has a lot of areas that can be worked on to
 *    improve its performance.
 *
 *    System.Collections.Hashtable is implemented with hash jump probing too,
 *    and buckets with nullable boxed keys. When subtracting the cost of boxing
 *    allocations, Hashtable tends to beat Dictionary and Hashmap at most speed
 *    tests; however, it too has an inflexible API.
 *
 *    Note: Non-null keys with a hash code of 0 are still valid keys, and should
 *    not break this implementation—EXCEPT if type K is such that default(K) is
 *    also equal to 0! ... This sucks in many ways, but that's how it is by
 *    default for now. Technically you could work around this limitation with a
 *    custom IComparator<K> provided, and perhaps a custom type K itself to
 *    boot... but that's up to you.
**/

using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

using Array = System.Array;
using Type = System.Type;


namespace Ore
{

  [System.Serializable] // only *actually* serializable if subclassed!
  public partial class HashMap<K,V> : IDictionary<K,V>, IDictionary
  {
    public Type KeyType => typeof(K);

    public Type ValueType => typeof(V);

    public IComparator<K> KeyComparator
    {
      get => m_KeyComparator;
      set => m_KeyComparator = value ?? Comparator<K>.Default;
    }

    public IComparator<V> ValueComparator
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

    public V this[K key]
    {
      get
      {
        _ = Find(key, out V result);
        return result;
      }
      set  => _ = TryInsert(key, value, overwrite: true, out _ );
    }

    public bool IsFixedSize => m_Params.IsFixedSize;



    public HashMap()
      : this(null)
    {
    }

    public HashMap(HashMapParams parms)
      : this(null, parms)
    {
    }

    public HashMap([CanBeNull] IComparator<K> keyComparator, HashMapParams parms = default)
    {
      if (keyComparator is {})
      {
        m_KeyComparator = keyComparator;
      }

      if (parms.Check())
      {
        m_Params = parms;
      }

      m_Buckets = new Bucket[m_Params.InitialSize];
      m_LoadLimit = m_Params.CalcLoadLimit();
      #if UNITY_INCLUDE_TESTS
      LifetimeAllocs = 1;
      #endif
    }

    public HashMap([NotNull] IReadOnlyCollection<K> keys, [NotNull] IReadOnlyCollection<V> values)
    {
      OAssert.False(keys.Count < values.Count);

      int size = m_Params.StoreLoadLimit(keys.Count);
      m_Buckets = new Bucket[size];
      m_LoadLimit = m_Params.CalcLoadLimit(size);

      OAssert.True(m_LoadLimit >= keys.Count);

      #if UNITY_INCLUDE_TESTS
      LifetimeAllocs = 1;
      #endif

      //
      // TODO optimize the following:
      //

      var keyiter = keys.GetEnumerator();
      var valiter = values.GetEnumerator();

      while (keyiter.MoveNext())
      {
        Remap(keyiter.Current, valiter.MoveNext() ? valiter.Current : default);
      }

      keyiter.Dispose();
      valiter.Dispose();
    }


    public HashMap<K,V> WithValueComparator(IComparator<V> cmp)
    {
      m_ValueComparator = cmp;
      return this;
    }


    public static HashMap<K,V> FixedCapacity(int capacity)
    {
      return new HashMap<K,V>(HashMapParams.FixedCapacity(capacity));
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
    public bool ContainsKey([NotNull] K key)
    {
      return FindBucket(in key) >= 0;
    }

    [Pure]
    public bool ContainsValue([CanBeNull] in V value)
    {
      return FindValue(in value) >= 0;
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
    public bool Find([NotNull] in K key, out V value)
    {
      int i = FindBucket(in key);
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
    public void Add([NotNull] K key, V val)
    {
      _ = TryInsert(in key, in val, overwrite: false, out _ );
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
    public bool Map([NotNull] in K key, in V val)
    {
      return TryInsert(in key, in val, overwrite: false, out _ );
    }

    /// <summary>
    /// Like Map(), but allows the user to overwrite preexisting values.
    /// </summary>
    /// <returns>
    /// true   if the value is successfully mapped,
    /// false  if the value is identical to a preexisting mapping,
    ///        or there was an error.
    /// </returns>
    public bool Remap([NotNull] in K key, in V val)
    {
      return TryInsert(in key, in val, overwrite: true, out _ );
    }

    /// <summary>
    /// Used in case you care what happens to previously mapped values at certain keys.
    /// </summary>
    /// <param name="key">The key to map the new value to.</param>
    /// <param name="val">The value to be mapped.</param>
    /// <param name="preexisting"><paramref name="val"/> if returns true or null; the preexisting value otherwise.</param>
    /// <returns>
    /// true   if new value was mapped successfully,
    /// false  if new value was NOT mapped because there is a preexisting value,
    /// null   if null key (likely), or map state error (unlikely).
    /// </returns>
    public bool? TryMap([NotNull] in K key, in V val, out V preexisting)
    {
      preexisting = val;

      if (TryInsert(in key, in val, overwrite: false, out int i))
      {
        return true;
      }

      if (i >= 0 && !m_KeyComparator.IsNone(m_Buckets[i].Key))
      {
        preexisting = m_Buckets[i].Value;
        return false;
      }

      return null;
    }


    public bool Unmap([NotNull] in K key)
    {
      int i = FindBucket(in key);

      if (i >= 0)
      {
        m_Buckets[i].Smear();
        --m_Count;
        ++m_Version;
        return true;
      }

      return false;
    }

    public bool Pop([NotNull] in K key, out V oldVal)
    {
      int i = FindBucket(in key);

      if (i >= 0)
      {
        oldVal = m_Buckets[i].Value;
        m_Buckets[i].Smear();
        --m_Count;
        ++m_Version;
        return true;
      }

      oldVal = default;
      return false;
    }

    public void Remove([NotNull] in K key)
    {
      _ = Unmap(in key);
    }

    public bool Clear()
    {
      return ClearNoAlloc(); // tests verify that NoAlloc is consistently faster
    }


    public void ResetCapacity()
    {
      m_Params.ResetInitialSize();

      int loadLimit = m_Params.CalcLoadLimit();

      if (m_Count == 0)
      {
        m_Collisions = 0;
        m_LoadLimit = loadLimit;

        if (m_Buckets.Length == m_Params.InitialSize)
        {
          Array.Clear(m_Buckets, 0, m_Buckets.Length);
        }
        else
        {
          m_Buckets = new Bucket[m_Params.InitialSize];
          #if UNITY_INCLUDE_TESTS
          ++ LifetimeAllocs;
          #endif
        }
      }
      else if (m_Count > loadLimit)
      {
        Rehash(m_Params.CalcInternalSize(m_Count));
      }
      else
      {
        Rehash(m_Params.InitialSize);
      }
    }

    public void Rehash()
    {
      Rehash(m_Buckets.Length);
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


  #region IDictionary<K,V>

    public ICollection<K> Keys     => new KeyCollection(this);
    ICollection IDictionary.Keys   => new KeyCollection(this);
    public ICollection<V> Values   => new ValueCollection(this);
    ICollection IDictionary.Values => new ValueCollection(this);

    public bool IsReadOnly => false;

    object IDictionary.this[object key]
    {
      get => this[(K)key];
      set => this[(K)key] = (V)value;
    }


    public bool TryGetValue(K key, out V value)
    {
      return Find(key, out value);
    }

    public bool Remove(KeyValuePair<K,V> kvp)
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

    bool IDictionary<K, V>.Remove(K key)
    {
      return Unmap(key);
    }

    public void Add(KeyValuePair<K, V> kvp)
    {
      Add(kvp.Key, kvp.Value);
    }

    void ICollection<KeyValuePair<K, V>>.Clear()
    {
      _ = Clear();
    }

    public bool Contains(KeyValuePair<K, V> kvp)
    {
      return Find(kvp.Key, out var value) && (m_ValueComparator is null ||
                                              m_ValueComparator.Equals(kvp.Value, value));
    }

    void ICollection<KeyValuePair<K,V>>.CopyTo(KeyValuePair<K,V>[] array, int start)
    {
      // TODO
      throw new System.NotImplementedException();
    }

    IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K,V>>.GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new Enumerator(this);
    }

  #endregion IDictionary<K,V>

  #region IDictionary

    bool ICollection.IsSynchronized => false;
    object ICollection.SyncRoot => this;

    void ICollection.CopyTo(Array array, int start)
    {
      if (array is KeyValuePair<K,V>[] arr)
      {
        ((ICollection<KeyValuePair<K,V>>)this).CopyTo(arr, start);
      }
    }

    void IDictionary.Add(object key, object value)
    {
      if (key is K k && value is V v)
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
      return key is K k && ContainsKey(k);
    }

    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
      return new Enumerator(this);
    }

    void IDictionary.Remove(object key)
    {
      if (key is K k)
      {
        Remove(k);
      }
    }

  #endregion IDictionary

  } // end partial class HashMap

}