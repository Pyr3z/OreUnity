/** @file       Abstract/IEvent.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-06
**/


namespace Bore
{

  public interface IEvent
  {
    bool IsEnabled { get; }
    void Invoke();
    bool TryInvoke();
  }

}
