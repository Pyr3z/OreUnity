/*! @file       Runtime/HashMap+Bucket.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-29
**/

using JetBrains.Annotations;

using MethodImplAttribute = System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions   = System.Runtime.CompilerServices.MethodImplOptions;


namespace Ore
{

  public partial class HashMap<K,V>
  {
    internal struct Bucket
    {
      // not sure if struct is actually better in this case~

      public int DirtyHash; // "dirty" bit implements linear probing for collision resolution
      public K   Key;
      public V   Value; // direct member access is faster


      [Pure]
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public bool MightBeEmpty()
      {
        return (DirtyHash & int.MaxValue) == 0;
      }


      [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
      public (int,int) PlaceIn([NotNull] Bucket[] buckets, int jump, [NotNull] IComparator<K> keyEq)
      {
        int collisions = 0;
        int jumps      = 0;
        int ilen       = buckets.Length;
        int i          = (DirtyHash & int.MaxValue) % ilen;

        while (!keyEq.IsNone(buckets[i].Key) && jumps++ < ilen)
        {
          if (buckets[i].DirtyHash >= 0)
          {
            buckets[i].DirtyHash |= int.MinValue;
            ++collisions;
          }

          i = (i + jump) % ilen;
        }

        buckets[i] = this;
        return (collisions,jumps);
      }

    } // end struct Bucket
  } // end partial class HashMap

}