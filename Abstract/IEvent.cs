/** @file       Abstract/IEvent.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-06
**/

using Action = UnityEngine.Events.UnityAction;


namespace Ore
{

  public interface IEvent
  {
    bool IsEnabled { get; set; }
    void Invoke();
    bool TryInvoke();


    #region UnityEvent throughface

    void AddListener(Action action);
    void RemoveListener(Action action);
    void RemoveAllListeners();

    #endregion UnityEvent throughface
  }

}
