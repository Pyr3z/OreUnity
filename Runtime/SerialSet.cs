/*! @file       Runtime/SerialSet.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-08-18
 *
 *  It's like a HashSet<T>, but faster and serializable.
**/

using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;


namespace Ore
{
  [System.Serializable]
  public class StringSet : SerialSet<string> { }
  [System.Serializable]
  public class ObjectSet : SerialSet<Object> { }

  /// <summary>
  /// Serializable HashSet. The original plan was to implement a faster version,
  /// but now this is TODO.
  /// </summary>
  [System.Serializable]
  public class SerialSet<T> : ISet<T>, ISerializationCallbackReceiver
  {
    public int Count => m_Set.Count;

    public bool IsReadOnly => false;
    
    
    [SerializeField]
    private T[] m_Items = System.Array.Empty<T>();
    
    [System.NonSerialized]
    private HashSet<T> m_Set;
    [System.NonSerialized]
    private bool m_Dirty;

    
    public SerialSet()
    {
      m_Set = new HashSet<T>();
    }
    
    public SerialSet([CanBeNull] IEnumerable<T> items)
    {
      if (items is null)
      {
        m_Set = new HashSet<T>();
      }
      else
      {
        m_Set = new HashSet<T>(items);
      }
    }


    public bool Add([NotNull] T item)
    {
      return m_Dirty |= m_Set.Add(item);
    }
    
    void ICollection<T>.Add([CanBeNull] T item)
    {
      m_Dirty |= m_Set.Add(item);
    }

    public void Clear()
    {
      m_Set.Clear();
    }

    public bool Contains([CanBeNull] T item)
    {
      return m_Set.Contains(item);
    }

    public void CopyTo([NotNull] T[] array, int start)
    {
      m_Set.CopyTo(array, start);
    }

    public bool Remove([CanBeNull] T item)
    {
      return m_Set.Remove(item);
    }
    
    public IEnumerator<T> GetEnumerator()
    {
      return m_Set.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void ExceptWith([NotNull] IEnumerable<T> other)
    {
      _ = NAND(other);
    }
    public bool NAND([NotNull] IEnumerable<T> other)
    {
      int precount = m_Set.Count;
      m_Set.ExceptWith(other);
      return m_Dirty |= (m_Set.Count != precount);
    }

    public void IntersectWith([NotNull] IEnumerable<T> other)
    {
      _ = AND(other);
    }
    public bool AND([NotNull] IEnumerable<T> other)
    {
      int precount = m_Set.Count;
      m_Set.IntersectWith(other);
      return m_Dirty |= (m_Set.Count != precount);
    }

    public bool IsProperSubsetOf([NotNull] IEnumerable<T> other)
    {
      return m_Set.IsProperSubsetOf(other);
    }

    public bool IsProperSupersetOf([NotNull] IEnumerable<T> other)
    {
      return m_Set.IsProperSupersetOf(other);
    }

    public bool IsSubsetOf([NotNull] IEnumerable<T> other)
    {
      return m_Set.IsSubsetOf(other);
    }

    public bool IsSupersetOf([NotNull] IEnumerable<T> other)
    {
      return m_Set.IsSupersetOf(other);
    }

    public bool Overlaps([NotNull] IEnumerable<T> other)
    {
      return m_Set.Overlaps(other);
    }

    public bool SetEquals([NotNull] IEnumerable<T> other)
    {
      return m_Set.SetEquals(other);
    }
    
    public void SymmetricExceptWith([NotNull] IEnumerable<T> other)
    {
      _ = XOR(other);
    }
    public bool XOR([NotNull] IEnumerable<T> other)
    {
      int precount = m_Set.Count;
      m_Set.SymmetricExceptWith(other);
      return m_Dirty |= (m_Set.Count != precount);
    }

    public void UnionWith([NotNull] IEnumerable<T> other)
    {
      _ = OR(other);
    }
    public bool OR([NotNull] IEnumerable<T> other)
    {
      int precount = m_Set.Count;
      m_Set.UnionWith(other);
      return m_Dirty |= (m_Set.Count != precount);
    }

    
    public void TrimSerialList()
    {
      m_Set.Clear();
      m_Set.UnionWith(m_Items);
      m_Items = new T[m_Set.Count];
      m_Set.CopyTo(m_Items);
      m_Dirty = false;
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
      m_Set.Clear();
      m_Set.UnionWith(m_Items);
      m_Dirty = false;
    }
    
    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
      if (m_Dirty)
      {
        m_Items = new T[m_Set.Count];
        m_Set.CopyTo(m_Items);
        m_Dirty = false;
      }
    }
    
  } // end class SerialSet
}