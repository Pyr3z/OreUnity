/*! @file       Runtime/HashKeyComparator.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-28
**/

using System.Collections.Generic;

using JetBrains.Annotations;


namespace Ore
{

  public interface IHashKeyComparator<T> : IEqualityComparer<T>
  {
    bool IsNullKey([CanBeNull] T obj);
  }


  public class HashKeyComparator<T> : IHashKeyComparator<T>
  {
    public static readonly HashKeyComparator<T> Default = new HashKeyComparator<T>();


    [PublicAPI]
    public bool IsNullKey(T key)
    {
      return key is null || Equals(key, default(T));
    }

    [PublicAPI]
    public bool Equals(T a, T b)
    {
      // slightly different short-circuiting than object.Equals
      return a is {} && a.Equals(b);
    }

    [PublicAPI]
    public int GetHashCode([CanBeNull] T key)
    {
      return key?.GetHashCode() ?? 0;
    }
  } // end class HashKeyComparator

}