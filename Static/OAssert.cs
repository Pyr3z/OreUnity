/** @file       Static/OAssert.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-01
**/

namespace Bore
{

  public /* static */ sealed class OAssert : Orator.Assert
  {

    // this is a syntax sugar class, so you can write, say, `OAssert.True(...)`
    // instead of `Orator.Assert.True(...)`.

  }

}
