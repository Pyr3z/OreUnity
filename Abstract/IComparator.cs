/*! @file       Abstract/IComparator.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-28
**/

using System.Collections.Generic;

using JetBrains.Annotations;

using TypeCode = System.TypeCode;


namespace Ore
{
  [PublicAPI]
  public interface IComparator<T> : IEqualityComparer<T>, IComparer<T>
  {
    [Pure]
    TypeCode GetTypeCode([CanBeNull] in T obj);

    [Pure]
    bool IsNone([CanBeNull] in T obj);

    [Pure]
    bool Equals([CanBeNull] in T a, [CanBeNull] in T b);

    [Pure]
    int GetHashCode([CanBeNull] in T obj);

    [Pure]
    int Compare([CanBeNull] in T a, [CanBeNull] in T b);

    [Pure]
    bool ComparatorEquals<U>(IComparator<U> other);
  }
}