/*! @file       Runtime/Coalescer.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-07-12
 *
 *  @brief      A container that only yields items if they are valid
 *              (by default: is not equal to default(T)).
**/

// TODO Probably migrate this implementation + others to a "Containers" namespace?

using JetBrains.Annotations;

using System.Collections;
using System.Collections.Generic;


// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable HeapView.ObjectAllocation
// ReSharper disable HeapView.ObjectAllocation.Possible
// ReSharper disable HeapView.DelegateAllocation
// ReSharper disable HeapView.PossibleBoxingAllocation


namespace Ore
{

  public struct Coalescer<T> : IEnumerable<T>
  {
    public delegate bool Validator([CanBeNull] T item);
    private static bool DefaultValidator([CanBeNull] T item) => !Equals(item, default(T));


    private IEnumerable<T>  m_Items;
    private Validator       m_Validator;


    public Coalescer(params T[] items)
      : this(items, validator: DefaultValidator)
    {
    }
    public Coalescer([CanBeNull] IEnumerable<T> items, [CanBeNull] Validator validator = null)
    {
      m_Items 		= items ?? System.Array.Empty<T>();
      m_Validator = validator ?? DefaultValidator;
    }

    public Coalescer<T> SetItems(params T[] items)
    {
      m_Items = items ?? System.Array.Empty<T>();
      return this;
    }

    public Coalescer<T> SetItems([CanBeNull] IEnumerable<T> items)
    {
      m_Items = items ?? System.Array.Empty<T>();
      return this;
    }

    public Coalescer<T> SetValidator([CanBeNull] Validator validator)
    {
      m_Validator = validator ?? DefaultValidator;
      return this;
    }


    public bool TryCoalesce(out T item)
    {
      item = default(T);

      if (OAssert.FailsNullChecks(m_Items, m_Validator))
        return false;

      foreach (var i in m_Items)
      {
        if (!m_Validator(i))
          continue;

        item = i;
        return true;
      }

      return false;
    }


    public IEnumerator<T> GetEnumerator()
    {
      if (OAssert.FailsNullChecks(m_Items, m_Validator))
        yield break;

      foreach (var item in m_Items)
      {
        if (m_Validator(item))
          yield return item;
      }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    public static implicit operator T (Coalescer<T> coal)
    {
      _ = coal.TryCoalesce(out T item);
      return item;
    }

  } // end struct Coalescer<T>

}
