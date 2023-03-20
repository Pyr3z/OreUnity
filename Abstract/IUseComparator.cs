/*! @file       Abstract/IUseComparator.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-03-20
**/

namespace Ore
{
  public interface IUseComparator<T>
  {

    IComparator<T> Comparator { get; }

  } // end class IUseComparator
}