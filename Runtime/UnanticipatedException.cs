/** @file   Objects/UnanticipatedException.cs
 *  @author levi\@leviperez.dev
 *  @date   2022-06-03
**/


namespace Ore
{
  /// <summary>
  /// A Bore-made generic exception type for self-documenting our own impossible error cases.
  /// </summary>
  public sealed class UnanticipatedException : System.NotImplementedException
  {

    public UnanticipatedException(System.Exception inner) :
      base("Unanticipated exception case!", inner)
    {
    }

    public UnanticipatedException(string message) :
      this(new System.InvalidOperationException(message))
    {
    }

  } // end class UnanticipatedException

}
