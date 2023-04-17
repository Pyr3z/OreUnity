/*! @file       Runtime/HashMap+Impl.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-29
**/

using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;


namespace Ore
{
  public partial class HashMap<K,V>
  {
    [SerializeField] // the only serializable field in this class
    protected HashMapParams m_Params = HashMapParams.Default;

    protected IComparator<K> m_KeyComparator   = Comparator<K>.Default;
    protected IComparator<V> m_ValueComparator = null;


    Bucket[] m_Buckets;
    Bucket   m_BadBucket;

    int m_Count, m_Collisions, m_LoadLimit;
    int m_LongestChain;
    int m_Version;

    int m_CachedLookup = int.MinValue;


  #if UNITY_INCLUDE_TESTS // expose more fields for unit testing purposes
    internal Bucket[] Buckets => m_Buckets;
    internal int CachedLookup => m_CachedLookup;
    internal int Collisions   => m_Collisions;
    internal int LongestChain => m_LongestChain;
    internal int LifetimeAllocs { get; private set; }
  #endif


    internal bool ClearAlloc()
    {
      bool alreadyClear = m_Count == 0;

      m_LongestChain = m_Collisions = m_Count = 0;
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

    internal bool ClearNoAlloc() // tests verify that NoAlloc is consistently faster
    {
      bool alreadyClear = m_Count == 0;

      m_LongestChain = m_Collisions = m_Count = 0;
      System.Array.Clear(m_Buckets, 0, m_Buckets.Length);

      if (!alreadyClear)
      {
        ++m_Version;
        return true;
      }

      return false;
    }


    bool TryInsert(in K key, in V val, bool overwrite, out int i)
    {
      if (m_KeyComparator.IsNone(key))
      {
        i = -1;
        return false;
      }

      if (m_Count >= m_LoadLimit)
      {
        if (Grow() <= m_Count)
        {
          i = -1;
          return false;
        }
      }

      int hash31 = m_KeyComparator.GetHashCode(key) & int.MaxValue;
      int jump   = m_Params.CalcJump(hash31, m_Buckets.Length);

      i = hash31 % m_Buckets.Length; // high chance that we immediately found the index O(1)

      int fallback = -1;
      int jumps = 0;

      do
      {
        var currKey = m_Buckets[i].Key;
        int dirtyHash = m_Buckets[i].DirtyHash;

        if (dirtyHash == int.MinValue && fallback == -1 && m_KeyComparator.IsNone(currKey))
        {
          // fallback is a smeared bucket.
          fallback = i;
          // if it ends up being the final smeared bucket in the jump chain,
          // it will be used instead of the next empty slot.
        }
        else if ((dirtyHash & int.MaxValue) == 0 && m_KeyComparator.IsNone(currKey))
        {
          if (fallback != -1) // end of smear chain;
            i = fallback;     // we can fill the last smear instead

          m_Buckets[i].Key       = key;
          m_Buckets[i].Value     = val;
          m_Buckets[i].DirtyHash = hash31 | (dirtyHash & int.MinValue);

          ++ m_Count;
          ++ m_Version;

          m_CachedLookup = i;

          return true;
        }
        else if ((dirtyHash & int.MaxValue) == hash31 && m_KeyComparator.Equals(key, currKey))
        {
          // equivalent bucket found

          if (!overwrite || (!(m_ValueComparator is null) &&
                             m_ValueComparator.Equals(val, m_Buckets[i].Value)))
          {
            return false;
          }

          m_Buckets[i].Key = key;
          m_Buckets[i].Value = val;
          ++ m_Version;

          m_CachedLookup = i;

          return true;
        }

        if (dirtyHash >= 0)
        {
          m_Buckets[i].DirtyHash = dirtyHash | int.MinValue;
          ++ m_Collisions;
        }

        i = (i + jump) % m_Buckets.Length;

        if (++jumps > m_LongestChain)
        {
          m_LongestChain = jumps;
        }
      }
      while (jumps < m_Buckets.Length);

      // MEGA bad if we reach here

      throw new UnanticipatedException($"HashMap.TryInsert: Too many consecutive collisions! hash31={hash31}, jumps={jumps}, key={key}");
    }

    int FindBucket([CanBeNull] in K key, int hash31 = -1)
    {
      if (m_Count == 0 || m_KeyComparator.IsNone(key))
      {
        return m_CachedLookup = int.MinValue;
      }

      if (m_CachedLookup >= 0 && m_KeyComparator.Equals(key, m_Buckets[m_CachedLookup].Key))
      {
        return m_CachedLookup;
      }

      if (hash31 < 0)
      {
        hash31 = m_KeyComparator.GetHashCode(key) & int.MaxValue;
      }

      int ilen   = m_Buckets.Length;
      int i      = hash31 % ilen;
      int jump   = m_Params.CalcJump(hash31, ilen);
      int jumps  = m_LongestChain;

      do
      {
        var bucket = m_Buckets[i];

        if (bucket.DirtyHash == 0 && m_KeyComparator.IsNone(bucket.Key))
        {
          break;
        }
        
        if ((bucket.DirtyHash & int.MaxValue) == hash31 && m_KeyComparator.Equals(key, bucket.Key))
        {
          return m_CachedLookup = i;
        }

        // else, NEXT

        i = (i + jump) % ilen;
      }
      while (jumps --> 0);

      m_CachedLookup = ~ i;

      return ~ (m_LongestChain - jumps);
    }

    int FindValue(in V value)
    {
      var cmp = m_ValueComparator ?? Comparator<V>.Default;

      for (int i = 0, left = m_Count; left > 0; ++i)
      {
        var bucket = m_Buckets[i];

        if ((bucket.DirtyHash & int.MaxValue) == 0 && m_KeyComparator.IsNone(bucket.Key))
          continue;

        if (cmp.Equals(bucket.Value, value))
          return i;

        -- left;
      }

      return -1;
    }


    int Grow()
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

    void Rehash(int newSize)
    {
      if (m_Params.IsFixedSize && m_Buckets.Length != newSize)
      {
        Orator.ErrorOnce($"Oh no! A fixed HashMap is trying to change its size! oldSize={m_Buckets.Length},newSize={newSize}");
        return;
      }

      m_LongestChain = m_Collisions = 0;
      m_CachedLookup = -1;
      m_LoadLimit    = m_Params.CalcLoadLimit(newSize);

      var newBuckets = new Bucket[newSize];

      #if UNITY_INCLUDE_TESTS
      ++ LifetimeAllocs;
      #endif

      if (m_Count == 0)
      {
        m_Buckets = newBuckets; // potential GC concern
        return;
      }

      int count = 0;
      foreach (var bucket in m_Buckets)
      {
        if (bucket.MightBeEmpty() && m_KeyComparator.IsNone(bucket.Key))
          continue;

        int hash31 = bucket.DirtyHash & int.MaxValue;
        int jump   = m_Params.CalcJump(hash31, newSize);

        var (c,j) = bucket.RehashClone(hash31).PlaceIn(newBuckets, jump, m_KeyComparator);
        if (j > m_LongestChain)
          m_LongestChain = j;
        m_Collisions += c;

        if (++count == m_Count)
          break;
      }

      m_Count   = count;
      m_Buckets = newBuckets; // potential GC concern

      // Leave version unchanged, since the top-level data contents should be the same
    }


    int IntersectSlow(IEnumerable<K> keys, IEnumerable<V> values, bool overwrite)
    {
      int delta = 0;

      // TODO

      return delta;
    }

  } // end partial class HashMap
}