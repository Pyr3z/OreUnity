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
      public bool MightBeEmpty()
      {
        return (DirtyHash & int.MaxValue) == 0;
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
        // (this isn't guaranteed to result in a smeared bucket; it might have 0 collisions.)
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
      public int PlaceIn([NotNull] Bucket[] buckets, int jump, [NotNull] IComparator<K> keyEq)
      {
        int collisions = 0;
        int ilen       = buckets.Length;
        int i          = Hash % ilen;

        while (!keyEq.IsNone(buckets[i].Key) && collisions < ilen)
        {
          if (buckets[i].DirtyHash >= 0)
          {
            buckets[i].DirtyHash |= int.MinValue;
            ++collisions;
          }

          i = (i + jump) % ilen;
        }

        buckets[i] = this;
        return collisions;
      }

    } // end struct Bucket
  } // end partial class HashMap

}