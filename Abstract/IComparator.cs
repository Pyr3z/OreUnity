/*! @file       Abstract/IComparator.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-28
**/

using System.Collections.Generic;

using JetBrains.Annotations;


namespace Ore
{
  public interface IComparator<T> : IEqualityComparer<T>, IComparer<T>
  {
    bool IsNone([CanBeNull] T obj);
  }
}