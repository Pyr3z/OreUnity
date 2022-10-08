/*! @file       Runtime/HashMap+Bucket.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-29
**/

using System.Collections.Generic;
using JetBrains.Annotations;


namespace Ore
{

  public partial class HashMap<K,V>
  {
    internal struct Bucket
    {
      // not sure if struct is actually better in this case~

      public int Hash
      {
        [Pure]
        get => DirtyHash & int.MaxValue;
        internal
        set => DirtyHash = (value & int.MaxValue) | (DirtyHash & int.MinValue);
      }


      public int DirtyHash; // "dirty" bit implements linear probing for collision resolution
      public K   Key;
      public V   Value; // direct member access is faster


      [Pure]
      public bool IsFree([NotNull] in IComparator<K> keyEq)
      {
        return (DirtyHash & int.MaxValue) == 0 && keyEq.IsNone(Key);
      }

      [Pure]
      public bool IsSmeared([NotNull] in IComparator<K> keyEq)
      {
        return DirtyHash == int.MinValue && keyEq.IsNone(Key);
      }

      [Pure]
      public bool MightBeEmpty()
      {
        return (DirtyHash & int.MaxValue) == 0;
      }


      [Pure]
      public KeyValuePair<K,V> GetPair()
      {
        return new KeyValuePair<K,V>(Key, Value);
      }


      public void Smear()
      {
        // A "smeared" bucket is the result of a bucket that was first dirtied
        // (via a collision), and subsequently cleared. This is necessary to
        // preserve the state of the jump graph, keeping lookups with collisions
        // reliable and reproducible.
        // Calling Rehash() eliminates all smeared buckets
        // (but not all dirty buckets!).

        Key        = default;
        Value      = default;
        DirtyHash &= int.MinValue;
      }

      [Pure]
      public Bucket RehashClone(int hash31)
      {
        return new Bucket
        {
          Key       = Key,
          Value     = Value,
          DirtyHash = hash31
        };
      }

      [Pure]
      public int PlaceIn([NotNull] Bucket[] buckets, int jump, [NotNull] in IComparator<K> keyEq)
      {
        int hash       = Hash;
        int collisions = 0;
        int i          = hash % buckets.Length;

        var bucket = buckets[i]; // keep in mind bucket != buckets[i] now
        while (!keyEq.IsNone(bucket.Key))
        {
          if (hash == bucket.Hash && keyEq.Equals(Key, bucket.Key))
          {
            return collisions | int.MinValue;
          }

          if (bucket.DirtyHash >= 0)
          {
            buckets[i].DirtyHash |= int.MinValue;
            ++collisions;
          }

          i = (i + jump) % buckets.Length;
          bucket = buckets[i];
        }

        buckets[i] = this;
        return collisions;
      }

      [Pure]
      public int ForcePlaceIn([NotNull] Bucket[] buckets, int jump, [NotNull] IComparator<K> keyEq)
      {
        int collisions = 0;
        int i          = Hash % buckets.Length;

        while (!keyEq.IsNone(buckets[i].Key))
        {
          if (buckets[i].DirtyHash >= 0)
          {
            buckets[i].DirtyHash |= int.MinValue;
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