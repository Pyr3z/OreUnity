/*! @file       Runtime/Future.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-03-02
**/

namespace Ore
{
  public struct Future<T>
  {

    private Promise<T> m_Promise;

    internal Future(Promise<T> promise)
    {
      m_Promise = promise;
    }

  } // end struct Future
}