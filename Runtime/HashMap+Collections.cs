/*! @file       Runtime/HashMap+Collections.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-10-26
**/

using JetBrains.Annotations;

using System.Collections;
using System.Collections.Generic;


namespace Ore
{
  public partial class HashMap<K,V>
  {

    public sealed class KeySet :
      ISet<K>,
      IReadOnlyCollection<K>,
      ICollection,
      IUseComparator<K>
    {
      public int    Count          => m_Parent.m_Count;
      public bool   IsReadOnly     { get; private set; } = true;
      public bool   IsSynchronized => ((ICollection)m_Parent).IsSynchronized;
      public object SyncRoot       => ((ICollection)m_Parent).SyncRoot;

      public IComparator<K> Comparator => m_Parent.KeyComparator;


      private readonly HashMap<K,V> m_Parent;


      internal KeySet([NotNull] HashMap<K,V> forMap)
      {
        m_Parent = forMap;
      }


      [PublicAPI]
      public KeySet MakeWriteProxy()
      {
        IsReadOnly = false;
        return this;
      }


      public IEnumerator<K> GetEnumerator()
      {
        // lazy implementation for now
        foreach (var ( key , _ ) in m_Parent)
        {
          yield return key;
        }
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }

      public bool Contains(K key)
      {
        return m_Parent.ContainsKey(key);
      }

      public void CopyTo(K[] array, int startIndex)
      {
        if (startIndex < 0)
        {
          throw new System.ArgumentOutOfRangeException(nameof(startIndex));
        }

        int ilen = array.Length;

        using (var iter = GetEnumerator())
        {
          while (startIndex < ilen && iter.MoveNext())
          {
            array[startIndex] = iter.Current;
            ++startIndex;
          }
        }
      }

      void ICollection.CopyTo(System.Array array, int startIndex)
      {
        if (startIndex < 0)
        {
          throw new System.ArgumentOutOfRangeException(nameof(startIndex));
        }

        int ilen = array.Length;

        using (var iter = GetEnumerator())
        {
          while (startIndex < ilen && iter.MoveNext())
          {
            array.SetValue(iter.Current, startIndex);
            ++startIndex;
          }
        }
      }


      void ICollection<K>.Add(K key)
      {
        if (IsReadOnly)
          throw new System.NotSupportedException(nameof(KeySet) + " is read only.");

        if (key != null)
          m_Parent.Add(key, default(V));
      }

      bool ISet<K>.Add(K item)
      {
        if (IsReadOnly)
          throw new System.NotSupportedException(nameof(KeySet) + " is read only.");

        return m_Parent.Map(item, default(V));
      }

      void ISet<K>.ExceptWith(IEnumerable<K> other)
      {
        if (IsReadOnly)
          throw new System.NotSupportedException(nameof(KeySet) + " is read only.");

        if (m_Parent.m_Count == 0)
          return;

        if (ReferenceEquals(this, other))
        {
          m_Parent.Clear();
          return;
        }

        foreach (var key in other)
        {
          _ = m_Parent.Unmap(key);
        }
      }

      void ISet<K>.IntersectWith(IEnumerable<K> other)
      {
        if (IsReadOnly)
          throw new System.NotSupportedException(nameof(KeySet) + " is read only.");

        if (m_Parent.m_Count == 0)
          return;

        if (other is ICollection<K> coll)
        {
          if (coll.Count == 0)
          {
            m_Parent.Clear();
            return;
          }

          // TODO check for equal comparators? even necessary?
          foreach (var key in this)
          {
            if (!coll.Contains(key))
              m_Parent.Unmap(key);
          }
        }
        else
        {
          var slowSet = new HashSet<K>(other, Comparator);

          foreach (var key in this)
          {
            if (!slowSet.Contains(key))
              m_Parent.Unmap(key);
          }
        }
      }

      void ISet<K>.SymmetricExceptWith(IEnumerable<K> other)
      {
        if (IsReadOnly)
          throw new System.NotSupportedException(nameof(KeySet) + " is read only.");

        if (ReferenceEquals(this, other))
        {
          m_Parent.Clear();
        }
        else if (m_Parent.m_Count == 0)
        {
          ((ISet<K>)this).UnionWith(other);
        }
        // TODO: this needs to be implemented in order for 100% correctness in all cases
        // else if (other is IUseComparator<K> iuc && Comparator.Equals(iuc.Comparator))
        // {
        // }
        else
        {
          foreach (var key in other)
          {
            if (m_Parent.TryInsert(key, default(V), false, out int i))
            {
            }
            else if (i >= 0)
            {
              m_Parent.m_Buckets[i].Smear();
              -- m_Parent.m_Count;
              ++ m_Parent.m_Version;
            }
          }
        }
      }

      void ISet<K>.UnionWith(IEnumerable<K> other)
      {
        if (IsReadOnly)
          throw new System.NotSupportedException(nameof(KeySet) + " is read only.");

        foreach (var key in other)
        {
          m_Parent.Add(key, default(V));
        }
      }

      void ICollection<K>.Clear()
      {
        if (IsReadOnly)
          throw new System.NotSupportedException(nameof(KeySet) + " is read only.");

        m_Parent.Clear();
      }

      bool ICollection<K>.Remove(K key)
      {
        if (IsReadOnly)
          throw new System.NotSupportedException(nameof(KeySet) + " is read only.");

        return m_Parent.Unmap(key);
      }

      bool ISet<K>.IsProperSubsetOf(IEnumerable<K> other)
      {
        if (ReferenceEquals(this, other))
          return false;

        int count = m_Parent.m_Count;

        if (other is ICollection<K> coll)
        {
          if (count == 0)
            return coll.Count > 0;

          if (count >= coll.Count)
            return false;

          if (other is IUseComparator<K> iuc && Comparator.Equals(iuc.Comparator))
          {
            foreach (var key in this)
            {
              if (!coll.Contains(key))
                return false;
            }

            return true;
          }
        }

        var (unique,unfound) = CountUniqueAndUnfoundKeys(other, false);

        return count == unique && unfound > 0;
      }

      bool ISet<K>.IsProperSupersetOf(IEnumerable<K> other)
      {
        if (ReferenceEquals(this, other))
          return false;

        int count = m_Parent.m_Count;

        if (count == 0)
          return false;

        if (other is ICollection<K> coll)
        {
          if (coll.Count == 0)
            return true;

          if (coll.Count >= count)
            return false;

          if (other is IUseComparator<K> iuc && Comparator.Equals(iuc.Comparator))
          {
            foreach (var key in other)
            {
              if (!m_Parent.ContainsKey(key))
                return false;
            }

            return true;
          }
        }

        var (unique,unfound) = CountUniqueAndUnfoundKeys(other, true);

        return unique < count && unfound == 0;
      }

      bool ISet<K>.IsSubsetOf(IEnumerable<K> other)
      {
        int count = m_Parent.m_Count;

        if (ReferenceEquals(this, other) || count == 0)
          return true;

        if (other is ICollection<K> coll)
        {
          if (count > coll.Count)
            return false;

          if (other is IUseComparator<K> iuc && Comparator.Equals(iuc.Comparator))
          {
            foreach (var key in this)
            {
              if (!coll.Contains(key))
                return false;
            }

            return true;
          }
        }

        var (unique,unfound) = CountUniqueAndUnfoundKeys(other, false);

        return count == unique && unfound >= 0;
      }

      bool ISet<K>.IsSupersetOf(IEnumerable<K> other)
      {
        if (ReferenceEquals(this, other))
          return true;

        if (other is ICollection<K> coll)
        {
          if (coll.Count == 0)
            return true;

          if (other is IUseComparator<K> iuc && Comparator.Equals(iuc.Comparator) &&
              coll.Count > m_Parent.m_Count)
          {
            return false;
          }
        }

        foreach (var key in other)
        {
          if (!m_Parent.ContainsKey(key))
            return false;
        }

        return true;
      }

      bool ISet<K>.Overlaps(IEnumerable<K> other)
      {
        if (ReferenceEquals(this, other))
          return true;

        if (m_Parent.m_Count == 0)
          return false;

        foreach (var key in other)
        {
          if (m_Parent.ContainsKey(key))
            return true;
        }

        return false;
      }

      bool ISet<K>.SetEquals(IEnumerable<K> other)
      {
        if (ReferenceEquals(this, other))
          return true;

        int count = m_Parent.m_Count;

        if (other is KeySet keySet)
        {
          if (ReferenceEquals(m_Parent, keySet.m_Parent))
            return true;

          if (count != keySet.Count || !Comparator.Equals(keySet.Comparator))
            return false;              // TODO ^ support?

          foreach (var key in keySet)
          {
            if (!m_Parent.ContainsKey(key))
              return false;
          }

          return true;
        }

        if (other is ICollection<K> coll)
        {
          // TODO for now assumes coll is already deduped

          if (count != coll.Count)
            return false;

          if (other is IUseComparator<K> iuc)
          {
            if (!Comparator.Equals(iuc.Comparator)) // TODO support?
              return false;

            foreach (var key in other)
            {
              if (!m_Parent.ContainsKey(key))
                return false;
            }

            return true;
          }
        }

        var (unique,unfound) = CountUniqueAndUnfoundKeys(other, true);

        return count == unique && unfound == 0;
      }


      (int unique, int unfound) CountUniqueAndUnfoundKeys(IEnumerable<K> other, bool scUnfound)
      {
        if (Count == 0)
        {
          using (var iter = other.GetEnumerator())
          {
            if (iter.MoveNext())
            {
              return (0,1);
            }
          }

          return (0,0);
        }

        int unique = 0, unfound = 0;

        if (scUnfound)
        {
          foreach (var okey in other)
          {
            int i = m_Parent.FindBucket(in okey);
            if (i >= 0)
            {
              // TODO use BitSet! this currently assumes other has no duplicates
              ++ unique;
            }
            else
            {
              return (unique,1);
            }
          }
        }
        else
        {
          foreach (var okey in other)
          {
            int i = m_Parent.FindBucket(in okey);
            if (i >= 0)
            {
              // TODO use BitSet! this currently assumes other has no duplicates
              ++ unique;
            }
            else
            {
              ++ unfound;
            }
          }
        }

        return (unique,unfound);
      }

    } // end nested class KeySet


    /// <remarks>
    /// Unfortunately has to be quite copy-paste compared to KeySet.
    /// We can thank C# generics for that one.
    /// </remarks>>
    public sealed class ValueCollection :
      ICollection<V>,
      IReadOnlyCollection<V>,
      ICollection
    {
      public int    Count          => m_Parent.m_Count;
      public bool   IsReadOnly     => true;
      public bool   IsSynchronized => false;
      public object SyncRoot       => this;


      private readonly HashMap<K,V> m_Parent;


      internal ValueCollection([NotNull] HashMap<K,V> forMap)
      {
        m_Parent = forMap;
      }


      public IEnumerator<V> GetEnumerator()
      {
        // lazy
        foreach (var ( _ , val ) in m_Parent)
        {
          yield return val;
        }
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }

      public bool Contains(V value)
      {
        return m_Parent.ContainsValue(value);
      }

      public void CopyTo(V[] array, int startIndex)
      {
        if (startIndex < 0)
        {
          throw new System.ArgumentOutOfRangeException(nameof(startIndex));
        }

        int ilen = array.Length;

        using (var iter = GetEnumerator())
        {
          while (startIndex < ilen && iter.MoveNext())
          {
            array[startIndex] = iter.Current;
            ++startIndex;
          }
        }
      }

      void ICollection.CopyTo(System.Array array, int startIndex)
      {
        if (startIndex < 0)
        {
          throw new System.ArgumentOutOfRangeException(nameof(startIndex));
        }

        int ilen = array.Length;

        using (var iter = GetEnumerator())
        {
          while (startIndex < ilen && iter.MoveNext())
          {
            array.SetValue(iter.Current, startIndex);
            ++startIndex;
          }
        }
      }


      void ICollection<V>.Add(V value)
      {
        throw new System.NotSupportedException(nameof(ValueCollection) + " is read only.");
      }

      void ICollection<V>.Clear()
      {
        throw new System.NotSupportedException(nameof(ValueCollection) + " is read only.");
      }

      bool ICollection<V>.Remove(V value)
      {
        throw new System.NotSupportedException(nameof(ValueCollection) + " is read only.");
      }
    } // end nested class ValueCollection

  } // end partial class HashMap<K,V>
}