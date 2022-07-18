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
    public float DelaySeconds
    {
      get => m_DelaySeconds.AtLeast(0f);
      set => m_DelaySeconds = value;
    }
    public int ApproximateFrameDelay
    {
      get => Mathf.RoundToInt(m_DelaySeconds.AtLeast(0f) * Application.targetFrameRate);
      set => m_DelaySeconds = (float)value / Application.targetFrameRate;
    }
    public bool IsScaledTime
    {
      get => m_ScaledTime;
      set => m_ScaledTime = value;
    }


    private const bool  DEFAULT_SCALED_TIME     = true;
    private const float DEFAULT_DELAY_SECONDS   = -1f;
    private const float MIN_DELAY_SECONDS_ASYNC = 1f / 90f;


    [SerializeField, HideInInspector]
    private bool m_IsEnabled;

    [SerializeField, HideInInspector]
    private MonoBehaviour m_Context; // auto-assigned in EventDrawer.cs

    [SerializeField, HideInInspector]
    private bool m_RunInGlobalContext;


    [SerializeField]
    private bool m_ScaledTime = DEFAULT_SCALED_TIME;

    [SerializeField] // TODO implement [ToggleFloat] custom drawer
    private float m_DelaySeconds = DEFAULT_DELAY_SECONDS;


    [System.NonSerialized]
    private Coroutine m_Invocation;


    public DelayedEvent(float seconds = DEFAULT_DELAY_SECONDS, bool is_scaled = DEFAULT_SCALED_TIME)
      : base()
    {
      m_DelaySeconds  = seconds;
      m_ScaledTime    = is_scaled;
    }

    public static DelayedEvent WithApproximateFrameDelay(int frames)
    {
      return new DelayedEvent(seconds:    (float)frames / Application.targetFrameRate,
                              is_scaled:  false);
    }


    new public void Invoke()
    {
      bool ok = TryInvoke();
      Orator.Assert.True(ok, nameof(TryInvoke), m_Context);
    }

    public bool TryInvoke()
    {
      if (m_RunInGlobalContext)
        return TryInvokeOn(ActiveScene.Current);
      return TryInvokeOn(m_Context);
    }

    public bool TryInvokeOn(MonoBehaviour component)
    {
      if (m_IsEnabled)
      {
        if (m_DelaySeconds < MIN_DELAY_SECONDS_ASYNC)
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
      else if (m_DelaySeconds < 1f / Application.targetFrameRate)
        yield return new WaitForEndOfFrame();
      else
        yield return new WaitForSecondsRealtime(m_DelaySeconds);

      base.Invoke();

      m_Invocation = null;
    }

  }

}
