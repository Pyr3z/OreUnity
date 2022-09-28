/*! @file       Runtime/HashMap+Bucket.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-08
**/

using System.Collections;
using JetBrains.Annotations;


namespace Ore
{

  public partial class HashMap<TKey,TValue>
  {
    protected struct Bucket
    {
      public object Key;   // boxing of the key is necessary
      public TValue Value; // direct member access is faster

      private int    m_DirtyHash;

      public int  Hash      => m_DirtyHash & int.MaxValue;
      public bool IsEmpty   => Key is null;
      public bool IsDefault => m_DirtyHash == 0 && Key is null;
      public bool IsDirty   => m_DirtyHash < 0;
      public bool IsSmeared => m_DirtyHash < 0 && Key is null;


      public Bucket(TKey key, TValue val, int hash31)
      {
        Key         = key; // <-- boxed
        Value       = val;
        m_DirtyHash = hash31;
      }

      public void Set(TKey key, TValue val, int hash31)
      {
        Key         = key; // <-- boxed
        Value       = val;
        m_DirtyHash = hash31 | (m_DirtyHash & int.MinValue); // preserve dirt bit
      }

      public bool TryUnpack(out (TKey key, TValue val) contents)
      {
        contents = default;

        if (Key is null)
          return false;

        contents.key = (TKey)Key;
        contents.val = Value;
        return true;
      }

      public bool MakeDirty()
      {
        if (m_DirtyHash < 0)
          return false;

        m_DirtyHash |= int.MinValue;
        return true;
      }

      public void Smear()
      {
        Key          = null;
        Value        = default;
        m_DirtyHash &= int.MinValue;
      }

      public Bucket RehashClone(int hash31)
      {
        return new Bucket
        {
          Key         = Key,
          Value       = Value,
          m_DirtyHash = hash31
        };
      }

      public int PlaceIn([NotNull] Bucket[] buckets, int jump, [NotNull] IEqualityComparer keyEq)
      {
        int hash       = Hash;
        int collisions = 0;
        int i          = hash % buckets.Length;

        var bucket = buckets[i]; // keep in mind bucket != buckets[i] now
        while (!bucket.IsEmpty)
        {
          if (hash == bucket.Hash && keyEq.Equals(Key, bucket.Key))
          {
            return collisions | int.MinValue;
          }

          if (bucket.m_DirtyHash >= 0)
          {
            buckets[i].m_DirtyHash |= int.MinValue;
            ++collisions;
          }

          i = (i + jump) % buckets.Length;
          bucket = buckets[i];
        }

        buckets[i] = this;
        return collisions;
      }

      public int PlaceIn([NotNull] Bucket[] buckets, int jump)
      {
        int collisions = 0;
        int i          = Hash % buckets.Length;

        while (!buckets[i].IsEmpty)
        {
          if (buckets[i].m_DirtyHash >= 0)
          {
            buckets[i].m_DirtyHash |= int.MinValue;
            ++collisions;
          }

          i = (i + jump) % buckets.Length;
        }

        buckets[i] = this;
        return collisions;
      }

    } // end struct Bucket
  } // end partial class HashMap

}