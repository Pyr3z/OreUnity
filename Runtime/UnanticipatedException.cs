/** @file   Objects/UnanticipatedException.cs
    @author levianperez\@gmail.com
    @author levi\@leviperez.dev
    @date   2022-06-03

    @brief  A new generic exception type with mega self-documentation in mind.
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
