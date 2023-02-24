/*! @file       Objects/DelayedEvent.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-01
**/

using JetBrains.Annotations;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEvent = UnityEngine.Events.UnityEvent;


namespace Ore
{

  [System.Serializable]
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
          ActiveScene.Coroutines.Run(InvokeCoroutine(), m_Context);
          m_InvokeHandle = m_Context;
        }
        else
        {
          ActiveScene.Coroutines.Run(InvokeCoroutine(), out string guid);
          m_InvokeHandle = guid;
        }

        return true;
      }

      if (m_Context is MonoBehaviour component && component.isActiveAndEnabled)
      {
        m_InvokeHandle = new KeyValuePair<MonoBehaviour,Coroutine>(component,
                                                                   component.StartCoroutine(InvokeCoroutine()));
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
        m_InvokeHandle = new KeyValuePair<MonoBehaviour,Coroutine>(component,
                                                                   component.StartCoroutine(InvokeCoroutine()));
        return true;
      }

      // don't fallback on ActiveScene - this method specifically wants to run on the given object.

      return false;
    }

    public bool TryInvokeOnGlobalContext()
    {
      if (!m_IsEnabled || m_InvokeHandle != null)
        return false;

      if (m_Delay < TimeInterval.Frame)
      {
        ((UnityEvent)this).Invoke();
        return true;
      }

      if (m_Context)
      {
        ActiveScene.Coroutines.Run(InvokeCoroutine(), m_Context);
        m_InvokeHandle = m_Context;
      }
      else
      {
        ActiveScene.Coroutines.Run(InvokeCoroutine(), out string guid);
        m_InvokeHandle = guid;
      }

      return true;
    }

    public bool TryCancelInvoke()
    {
      if (m_InvokeHandle == null)
        return false;

      if (m_InvokeHandle is Object contract)
      {
        m_InvokeHandle = null;

        if (!contract)
          return false;

        ActiveScene.Coroutines.Halt(contract);
        return true;
      }

      if (m_InvokeHandle is string guid)
      {
        m_InvokeHandle = null;
        ActiveScene.Coroutines.Halt(guid);
        return true;
      }

      if (m_InvokeHandle is KeyValuePair<MonoBehaviour,Coroutine> kvp)
      {
        m_InvokeHandle = null;

        if (!kvp.Key || !kvp.Key.isActiveAndEnabled)
          return false;

        kvp.Key.StopCoroutine(kvp.Value);
        return true;
      }

      throw new UnanticipatedException($"{nameof(m_InvokeHandle)} is not null, but is also ??? ~ type={m_InvokeHandle.GetType().FullName}");
    }


    private IEnumerator InvokeCoroutine()
    {
      return m_ScaledTime ? ScaledInvokeCoroutine(m_Delay.FSeconds) :
                            new DelayedRoutine(InvokePayload, m_Delay);
    }

    private IEnumerator ScaledInvokeCoroutine(float seconds)
    {
      yield return new WaitForSeconds(seconds);

      try
      {
        ((UnityEvent)this).Invoke();
      }
      catch (System.Exception ex)
      {
        Orator.NFE(ex, m_Context);
      }
      finally
      {
        m_InvokeHandle = null;
      }
    }

    private void InvokePayload()
    {
      try
      {
        ((UnityEvent)this).Invoke();
      }
      catch (System.Exception ex)
      {
        Orator.NFE(ex, m_Context);
      }
      finally
      {
        m_InvokeHandle = null;
      }
    }

  }

}
