/*! @file       Runtime/HashKeyComparator.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-28
**/

using System.Collections;
using System.Collections.Generic;

using JetBrains.Annotations;

using IConvertible = System.IConvertible;


namespace Ore
{

  public class HashKeyComparator<T> : IEqualityComparer<T>
  {
    public static readonly HashKeyComparator<T> Default = new HashKeyComparator<T>();


    public bool Equals([CanBeNull] T a, [CanBeNull] T b)
    {
      // slightly different short-circuiting than object.Equals
      return a is {} && a.Equals(b);
    }

    public int GetHashCode([CanBeNull] T obj)
    {
      return obj?.GetHashCode() ?? 0;
    }
  } // end class HashKeyComparator

}