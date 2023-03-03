/*! @file       Runtime/FauxException.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-03-02
**/

using JetBrains.Annotations;

using Exception = System.Exception;


namespace Ore
{
  [PublicAPI]
  public sealed class FauxException : Exception
  {
    public static readonly FauxException Default = new FauxException();


    [NotNull]
    public static FauxException Silence([CanBeNull] Exception other)
    {
      if (other is null)
        return Default;

      if (other is FauxException already)
        return already;

      return new FauxException(other);
    }


    public FauxException()
      : base(DEFAULT_MSG)
    {
    }

    private FauxException(Exception inner)
      : base(DEFAULT_MASK_MSG, inner)
    {
    }


    private const string DEFAULT_MSG      = "(faux exception, please disregard)";
    private const string DEFAULT_MASK_MSG = "(inner exception has been silenced)";

  } // end class FauxException
}