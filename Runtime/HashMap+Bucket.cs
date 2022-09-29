/*! @file       Runtime/HashMap+Bucket.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-08
**/

using System.Collections.Generic;
using JetBrains.Annotations;


namespace Ore
{

  public partial class HashMap<TKey,TValue>
  {
    protected struct Bucket
    {
      public int Hash
      {
        get => DirtyHash & int.MaxValue;
        set => DirtyHash = value | (DirtyHash & int.MinValue);
      }

      public int    DirtyHash;
      public TKey   Key;
      public TValue Value; // direct member access is faster


      public void Fill(TKey key, TValue val, int hash31)
      {
        Key         = key; // <-- boxed
        Value       = val;
        DirtyHash   = hash31 | (DirtyHash & int.MinValue); // preserve dirt bit
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

      public void Smear()
      {
        Key          = default;
        Value        = default;
        DirtyHash   &= int.MinValue;
      }

      public Bucket RehashClone(int hash31)
      {
        return new Bucket
        {
          Key         = Key,
          Value       = Value,
          DirtyHash   = hash31
        };
      }

      public int PlaceIn([NotNull] Bucket[] buckets, int jump, [NotNull] IHashKeyComparator<TKey> keyEq)
      {
        int hash       = Hash;
        int collisions = 0;
        int i          = hash % buckets.Length;

        var bucket = buckets[i]; // keep in mind bucket != buckets[i] now
        while (!keyEq.IsNullKey(bucket.Key))
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

      public int ForcePlaceIn([NotNull] Bucket[] buckets, int jump, [NotNull] IHashKeyComparator<TKey> keyEq)
      {
        int collisions = 0;
        int i          = Hash % buckets.Length;

        while (!keyEq.IsNullKey(buckets[i].Key))
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