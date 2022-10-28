/*! @file       Runtime/HashMap+Collections.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-10-26
**/

using System.Collections.Generic;
using System.Collections;


namespace Ore
{
  public partial class HashMap<K,V>
  {

    public sealed class KeyCollection :
      ICollection<K>,
      IReadOnlyCollection<K>,
      ICollection
    {
      public int    Count          => m_Parent.Count;
      public bool   IsReadOnly     => true;
      public bool   IsSynchronized => ((ICollection)m_Parent).IsSynchronized;
      public object SyncRoot       => ((ICollection)m_Parent).SyncRoot;

      int ICollection<K>.Count => m_Parent.Count;


      private readonly HashMap<K,V> m_Parent;


      public KeyCollection(HashMap<K,V> forMap)
      {
        m_Parent = forMap;
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
        throw new System.NotSupportedException();
      }

      void ICollection<K>.Clear()
      {
        throw new System.NotSupportedException();
      }

      bool ICollection<K>.Remove(K key)
      {
        throw new System.NotSupportedException();
      }
    } // end nested class KeyCollection


    /// <remarks>
    /// Unfortunately has to be quite copy-paste compared to KeyCollection.
    /// We can thank C# generics for that one.
    /// </remarks>>
    public sealed class ValueCollection :
      ICollection<V>,
      IReadOnlyCollection<V>,
      ICollection
    {
      public int    Count          => m_Parent.Count;
      public bool   IsReadOnly     => true;
      public bool   IsSynchronized => ((ICollection)m_Parent).IsSynchronized;
      public object SyncRoot       => ((ICollection)m_Parent).SyncRoot;

      int ICollection<V>.Count => m_Parent.Count;


      private readonly HashMap<K,V> m_Parent;


      public ValueCollection(HashMap<K,V> forMap)
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
        throw new System.NotSupportedException();
      }

      void ICollection<V>.Clear()
      {
        throw new System.NotSupportedException();
      }

      bool ICollection<V>.Remove(V value)
      {
        throw new System.NotSupportedException();
      }
    } // end nested class ValueCollection

  } // end partial class HashMap<K,V>
}