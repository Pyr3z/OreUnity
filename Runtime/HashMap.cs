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
 *      - Map(K,V) doesn't overwrite and can be used like HashSet.Add().
 *      - Map(K,V,out V) allows algorithms to either insert or update existing
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
using Type  = System.Type;


namespace Ore
{

  [System.Serializable] // only *actually* serializable if subclassed!
  public partial class HashMap<K,V> : IDictionary<K,V>, IDictionary
  {
    public Type KeyType   => typeof(K); // TODO these used to be hooked up for editor stuff
    public Type ValueType => typeof(V);

    /// <summary>
    ///   The comparator used to calculate hashes and determine equality (and
    ///   emptiness) of keys.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">
    ///   if you attempt to set this property while the map contains entries.
    /// </exception>
    public IComparator<K> KeyComparator
    {
      [NotNull]
      get => m_KeyComparator;
      [CanBeNull]
      set
      {
        if (m_Count > 0 && !ReferenceEquals(m_KeyComparator, value))
        {
          throw new System.InvalidOperationException(
            "Changing the key comparator of a non-empty HashMap will result in undefined behavior."
          );
        }

        m_KeyComparator = value ?? Comparator<K>.Default;
      }
    }

    /// <summary>
    ///   An optional comparator that, if provided, changes the behaviour of
    ///   overwriting insertions to <i>not</i> proceed if the mapped value
    ///   compares equal to the inserting value.
    /// </summary>
    [CanBeNull]
    public IComparator<V> ValueComparator
    {
      get => m_ValueComparator;
      set => m_ValueComparator = value; // null is allowed
    }

    /// <summary>
    ///   The number of entries in this map.
    /// </summary>
    public int Count => m_Count;

    /// <summary>
    ///   The usable capacity of the map.
    /// </summary>
    /// <remarks>
    ///   It is recommended to use <see cref="EnsureCapacity">EnsureCapacity()</see>
    ///   instead of the setter here, although it's not poor form if you don't.
    /// </remarks>
    public int Capacity
    {
      get => m_LoadLimit;
      set => _ = EnsureCapacity(value);
    }

    /// <summary>
    ///   The tweaky parameter struct that can be used to recreate the internal
    ///   structure of this map (not, however, its contents).
    /// </summary>
    public HashMapParams Parameters => m_Params;

    /// <summary>
    ///   An integer value that changes whenever the map changes.
    /// </summary>
    public int Version => m_Version;

    /// <remarks>
    ///   The setter (<c>this[K] = V</c>) here utilizes the equivalent of
    ///   <see cref="OverMap">OverMap()</see> to set the value at key.
    /// </remarks>
    public V this[[CanBeNull] K key]
    {
      get
      {
        _ = Find(key, out V result);
        return result;
      }
      set  => _ = TryInsert(key, value, overwrite: true, out _ );
    }

    /// <summary>
    ///   Fixed-size maps may be able to map new values (if there's space), but
    ///   the internal <see cref="Capacity"/> can never grow or change.
    /// </summary>
    public bool IsFixedSize => m_Params.IsFixedSize;



    public HashMap(HashMapParams parms = default)
    {
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

    public HashMap([NotNull] IReadOnlyCollection<K> keys, [CanBeNull] IReadOnlyCollection<V> values)
    {
      _ = MapAll(keys, values);
    }

    /// <summary>
    ///   "Copy" constructor.
    /// </summary>
    public HashMap([NotNull] IDictionary<K,V> other)
      : this((IReadOnlyCollection<K>)other.Keys,
             (IReadOnlyCollection<V>)other.Values)
    {
    }


    /// <summary>
    ///   Sets this map's optional <see cref="ValueComparator"/> property.
    /// </summary>
    /// <remarks>
    ///   Intended to be called following a constructor, e.g. <br/>
    ///   <c>new HashMap().WithValueComparator(cmp)</c>
    /// </remarks>
    public HashMap<K,V> WithValueComparator(IComparator<V> cmp)
    {
      m_ValueComparator = cmp;
      return this;
    }

    /// <returns>
    ///   a new HashMap with the given FIXED capacity.
    /// </returns>
    public static HashMap<K,V> FixedCapacity(int capacity)
    {
      return new HashMap<K,V>(HashMapParams.FixedCapacity(capacity));
    }


    /// <summary>
    ///   Fast search for the existence of the given key in this map.
    /// </summary>
    [Pure]
    public bool ContainsKey([CanBeNull] K key)
    {
      return FindBucket(in key) >= 0;
    }

    /// <summary>
    ///   LINEAR search for the existence of the given value in this map.
    /// </summary>
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
    public bool Find([CanBeNull] in K key, [CanBeNull] out V value)
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
    ///   For syntactic sugar and familiarity, however no different from Map(),
    ///   aside from the void return.
    /// </summary>
    public void Add([NotNull] K key, V val)
    {
      _ = TryInsert(in key, in val, overwrite: false, out _ );
    }

    /// <summary>
    ///   Registers a new key-value mapping in the HashMap iff there isn't
    ///   already a mapping at the given key.
    /// </summary>
    /// <returns>
    ///   <c>true</c>   if the value was successfully mapped to the given key, <br/>
    ///   <c>false</c>  if there was already a value mapped to this key.
    /// </returns>
    public bool Map([NotNull] in K key, in V val)
    {
      return TryInsert(in key, in val, overwrite: false, out _ );
    }

    /// <summary>
    ///   Overload of <see cref="Map(K,V)"/> for callers who need to know about
    ///   potentially preexisting values at the given key.
    /// </summary>
    /// <param name="preexisting">
    ///   References the input <paramref name="val"/> if null or true are
    ///   returned; otherwise, references the preexisting value in the map.
    /// </param>
    /// <returns>
    ///   <c>true</c>   if new value was mapped successfully, <br/>
    ///   <c>false</c>  if new value was NOT mapped because there is a preexisting value, <br/>
    ///   <c>null</c>   if key is null, or map state error (unlikely).
    /// </returns>
    public bool? Map([NotNull] in K key, in V val, out V preexisting)
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

    /// <summary>
    ///   Maps all the given keys,
    /// </summary>
    /// <returns>
    ///   The count of new entries resulting from the operation.
    /// </returns>
    public int MapAll([NotNull] IReadOnlyCollection<K> keys, IReadOnlyCollection<V> values = null)
    {
      if (values is null)
      {
        values = Array.Empty<V>();
      }

      OAssert.False(keys.Count < values.Count);

      if (!EnsureCapacity(m_Count + keys.Count))
      {
        return 0;
      }

      OAssert.True(m_LoadLimit >= keys.Count);

      //
      // TODO optimize the following:
      //

      int count = 0;

      var keyiter = keys.GetEnumerator();
      var valiter = values.GetEnumerator();

      while (keyiter.MoveNext())
      {
        if (TryInsert(keyiter.Current, valiter.MoveNext() ? valiter.Current : default, overwrite: false, out _ ))
          ++ count;
      }

      keyiter.Dispose();
      valiter.Dispose();

      return count;
    }


    /// <summary>
    ///   Like <see cref="Map(K,V)"/>, except that preexisting values will be
    ///   overwritten.
    /// </summary>
    /// <returns>
    ///   <c>true</c>   if the value is successfully mapped, <br/>
    ///   <c>false</c>  if the value is identical to a preexisting mapping.
    /// </returns>
    /// <remarks>
    ///   This should only return false if <see cref="ValueComparator"/> has
    ///   been set to a non-null comparator, AND if said comparator has
    ///   determined that the given value and a preexisting value are equal.
    /// </remarks>
    public bool OverMap([NotNull] in K key, in V val)
    {
      return TryInsert(in key, in val, overwrite: true, out _ );
    }


    /// <summary>
    ///   Overrides the value at the given key, if and only if it already exists.
    /// </summary>
    /// <returns>
    ///   True iff the value at the key was modified.
    /// </returns>
    public bool Remap([NotNull] in K key, in V val)
    {
      int i = FindBucket(in key);

      // if (i >= 0 && ( m_ValueComparator == null ||
      //                !m_ValueComparator.Equals(m_Buckets[i].Value, val) ))
      if (i >= 0)
      {
        m_Buckets[i].Value = val;
        ++ m_Version;
        return true;
      }

      return false;
    }


    /// <summary>
    ///   Removes an entry with the given key, if one exists.
    /// </summary>
    /// <returns>
    ///   <c>true</c> iff there was a matching entry and it was removed.
    /// </returns>
    public bool Unmap([NotNull] in K key)
    {
      int i = FindBucket(in key);

      if (i >= 0)
      {
        m_Buckets[i].Smear();
        -- m_Count;
        ++ m_Version;
        return true;
      }

      return false;
    }

    /// <summary>
    ///   Identical to <see cref="Unmap"/>, except returns the value from the
    ///   removed entry as an out parameter.
    /// </summary>
    public bool Pop([NotNull] in K key, out V oldVal)
    {
      int i = FindBucket(in key);

      if (i >= 0)
      {
        oldVal = m_Buckets[i].Value;
        m_Buckets[i].Smear();
        -- m_Count;
        ++ m_Version;
        return true;
      }

      oldVal = default;
      return false;
    }

    /// <summary>
    ///   void-returning equivalent of <see cref="Unmap"/>.
    /// </summary>
    public void Remove([NotNull] in K key)
    {
      _ = Unmap(in key);
    }

    /// <summary>
    ///   Unmaps all keys that evaluate <c>true</c> for the given predicate.
    /// </summary>
    /// <returns>
    ///   The number of entries unmapped by this operation.
    /// </returns>
    public int UnmapAllKeys([NotNull] System.Predicate<K> where)
    {
      if (m_Count == 0)
        return 0;

      int precount = m_Count;

      using (var enumerator = new Enumerator(this))
      {
        while (enumerator.MoveNext())
        {
          if (where(enumerator.CurrentKey))
          {
            enumerator.UnmapCurrent();
          }
        }
      }

      return precount - m_Count;
    }

    /// <summary>
    ///   Unmaps all entries whose value evaluates <c>true</c> for the given
    ///   predicate.
    /// </summary>
    /// <returns>
    ///   The number of entries unmapped by this operation.
    /// </returns>
    public int UnmapAllValues([NotNull] System.Predicate<V> where)
    {
      if (m_Count == 0)
        return 0;

      int precount = m_Count;

      using (var enumerator = new Enumerator(this))
      {
        while (enumerator.MoveNext())
        {
          if (where(enumerator.CurrentValue))
          {
            enumerator.UnmapCurrent();
          }
        }
      }

      return precount - m_Count;
    }

    /// <summary>
    ///   Unmaps all entries whose value is currently null.
    /// </summary>
    /// <returns>
    ///   The number of entries unmapped by this operation.
    /// </returns>
    public int UnmapNulls()
    {
      if (m_Count == 0)
        return 0;

      int precount = m_Count;

      using (var enumerator = new Enumerator(this))
      {
        while (enumerator.MoveNext())
        {
          if (enumerator.CurrentValue is null)
          {
            enumerator.UnmapCurrent();
          }
        }
      }

      return precount - m_Count;
    }

    /// <returns>
    ///   <c>true</c> if the map changed as a result of the method call.
    /// </returns>
    public bool Clear()
    {
      return ClearNoAlloc(); // tests verify that NoAlloc is consistently faster
    }


    /// <summary>
    ///   Copies all entries from the given map <b>which do not already exist</b>
    ///   (by key) to this map.
    /// </summary>
    /// <param name="other">
    ///   The map to perform a union with.
    ///   Will not be modified.
    /// </param>
    /// <param name="overwrite">
    ///   Optionally specify if the entries already in this map, whose key
    ///   collides with a key in other, should have their value overwritten by
    ///   the value in other.
    /// </param>
    /// <returns>
    ///   The number of new/changed entries in this map resulting from the
    ///   operation.
    /// </returns>
    public int Union([NotNull] HashMap<K,V> other, bool overwrite = false)
    {
      if (ReferenceEquals(other, this) || other.m_Count == 0)
        return 0;

      // TODO check KeyComparator equality, optimize from there

      int changes = m_Version;

      int o = other.m_Buckets.Length;
      while (o --> 0)
      {
        var otherBuck = other.m_Buckets[o];

        if (otherBuck.MightBeEmpty() && m_KeyComparator.IsNone(otherBuck.Key))
          continue;

        int hash31 = otherBuck.DirtyHash & int.MaxValue;

        int i = FindBucket(otherBuck.Key, otherBuck.DirtyHash & int.MaxValue);

        if (i < 0)
        {
          otherBuck.DirtyHash = hash31;
          var (c,j) = otherBuck.PlaceIn(m_Buckets,
                                         m_Params.CalcJump(hash31, m_Buckets.Length),
                                         m_KeyComparator);
          m_Collisions += c;

          if (j > m_LongestChain)
            m_LongestChain = j;

          ++ m_Count;
          ++ m_Version;
        }
        else if (overwrite && ( m_ValueComparator == null ||
                               !m_ValueComparator.Equals(otherBuck.Value, m_Buckets[i].Value) ))
        {
          m_Buckets[i].Value = otherBuck.Value;
          ++ m_Version;
        }
      }

      return m_Version - changes;
    }

    /// <summary>
    ///   Modifies this map to only contain <b>entries whose key exists in
    ///   both maps</b>.
    /// </summary>
    /// <param name="other">
    ///   The map to perform an intersection with.
    ///   Will not be modified.
    /// </param>
    /// <param name="overwrite">
    ///   Optionally specify if the entries already in this map, whose key
    ///   collides with a key in other, should have their value overwritten by
    ///   the value in other.
    /// </param>
    /// <returns>
    ///   The number of removed/changed entries in this map resulting from the
    ///   operation.
    /// </returns>
    public int Intersect([NotNull] HashMap<K,V> other, bool overwrite = false)
    {
      if (ReferenceEquals(other, this) || m_Count == 0)
        return 0;

      int changes = m_Version;

      if (other.m_Count == 0) // short-circuit
      {
        changes = m_Count;
        if (Clear())
          return changes;
        return 0;
      }

      // TODO:
      // if (!ReferenceEquals(m_KeyComparator, other.m_KeyComparator))
      //   return IntersectSlow(other.Keys, other.Values, overwrite);

      int remaining = m_Count;
      int i = m_Buckets.Length;
      while (i --> 0)
      {
        var bucket = m_Buckets[i];

        if (bucket.MightBeEmpty() && m_KeyComparator.IsNone(bucket.Key))
          continue;

        int o = other.FindBucket(bucket.Key);
        if (o < 0)
        {
          m_Buckets[i].Smear();
          -- m_Count;
          ++ m_Version;
        }
        else if (overwrite && ( m_ValueComparator == null ||
                               !m_ValueComparator.Equals(bucket.Value, other.m_Buckets[o].Value) ))
        {
          m_Buckets[i].Value = other.m_Buckets[o].Value;
          ++ m_Version;
        }

        if (--remaining == 0)
          break;
      }

      return m_Version - changes;
    }

    /// <summary>
    ///   Unmaps all keys contained in other from this map.
    /// </summary>
    /// <returns>
    ///   The number of entries removed by the operation.
    /// </returns>
    public int Except([NotNull] HashMap<K,V> other)
    {
      int unmapped = 0;

      if (ReferenceEquals(other, this))
      {
        unmapped = m_Count;
        if (Clear())
          return unmapped;
        return 0;
      }

      if (other.m_Count == 0)
        return 0;

      foreach (var key in other.Keys)
      {
        if (Unmap(key))
          ++ unmapped;
      }

      return unmapped;
    }

    /// <summary>
    ///   Modifies this map to contains only entries that are present in either
    ///   itself or other, but not both.
    /// </summary>
    /// <returns>
    ///   The number of entries removed/changed as a result of this operation.
    /// </returns>
    /// <remarks>
    ///   It's like the XOR of set theory.
    /// </remarks>
    public int SymmetricExcept([NotNull] HashMap<K,V> other, bool overwrite = false)
    {
      int changes = m_Version;

      if (ReferenceEquals(other, this))
      {
        changes = m_Count;
        if (Clear())
          return changes;
        return 0;
      }

      if (other.m_Count == 0)
        return 0;

      using (var enumerator = new Enumerator(other))
      {
        while (enumerator.MoveNext())
        {
          var buck = enumerator.m_Bucket;

          int hash31 = buck.DirtyHash & int.MaxValue;

          int i = FindBucket(buck.Key, hash31);

          if (i >= 0) // exists in both = yeet
          {
            m_Buckets[i].Smear();
            -- m_Count;
            ++ m_Version;
            continue;
          }

          int chain = ~ i;

          i = ~ m_CachedLookup;


          if (chain == 0) // best case = no collisions
          {
            m_Buckets[i].Key       = buck.Key;
            m_Buckets[i].Value     = buck.Value;
            m_Buckets[i].DirtyHash = buck.DirtyHash & int.MaxValue;
            ++ m_Count;
            ++ m_Version;
            continue;
          }

          // sad case = some collisions
          buck.DirtyHash = hash31;
          var (c,j) = buck.PlaceIn(m_Buckets,
                                         m_Params.CalcJump(hash31, m_Buckets.Length),
                                         m_KeyComparator);
          m_Collisions += c;

          if (j > m_LongestChain)
            m_LongestChain = j;

          ++ m_Count;
          ++ m_Version;
        }
      }

      return m_Version - changes;
    }


    /// <inheritdoc cref="ResetCapacity"/>
    public void TrimExcess()
    {
      ResetCapacity();
    }

    /// <summary>
    ///   Resets the capacity of the hashmap to the one stored in its original
    ///   <see cref="HashMapParams"/> struct, growing as necessary to fit the
    ///   map's current contents.
    /// </summary>
    /// <remarks>
    ///   A <see cref="Rehash"/> operation will occur if the map is non-empty.
    /// </remarks>
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

    /// <summary>
    ///   Public method exposed to allow you to "rehash" the hashmap, which might
    ///   improve hash table structural performance—particularly if there have
    ///   been a lot of collisions / removals since the last time the map was empty.
    /// </summary>
    public void Rehash()
    {
      Rehash(m_Buckets.Length);
    }

    /// <summary>
    ///   Tries to ensure the HashMap can hold at least loadLimit items.
    /// </summary>
    /// <param name="loadLimit">
    ///   The minimum quantity of items to ensure capacitance for.
    /// </param>
    /// <returns>
    ///   <c>true</c>   if the HashMap can now hold at least loadLimit items. <br/>
    ///   <c>false</c>  if the HashMap failed to reallocate enough space to hold loadLimit items.
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


    /// <returns>
    ///   an <see cref="Enumerator"/> for this map.
    /// </returns>
    /// <remarks>
    ///   This enumerator is special because, if you iterate over it using MoveNext(),
    ///   it exposes additional properties and methods that widen your horizons.
    /// </remarks>
    /// <seealso cref="Enumerator.UnmapCurrent"/>
    /// <seealso cref="Enumerator.RemapCurrent"/>
    /// <seealso cref="Enumerator.CurrentKey"/>
    /// <seealso cref="Enumerator.CurrentValue"/>
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

      if (!(m_ValueComparator is null) && !m_ValueComparator.Equals(kvp.Value, m_Buckets[i].Value))
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