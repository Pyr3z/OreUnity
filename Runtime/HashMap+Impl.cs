/*! @file       Runtime/HashMap+Impl.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-29
**/

using System.Collections.Generic;

using UnityEngine;


namespace Ore
{
  public partial class HashMap<K,V>
  {
    [SerializeField] // the only serializable field in this class
    protected HashMapParams m_Params = HashMapParams.Default;

    protected IComparator<K> m_KeyComparator   = Comparator<K>.Default;
    protected IComparator<V> m_ValueComparator = null;


    private Bucket[] m_Buckets;

    private int m_Count, m_Collisions, m_LoadLimit;
    private int m_Version;

    private int m_CachedLookup = int.MinValue;


  #if UNITY_INCLUDE_TESTS // expose more fields for unit testing purposes
    internal Bucket[] Buckets => m_Buckets;
    internal int CachedLookup => m_CachedLookup;
    internal int LifetimeAllocs { get; private set; }
  #endif


    internal bool ClearAlloc()
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

    internal bool ClearNoAlloc() // tests verify that NoAlloc is consistently faster
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


    private bool TryInsert(in K key, in V val, bool overwrite, out int i)
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
      else if (m_Collisions > m_LoadLimit * m_Params.LoadFactor) // essentially LF squared
      {
        Rehash();
      }

      int hash31 = m_KeyComparator.GetHashCode(key) & int.MaxValue;
      int jump   = m_Params.CalcJump(hash31, m_Buckets.Length);

      i = hash31 % m_Buckets.Length; // high chance that we immediately found the index O(1)

      int fallback = -1;
      int jumps = 0;

      do
      {
        var bucket = m_Buckets[i];

        if (bucket.DirtyHash == int.MinValue && fallback == -1 && m_KeyComparator.IsNone(bucket.Key))
        {
          // fallback is a smeared bucket.
          fallback = i;
          // if it ends up being the final smeared bucket in the jump chain,
          // it will be used instead of the next empty slot.
        }
        else if ((bucket.DirtyHash & int.MaxValue) == 0 && m_KeyComparator.IsNone(bucket.Key))
        {
          if (fallback != -1) // end of smear chain;
            i = fallback;     // we can fill the last smear instead

          m_Buckets[i].Key = key;
          m_Buckets[i].Value = val;
          m_Buckets[i].Hash = hash31;

          ++m_Count;
          ++m_Version;

          if (jumps >= m_Params.HashPrime)
          {
            Rehash();
          }

          return true;
        }
        else if (bucket.Hash == hash31 && m_KeyComparator.Equals(key, bucket.Key))
        {
          // equivalent bucket found

          if (!overwrite || (m_ValueComparator is {} &&
                             m_ValueComparator.Equals(val, bucket.Value)))
          {
            return false;
          }

          m_Buckets[i].Key = key;
          m_Buckets[i].Value = val;
          ++m_Version;

          if (jumps >= m_Params.HashPrime)
          {
            Rehash();
          }

          return true;
        }

        if (bucket.DirtyHash >= 0)
        {
          m_Buckets[i].DirtyHash |= int.MinValue;
          ++m_Collisions;
        }

        i = (i + jump) % m_Buckets.Length;
      }
      while (++jumps < m_Buckets.Length);

      // MEGA bad if we reach here

      Orator.ErrorOnce($"HashMap.TryInsert: Too many collisions ({jumps}) hit!");

      if (fallback != -1)
      {
        m_Buckets[fallback].Key = key;
        m_Buckets[fallback].Value = val;
        m_Buckets[fallback].Hash = hash31;
        ++m_Count;
        ++m_Version;

        if (jumps >= m_Params.HashPrime)
        {
          Rehash();
        }

        return true;
      }

      i = -1;
      return false;
    }

    private int FindBucket(in K key)
    {
      if (m_Count == 0 || m_KeyComparator.IsNone(key))
      {
        return m_CachedLookup = int.MinValue;
      }

      if (m_CachedLookup >= 0 && m_KeyComparator.Equals(key, m_Buckets[m_CachedLookup].Key))
      {
        return m_CachedLookup;
      }

      int ilen   = m_Buckets.Length;
      int hash31 = m_KeyComparator.GetHashCode(key) & int.MaxValue;
      int i      = hash31 % ilen;
      int jump   = m_Params.CalcJump(hash31, ilen);
      int jumps  = 0;

      do
      {
        var bucket = m_Buckets[i];

        if (bucket.DirtyHash == 0 && m_KeyComparator.IsNone(bucket.Key))
        {
          return m_CachedLookup = -i;
        }
        
        if (bucket.Hash == hash31 && m_KeyComparator.Equals(key, bucket.Key))
        {
          return m_CachedLookup = i;
        }

        // else, NEXT

        i = (i + jump) % ilen;
      }
      while (++jumps < ilen); // can't be < m_Count because of smearing algorithm =/

      return m_CachedLookup = -i;
    }


    private void MakeBuckets()
    {
      m_Count = m_Collisions = 0;
      m_LoadLimit = m_Params.MakeBuckets(out m_Buckets);

      #if UNITY_INCLUDE_TESTS
      ++LifetimeAllocs;
      #endif
    }

    private int Grow()
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
      m_CachedLookup = -1;
      m_LoadLimit  = m_Params.CalcLoadLimit(newSize);

      var newBuckets = new Bucket[newSize];

      #if UNITY_INCLUDE_TESTS
      ++LifetimeAllocs;
      #endif

      if (m_Count == 0)
      {
        m_Buckets = newBuckets; // potential GC concern
        return;
      }

      int count = 0;
      int hash31, jump;
      foreach (var bucket in m_Buckets)
      {
        if (bucket.MightBeEmpty() && m_KeyComparator.IsNone(bucket.Key))
          continue;

        hash31 = bucket.Hash;
        jump   = m_Params.CalcJump(hash31, newSize);

        m_Collisions += bucket.RehashClone(hash31).PlaceIn(newBuckets, jump, m_KeyComparator);

        if (++count == m_Count)
          break;
      }

      m_Count   = count;
      m_Buckets = newBuckets; // potential GC concern

      // Leave version unchanged, since the top-level data contents should be the same
    }

  } // end partial class HashMap
}