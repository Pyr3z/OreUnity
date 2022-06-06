/** @file   Objects/UnanticipatedException.cs
    @author levianperez\@gmail.com
    @author levi\@leviperez.dev
    @date   2022-06-03
**/

namespace Bore
{

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
