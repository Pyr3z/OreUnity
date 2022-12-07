/*! @file       Objects/DelayedEvent.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-01
**/

using System.Collections;
using System.Diagnostics.CodeAnalysis;

using UnityEngine;

using UnityEvent = UnityEngine.Events.UnityEvent;


namespace Ore
{

  [System.Serializable]
  [SuppressMessage("ReSharper", "MemberInitializerValueIgnored")]
  public class DelayedEvent : VoidEvent
  {
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


    private const bool  DEFAULT_SCALED_TIME      = true;
    private const float DEFAULT_DELAY_SECONDS    = -1f;
    private const float DEFAULT_TARGET_FPS       = 60f;
    private const float MIN_DELAY_SECONDS_ASYNC  = 1f / 90f;

    [SerializeField, HideInInspector]
    protected Object m_Context; // auto-assigned in EventDrawer.cs
    [SerializeField, HideInInspector]
    protected bool m_RunInGlobalContext;

    [SerializeField]
    private bool m_ScaledTime = DEFAULT_SCALED_TIME;

    [SerializeField] // TODO implement [ToggleFloat] custom drawer
    private float m_DelaySeconds = DEFAULT_DELAY_SECONDS;


    [System.NonSerialized]
    private Coroutine m_Invocation;


    public DelayedEvent()
      : this(DEFAULT_DELAY_SECONDS)
    {
    }

    public DelayedEvent(float seconds, bool is_scaled = DEFAULT_SCALED_TIME)
    {
      m_DelaySeconds = seconds;
      m_ScaledTime   = is_scaled;
    }

    public static DelayedEvent WithApproximateFrameDelay(int frames, float target_fps = DEFAULT_TARGET_FPS)
    {
      return new DelayedEvent(seconds: frames / target_fps,
                              is_scaled: false);
    }


    public override bool TryInvoke()
    {
      if (!m_IsEnabled)
        return true;

      if (m_RunInGlobalContext || m_Context is ScriptableObject)
      {
        if (m_DelaySeconds < MIN_DELAY_SECONDS_ASYNC)
        {
          ((UnityEvent)this).Invoke();
        }
        else
        {
          ActiveScene.Coroutines.Run(DelayedInvokeCoroutine(), m_Context);
        }

        return true;
      }

      return TryInvokeOn(m_Context as MonoBehaviour);
    }

    public bool TryInvokeOn(MonoBehaviour component)
    {
      if (!m_IsEnabled)
        return true;

      if (m_DelaySeconds < MIN_DELAY_SECONDS_ASYNC)
      {
        ((UnityEvent)this).Invoke();
      }
      else if (m_Invocation == null && component && component.isActiveAndEnabled)
      {
        m_Invocation = component.StartCoroutine(DelayedInvokeCoroutine());
      }
      else
      {
        return false;
      }

      return true;
    }


    private IEnumerator DelayedInvokeCoroutine()
    {
      if (m_ScaledTime)
        yield return new WaitForSeconds(m_DelaySeconds);
      else if (m_DelaySeconds < 1f / 30f)
        yield return null;
      else
        yield return new WaitForSecondsRealtime(m_DelaySeconds);

      ((UnityEvent)this).Invoke();
      m_Invocation = null;
    }

  }

}
