/*! @file       Runtime/HashMap+Impl.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-29
**/

using System.Collections.Generic;

using UnityEngine;


namespace Ore
{
  public partial class HashMap<TKey,TValue>
  {
    [SerializeField] // the only serializable field in this class
    protected HashMapParams m_Params = HashMapParams.Default;

    protected IHashKeyComparator<TKey>  m_KeyComparator   = HashKeyComparator<TKey>.Default;
    protected IEqualityComparer<TValue> m_ValueComparator = EqualityComparer<TValue>.Default;

    private int m_Count, m_Collisions, m_LoadLimit;
    private int m_Version;

    private Bucket[] m_Buckets;


    private bool TryInsert(TKey key, TValue val, bool overwrite, out int i)
    {
      if (m_KeyComparator.IsNullKey(key))
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
      else if (m_Collisions > m_LoadLimit && m_Count > m_Params.RehashThreshold)
      {
        Rehash();
      }

      CalcHashJump(key, out int hash31, out int jump);

      i = hash31 % m_Buckets.Length; // high chance that we immediately found the index O(1)

      int fallback = -1;
      int jumps = 0;

      do
      {
        var bucket = m_Buckets[i];

        if (bucket.IsEmpty(m_KeyComparator))
        {
          if (fallback != -1)
            i = fallback;

          m_Buckets[i].Fill(key, val, hash31);

          ++m_Count;
          ++m_Version;
          return true;
        }


        if (fallback == -1 && bucket.IsSmeared(m_KeyComparator))
        {
          fallback = i;
        }
        else if (bucket.IsFree(m_KeyComparator))
        {
          if (fallback != -1) // end of smear chain;
            i = fallback;     // we can fill the last smear instead

          m_Buckets[i].Fill(key, val, hash31);

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

          if (!overwrite || (m_ValueComparator is {} &&
                             m_ValueComparator.Equals(val, bucket.Value)))
          {
            return false;
          }

          m_Buckets[i].Value = val;
          ++m_Version;

          if (jumps > m_Params.RehashThreshold)
          {
            Rehash();
          }

          return true;
        }

        if (fallback == -1 && bucket.DirtyHash >= 0)
        {
          // Mark new collision
          m_Buckets[i].DirtyHash |= int.MinValue;
          ++m_Collisions;
        }

        i = (i + jump) % m_Buckets.Length;
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

    private void CalcHashJump(in TKey key, out int hash31, out int jump)
    {
      hash31 = m_KeyComparator.GetHashCode(key) & int.MaxValue;
      jump   = m_Params.CalcJump(hash31, m_Buckets.Length);
    }

    private int FindBucket(in TKey key)
    {
      if (m_Count == 0 || m_KeyComparator.IsNullKey(key))
      {
        return -1;
      }

      CalcHashJump(key, out int hash31, out int jump);

      int i = hash31 % m_Buckets.Length;
      int jumps = 0;

      do
      {
        int found = BucketEquals(m_Buckets[i], hash31, key);

        if (found > 0) // YEP
          return i;

        if (found == 0) // NOPE
          return -i;

        // else, NEXT

        i = (i + jump) % m_Buckets.Length;
      }
      while (++jumps < m_Count);

      return -i;
    }

    private int BucketEquals(in Bucket bucket, int hash31, in TKey key)
    {
      #pragma warning disable CS0219
      const int NEXT = -1, NOPE = 0, YEP = +1;
      #pragma warning restore CS0219

      if (bucket.IsEmpty(m_KeyComparator))
      {
        return -(bucket.DirtyHash >> 31); // -1 or 0 / NEXT if smeared, else NOPE
      }

      if (bucket.Hash == hash31 && m_KeyComparator.Equals(key, bucket.Key))
      {
        return YEP;
      }

      return NEXT;
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
      m_LoadLimit  = m_Params.CalcLoadLimit(newSize);

      var newBuckets = new Bucket[newSize];

      if (m_Count == 0 && m_Buckets.Length != newSize)
      {
        m_Buckets = newBuckets; // potential GC concern
        return;
      }

      int count = 0;
      int hash31, jump;
      foreach (var bucket in m_Buckets)
      {
        if (bucket.IsEmpty(m_KeyComparator))
          continue;

        hash31 = bucket.Hash;
        jump   = m_Params.CalcJump(hash31, newSize);

        m_Collisions += bucket.RehashClone(hash31).ForcePlaceIn(newBuckets, jump, m_KeyComparator);

        ++count;
      }

      OAssert.True(count == m_Count, "HashMap.Rehash");

      m_Count   = count;
      m_Buckets = newBuckets; // potential GC concern

      // Leave version unchanged, since the top-level data contents should be the same
    }

  } // end partial class HashMap
}