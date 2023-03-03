/*! @file       Runtime/Exceptions.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-03-02
**/

using JetBrains.Annotations;

using Exception = System.Exception;


namespace Ore
{
  public static class Exceptions
  {

    public static Exception Silenced([CanBeNull] this Exception self)
    {
      return FauxException.Silence(self);
    }

  } // end static class Exceptions
}