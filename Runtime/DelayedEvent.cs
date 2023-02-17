/*! @file       Objects/DelayedEvent.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-01
**/

using JetBrains.Annotations;

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
    public TimeInterval Delay
    {
      get => m_Delay;
      set => m_Delay = value;
    }

    public bool IsScaledTime
    {
      get => m_ScaledTime && !m_Delay.TicksAreFrames;
      set => m_ScaledTime = value;
    }


    private const bool DEFAULT_SCALED_TIME = false;

    [SerializeField, HideInInspector]
    protected Object m_Context; // auto-assigned in EventDrawer.cs
    [SerializeField, HideInInspector]
    protected bool m_RunInGlobalContext;


    [SerializeField]
    private bool m_ScaledTime = DEFAULT_SCALED_TIME;

    [SerializeField]
    private TimeInterval m_Delay = TimeInterval.Frame;


    [System.NonSerialized]
    private object m_InvokeHandle;


    public DelayedEvent()
    {
    }

    public DelayedEvent(TimeInterval delay, bool isScaled = DEFAULT_SCALED_TIME)
    {
      m_ScaledTime = isScaled;
      m_Delay = isScaled ? delay.WithSystemTicks() : delay;
    }


    public override bool TryInvoke()
    {
      if (!m_IsEnabled || m_InvokeHandle != null)
        return false;

      if (m_Delay < TimeInterval.Frame)
      {
        ((UnityEvent)this).Invoke();
        return true;
      }

      if (m_RunInGlobalContext || m_Context is ScriptableObject)
      {
        if (m_Context)
        {
          ActiveScene.Coroutines.Run(DelayedInvokeCoroutine(), m_Context);
          m_InvokeHandle = m_Context;
        }
        else
        {
          Orator.Reached("unexpected null here.");
          ActiveScene.Coroutines.Run(DelayedInvokeCoroutine(), out string guid);
          m_InvokeHandle = guid;
        }

        return true;
      }

      if (m_Context is MonoBehaviour component && component.isActiveAndEnabled)
      {
        m_InvokeHandle = component.StartCoroutine(DelayedInvokeCoroutine());
        return true;
      }

      return false;
    }

    public bool TryInvokeOn([CanBeNull] MonoBehaviour component)
    {
      if (!m_IsEnabled || m_InvokeHandle != null)
        return false;

      if (m_Delay < TimeInterval.Frame)
      {
        ((UnityEvent)this).Invoke();
        return true;
      }

      if (component && component.isActiveAndEnabled)
      {
        m_InvokeHandle = component.StartCoroutine(DelayedInvokeCoroutine());
        return true;
      }

      return false;
    }


    private IEnumerator DelayedInvokeCoroutine()
    {
      if (m_Delay.TicksAreFrames)
      {
        int i = (int)m_Delay.Ticks;
        while (i --> 0)
          yield return null;
      }
      else if (m_ScaledTime)
      {
        yield return new WaitForSeconds(m_Delay.FSeconds);
      }
      else
      {
        yield return new WaitForSecondsRealtime(m_Delay.FSeconds);
      }

      ((UnityEvent)this).Invoke();

      m_InvokeHandle = null;
    }

  }

}
