/** @file       Objects/DelayedEvent.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-01
**/

using System.Collections;

using UnityEngine;

using UnityEvent = UnityEngine.Events.UnityEvent;


namespace Bore
{

  [System.Serializable]
  public class DelayedEvent : UnityEvent, IEvent
  {
    public bool IsEnabled
    {
      get => m_IsEnabled;
      set => m_IsEnabled = value;
    }
    public float Delay
    {
      get => m_DelaySeconds.AtLeast(0f);
      set => m_DelaySeconds = value.AtLeast(0f);
    }


    [SerializeField] [HideInInspector]
    private bool m_IsEnabled;

    [SerializeField]
    private bool m_ScaledTime = true;

    [SerializeField] // TODO implement [ToggleFloat] custom drawer
    private float m_DelaySeconds = -1f;


    [System.NonSerialized]
    private Coroutine m_Invocation;


    new public void Invoke()
    {
      bool ok = TryInvoke();
      Debug.Assert(ok, "TryInvoke()");
    }

    public bool TryInvoke()
    {
      return TryInvokeOn(ActiveScene.Current);
    }

    public bool TryInvokeOn(GameObject obj)
    {
      if (m_IsEnabled)
      {
        if (m_DelaySeconds < Floats.EPSILON)
          base.Invoke();
        else if (m_Invocation == null && obj && obj.TryGetComponent(out MonoBehaviour component) && component.isActiveAndEnabled)
          m_Invocation = component.StartCoroutine(DelayedInvokeCoroutine());
        else
          return false;
      }

      return true;
    }

    public bool TryInvokeOn(MonoBehaviour component)
    {
      if (m_IsEnabled)
      {
        if (m_DelaySeconds < Floats.EPSILON)
          base.Invoke();
        else if (m_Invocation == null && component && component.isActiveAndEnabled)
          m_Invocation = component.StartCoroutine(DelayedInvokeCoroutine());
        else
          return false;
      }

      return true;
    }


    private IEnumerator DelayedInvokeCoroutine()
    {
      if (m_ScaledTime)
        yield return new WaitForSeconds(m_DelaySeconds);
      else
        yield return new WaitForSecondsRealtime(m_DelaySeconds);

      base.Invoke();

      m_Invocation = null;
    }

  }

}
