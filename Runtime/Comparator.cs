/*! @file       Runtime/Comparator.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-28
 *
 *  Default implementation for the IComparator<T> interface.
 *
 *  TODO: change the interface structure so that the default implementation
 *        can be a sealed class, but there is still some base class available
 *        for easy subclassing. Reason = want to avoid as much vtable as
 *        possible for such a core algorithm component.
**/

// ReSharper disable InconsistentNaming

using JetBrains.Annotations;
using System.Collections.Generic;

using Type           = System.Type;
using TypeCode       = System.TypeCode;
using StringComparer = System.StringComparer;


namespace Ore
{
  [PublicAPI]
  public class Comparator<T> : IComparator<T>
  {
    public static readonly Comparator<T> Default = new Comparator<T>();


    private static readonly TypeCode s_Type = Type.GetTypeCode(typeof(T));


    [Pure]
    public virtual TypeCode GetTypeCode(in T obj)
    {
      if (typeof(T) == typeof(object))
      {
        return obj is null ? s_Type : Type.GetTypeCode(obj.GetType());
      }

      return s_Type;
    }

    [Pure]
    public virtual bool IsNone(in T obj)
    {
      return obj is null || Equals(obj, default(T));
    }

    [Pure]
    public virtual bool Equals(in T a, in T b)
    {
      // slightly different short-circuiting than object.Equals
      return (object)a == (object)b || (a is { } && a.Equals(b));
    }

    [Pure]
    public virtual int GetHashCode(in T obj)
    {
      return obj?.GetHashCode() ?? 0;
    }

    [Pure]
    public virtual int Compare(in T a, in T b)
    {
      if (s_Type == TypeCode.String)
      {
        return StringComparer.Ordinal.Compare(a as string, b as string);
      }

      if ((object)a == (object)b)
      {
        return  0;
      }

      if (a is null)
      {
        return -1;
      }

      if (b is null)
      {
        return +1;
      }

      if (a is System.IComparable<T> cmp)
      {
        return cmp.CompareTo(b);
      }

      return StringComparer.Ordinal.Compare(a.ToString(), b.ToString());
    }


    bool IEqualityComparer<T>.Equals(T a, T b)
      => Equals(in a, in b);

    int IEqualityComparer<T>.GetHashCode(T obj)
      => GetHashCode(in obj);

    int IComparer<T>.Compare(T a, T b)
      => Compare(in a, in b);

  } // end class Comparator

}