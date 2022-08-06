/** @file       Runtime/Coalescer.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-07-12
 *
 *  @brief      A container that only yields items if they are valid
 *              (by default: is not equal to default(T)).
 *
 *  @todo       Probably migrate this implementation + others to a
 *              "Containers" namespace?
**/

using System.Collections;
using System.Collections.Generic;


namespace Ore
{

  public struct Coalescer<T> : IEnumerable<T>
  {
    public delegate bool Validator(T item);
    public static bool DefaultValidator(T item) => !Equals(item, default(T));


    private IEnumerable<T>  m_Items;
    private Validator       m_Validator;


    public Coalescer(params T[] items)
      : this(items, validator: DefaultValidator)
    {
    }
    public Coalescer(IEnumerable<T> items, Validator validator = null)
    {
      m_Items = items ?? new T[0];
      m_Validator = validator ?? DefaultValidator;
    }

    public Coalescer<T> SetItems(params T[] items)
    {
      m_Items = items ?? new T[0];
      return this;
    }

    public Coalescer<T> SetItems(IEnumerable<T> items)
    {
      m_Items = items ?? new T[0];
      return this;
    }

    public Coalescer<T> SetValidator(Validator validator)
    {
      m_Validator = validator ?? DefaultValidator;
      return this;
    }


    public bool TryCoalesce(out T item)
    {
      var it = GetEnumerator();
      if (it.MoveNext())
      {
        item = it.Current;
        return true;
      }
      else
      {
        item = default;
        return false;
      }
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
