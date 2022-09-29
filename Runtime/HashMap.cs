/*! @file       Runtime/HashMap.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-28
**/

using System.Collections.Generic;
using System.Collections;

using JetBrains.Annotations;

using UnityEngine;

using Type = System.Type;


namespace Ore
{

  [System.Serializable] // only *actually* serializable if subclassed!
  public partial class HashMap<TKey,TValue>
  {

    #region Properties

    [PublicAPI]
    public Type KeyType   => typeof(TKey);
    [PublicAPI]
    public Type ValueType => typeof(TValue);

    [PublicAPI]
    public int Count => m_Count;

    [PublicAPI]
    public int Capacity
    {
      get => m_LoadLimit;
      set => _ = EnsureCapacity(value);
    }

    public HashMapParams Parameters => m_Params;
    public int Version => m_Version;

    #endregion Properties


    #region Fields

    [SerializeField] // the only serializable field in this class
    protected HashMapParams m_Params = HashMapParams.Default;

    protected int m_Count, m_Collisions, m_LoadLimit;
    protected int m_Version;

    protected Bucket[] m_Buckets;

    protected IHashKeyComparator<TKey>  m_KeyComparator   = HashKeyComparator<TKey>.Default;
    protected IEqualityComparer<TValue> m_ValueComparator = EqualityComparer<TValue>.Default;

    #endregion


    #region Constructors

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

    #endregion Constructors


    #region Public Methods

    public bool EnsureCapacity(int userCapacity)
    {
      OAssert.False(userCapacity < 0, "provided negative userCapacity");

      if (!m_Params.IsFixedSize && userCapacity > m_LoadLimit)
      {
        Rehash(m_Params.SetUserCapacity(userCapacity));
      }

      return m_LoadLimit >= userCapacity;
    }

    #endregion Public Methods


    #region Internal Methods

    private bool TryInsert(TKey key, TValue val, bool overwrite, out int i)
    {
      if (m_KeyComparator.IsNullKey(key))
      {
        i = -1;
        return false;
      }

      if (m_Buckets is null)
      {
        MakeBuckets();
      }
      else if (m_Count >= m_LoadLimit)
      {
        if (GrowCapacity() <= m_Count)
        {
          i = -1;
          return false;
        }
      }
      else if (m_Collisions > m_LoadLimit && m_Count > m_Params.RehashThreshold)
      {
        Rehash();
      }

      CalcHashJump(key, out int hash31, out int jump);

      i = hash31 % m_Buckets.Length;

      int fallback = -1;
      int jumps = 0;

      do
      {
        var bucket = m_Buckets[i];

        if (IsEmptyBucket(bucket))
        {
          if (fallback != -1)
            i = fallback;

          bucket.DirtyHash = hash31;
          bucket.Key       = key;
          bucket.Value     = val;

          m_Buckets[i] = bucket;

          ++m_Count;
          ++m_Version;
          return true;
        }


        if (fallback == -1 && IsSmearedBucket(bucket))
        {
          fallback = i;
        }
        else if (IsFreeBucket(bucket))
        {
          // end of smear chain

          if (fallback != -1)
            i = fallback;

          bucket.Hash  = hash31; // preserves dirty bit
          bucket.Key   = key;
          bucket.Value = val;

          m_Buckets[i] = bucket;

          ++m_Count;
          ++m_Version;

          if (jumps > m_Params.RehashThreshold)
          {
            Rehash();
          }

          return true;
        }
        else if (bucket.Hash == hash31 && m_KeyComparator.Equals(key, bucket.Key))
        {
          // equivalent bucket found
          if (!overwrite || m_ValueComparator.Equals(val, bucket.Value))
            return false;

          m_Buckets[i].Value = val;
          ++m_Version;

          if (jumps > m_Params.RehashThreshold)
          {
            Rehash();
          }

          return true;
        }
        else // Mark collision
        {
          if (fallback == -1 && bucket.DirtyHash >= 0)
          {
            m_Buckets[i].DirtyHash |= int.MinValue;
            ++m_Collisions;
          }

          i = (i + jump) % m_Buckets.Length;
        }
      }
      while (++jumps < m_Buckets.Length);

      // MEGA bad if we reach here

      Orator.Error($"HashMap.TryInsert: Too many collisions ({jumps}) hit!");

      if (fallback != -1)
      {
        m_Buckets[fallback].Fill(key, val, hash31);
        ++m_Count;
        ++m_Version;

        if (jumps > m_Params.RehashThreshold)
        {
          Rehash();
        }

        return true;
      }

      return false;
    }

    private bool IsEmptyBucket(Bucket bucket)
    {
      return bucket.DirtyHash == 0 && m_KeyComparator.IsNullKey(bucket.Key);
    }

    private bool IsFreeBucket(Bucket bucket)
    {
      return m_KeyComparator.IsNullKey(bucket.Key);
    }

    private bool IsSmearedBucket(Bucket bucket)
    {
      // A "smeared" bucket is the result of a bucket that was first dirtied
      // (via a collision), and subsequently cleared. This is necessary to
      // preserve the state of the jump graph, keeping lookups with collisions
      // reliable and reproducible.
      // Calling Rehash() eliminates all smeared buckets
      // (but not all dirty buckets!).
      return bucket.DirtyHash < 0 && m_KeyComparator.IsNullKey(bucket.Key);
    }

    private void CalcHashJump(TKey key, out int hash31, out int jump)
    {
      hash31 = m_KeyComparator.GetHashCode(key) & int.MaxValue;
      jump   = m_Params.CalcJump(hash31, m_Buckets.Length);
    }

    private int FindBucket(TKey key, out Bucket bucket)
    {
      bucket = default;

      if (m_Count == 0 || m_KeyComparator.IsNullKey(key))
      {
        return -1;
      }

      CalcHashJump(key, out int hash31, out int jump);

      int i = hash31 % m_Buckets.Length;
      int jumps = 0;
      int found = 0;

      do
      {
        found = BucketEquals(i, hash31, key);

        if (found == 0) // NOPE
        {
          return -1;
        }

        if (found > 0) // YEP
        {
          bucket = m_Buckets[i];
          return i;
        }

        // SKIP

        i = (i + jump) % m_Buckets.Length;
      }
      while (++jumps < m_Count);

      return -1;
    }

    private int BucketEquals(int i, int hash31, TKey key)
    {
      const int SKIP = -1, NOPE = 0, YEP = +1;

      if (i < 0 || i >= m_Buckets.Length)
      {
        return NOPE;
      }

      var bucket = m_Buckets[i];

      if (IsFreeBucket(bucket))
      {
        return -(bucket.DirtyHash >> 31); // -1 or 0 / FALSE or NULL
      }

      if (bucket.Hash == hash31 && m_KeyComparator.Equals(key, bucket.Key))
      {
        return YEP;
      }

      return SKIP;
    }


    private void MakeBuckets()
    {
      m_Count = m_Collisions = 0;
      m_LoadLimit = m_Params.MakeBuckets(out m_Buckets);
    }
    private void MakeBuckets(int userCapacity)
    {
      m_Count = m_Collisions = 0;
      m_LoadLimit = m_Params.MakeBuckets(userCapacity, out m_Buckets);
    }

    private int GrowCapacity()
    {
      if (m_Params.IsFixedSize)
        return -1;

      int newcap = m_Params.CalcNextSize(m_Buckets.Length);

      if (m_Buckets.Length < newcap)
      {
        Rehash(newcap);
      }

      return m_LoadLimit;
    }

    private void Rehash()
    {
      Rehash(m_Buckets.Length);
    }

    private void Rehash(int newSize)
    {
      if (m_Params.IsFixedSize && m_Buckets.Length != newSize)
      {
        Orator.ErrorOnce($"Oh no! A fixed HashMap is trying to change its size! oldSize={m_Buckets.Length},newSize={newSize}");
        return;
      }

      m_Collisions = 0;
      m_LoadLimit  = m_Params.CalcLoadLimit(newSize);

      var newBuckets = new Bucket[newSize];

      if (m_Count == 0 && m_Buckets.Length != newSize)
      {
        m_Buckets = newBuckets;
        return;
      }

      int count = 0;
      int hash31, jump;
      foreach (var bucket in m_Buckets)
      {
        if (IsEmptyBucket(bucket))
          continue;

        hash31 = bucket.Hash;
        jump   = m_Params.CalcJump(hash31, newSize);

        m_Collisions += bucket.RehashClone(hash31).ForcePlaceIn(newBuckets, jump, m_KeyComparator);

        ++count;
      }

      OAssert.True(count == m_Count, "HashMap.Rehash");

      m_Count   = count;
      m_Buckets = newBuckets;

      // Leave version unchanged, since the top-level data contents should be the same
    }

    #endregion Internal Methods

  } // end partial class HashMap

}