/** @file       Static/Find.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-06
**/

using UnityEngine;


namespace Bore
{

  public static class Find
  {

    public static bool Any<T>(out T obj) where T : Object
    {
      obj = null;
      return obj;
    }

  } // end static class Find

}
