/*! @file       Runtime/VoidEvent.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-08
**/

using UnityEngine;
using UnityEngine.Events;


namespace Ore
{
  [System.Serializable]
  public class VoidEvent : UnityEvent, IEvent
  {
    public bool IsEnabled
    {
      get => m_IsEnabled;
      set => m_IsEnabled = value;
    }

    [SerializeField, HideInInspector]
    protected bool m_IsEnabled;


    public new void Invoke()
    {
      _ = TryInvoke();
    }

    public virtual bool TryInvoke()
    {
      if (!m_IsEnabled)
        return false;

      base.Invoke();
      return true;
    }

  } // end class VoidEvent
}