/** @file   Objects/UnhandledException.cs
    @author levianperez\@gmail.com
    @author levi\@leviperez.dev
    @date   2022-06-03
**/

namespace Bore
{

  public sealed class UnhandledException : System.NotImplementedException
  {

    public UnhandledException(System.Exception inner) :
      base("Encountered unanticipated exception!", inner)
    {
    }

  } // end class UnhandledException

}
