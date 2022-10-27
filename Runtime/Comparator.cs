/*! @file       Runtime/Comparator.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-28
 *
 *  Default implementation for the IComparator<T> interface.
**/

// ReSharper disable InconsistentNaming

using System;
using JetBrains.Annotations;

using Type = System.Type;


namespace Ore
{
  [PublicAPI]
  public class Comparator<T> : IComparator<T>
  {
    public static readonly Comparator<T> Default = new Comparator<T>();


    private readonly TypeCode m_Type = Type.GetTypeCode(typeof(T));


    [Pure]
    public virtual bool IsNone(T obj)
    {
      return obj is null || Equals(obj, default(T));
    }

    [Pure]
    public virtual bool Equals(T a, T b)
    {
      // slightly different short-circuiting than object.Equals
      return (object)a == (object)b || (a is { } && a.Equals(b));
    }

    [Pure]
    public virtual int GetHashCode([CanBeNull] T obj)
    {
      return obj?.GetHashCode() ?? 0;
    }

    [Pure]
    public virtual int Compare(T a, T b)
    {
      if (m_Type == TypeCode.String)
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

      if (a is IComparable<T> cmp)
      {
        return cmp.CompareTo(b);
      }

      return StringComparer.Ordinal.Compare(a.ToString(), b.ToString());
    }
  } // end class Comparator

}