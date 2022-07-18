/** @file       Abstract/IImmortalSingleton.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-21
**/

namespace Bore
{
  /// <summary>
  /// Marks a Singleton as unkillable, meaning it cannot be destroyed,
  /// even in the Editor.
  /// </summary>
  public interface IImmortalSingleton
  {
    // primarily a type tag, a.k.a. policy sugar.
  }

}