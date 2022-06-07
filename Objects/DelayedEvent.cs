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
  public class DelayedEvent : UnityEvent
  {
    public float Delay
    {
      get => m_DelaySeconds.AtLeast(0f);
      set => m_DelaySeconds = value.AtLeast(0f);
    }


    [SerializeField] // TODO implement custom drawers
    private bool m_IsEnabled;

    [SerializeField] // TODO implement custom drawers
    private float m_DelaySeconds = -1f;


    [System.NonSerialized]
    private Coroutine m_Invocation;


    new public void Invoke()
    {
      // TODO implement RootScene equiv.
      throw new System.NotImplementedException(nameof(Invoke));
    }

    public bool TryInvokeOn(GameObject obj)
    {
      if (m_DelaySeconds < Floats.EPSILON)
        base.Invoke();
      else if (m_Invocation == null && obj && obj.TryGetComponent(out MonoBehaviour component) && component.isActiveAndEnabled)
        m_Invocation = component.StartCoroutine(DelayedInvokeCoroutine());
      else
        return false;

      return true;
    }

    public bool TryInvokeOn(MonoBehaviour component)
    {
      if (m_DelaySeconds < Floats.EPSILON)
        base.Invoke();
      else if (m_Invocation == null && component && component.isActiveAndEnabled)
        m_Invocation = component.StartCoroutine(DelayedInvokeCoroutine());
      else
        return false;

      return true;
    }


    private IEnumerator DelayedInvokeCoroutine()
    {
      yield return new WaitForSeconds(m_DelaySeconds);
      base.Invoke();
      m_Invocation = null;
    }

  }

}
