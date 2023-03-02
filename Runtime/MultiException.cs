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
        return new MultiException(outer: next, inner: top);
      }

      int i = theRest.Length;

      var curr = theRest[--i];

      while (i --> 0)
      {
        curr = new MultiException(outer: curr, inner: theRest[i]);
      }

      return new MultiException(outer: new MultiException(outer: curr, inner: next), inner: top);
    }


    private MultiException([NotNull] Exception outer, [NotNull] Exception inner)
      : base(MakeMessage(inner, outer), inner)
    {
    }

    private static string MakeMessage([NotNull] Exception inner, [NotNull] Exception outer)
    {
      int flags = 0;

      if (inner is MultiException)
        flags |= 0b01;
      if (outer is MultiException)
        flags |= 0b10;

      switch (flags)
      {
        default:
        case 0b00:
          return $"{inner.GetType().Name}: {inner.Message}\n{outer.GetType().Name}: {outer.Message}";

        case 0b01:
          return $"{inner.Message}\n{outer.GetType().Name}: {outer.Message}";

        case 0b10:
          return $"{inner.GetType().Name}: {inner.Message}\n{outer.Message}";

        case 0b11:
          return $"{inner.Message}\n{outer.Message}";
      }
    }

  } // end class MultiException
}