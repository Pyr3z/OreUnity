/*! @file       Runtime/MultiException.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-03-02
**/

using JetBrains.Annotations;

using Exception = System.Exception;


namespace Ore
{
  [PublicAPI]
  public sealed class MultiException : Exception
  {

    [NotNull]
    public static MultiException Create([NotNull] Exception top,
                                        [NotNull] Exception next,
                                        [ItemNotNull] params Exception[] theRest)
    {
      if (theRest.IsEmpty())
      {
        return new MultiException(top, next);
      }

      int i = theRest.Length;

      var curr = theRest[--i];

      while (i --> 0)
      {
        curr = new MultiException(theRest[i], curr);
      }

      return new MultiException(top, new MultiException(next, curr));
    }

    private MultiException([NotNull] Exception top, [CanBeNull] Exception next)
      : base(top.Message, next)
    {
      HelpLink = top.HelpLink;
      Source   = top.Source;
    }

  } // end class MultiException
}