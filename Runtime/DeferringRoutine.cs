/*! @file       Runtime/DeferringRoutine.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-08
**/

using JetBrains.Annotations;
using UnityEngine;
using IEnumerator = System.Collections.IEnumerator;
using Action      = System.Action;
using Condition   = System.Func<bool>;


namespace Ore
{

  public struct DeferringRoutine : IEnumerator
  {
    public object Current => null;

    Action             m_Payload;
    readonly float     m_DoneTime;
    readonly Condition m_Condition;

    /// <summary>
    /// Without a delay specified, invokes the payload next frame (when run
    /// as a normal coroutine).
    /// </summary>
    /// 
    /// <param name="payload">
    /// The callback to be invoked at the end of this routine, if all conditions
    /// are satisfied. If it's null or no-op, this struct is useless.
    /// </param>
    /// 
    /// <param name="condition">
    /// If not null, <paramref name="payload"/> will only trigger if the
    /// condition evaluates to true.
    /// </param>
    public DeferringRoutine([CanBeNull] Action    payload,
                            [CanBeNull] Condition condition = null)
      : this(payload, TimeInterval.Epsilon, condition)
    {
    }

    /// <summary>
    /// Invokes the payload callback after the given delay has elapsed and any
    /// given conditions have been satisfied.
    /// </summary>
    /// 
    /// <param name="payload">
    /// The callback to be invoked at the end of this routine, if all conditions
    /// are satisfied. If it's null or no-op, this struct is useless.
    /// </param>
    ///
    /// <param name="delay">
    /// The TimeInterval to wait for before attempting to invoke the payload.
    /// If negative or zero, the invocation is initiated immediately.
    /// </param>
    ///
    /// <param name="condition">
    /// If not null, <paramref name="payload"/> will only trigger after the
    /// delay interval if the condition evaluates to true.
    /// </param>
    public DeferringRoutine([CanBeNull] Action       payload,
                                        TimeInterval delay,
                            [CanBeNull] Condition    condition = null)
    {
      m_Payload   = payload;
      m_Condition = condition;

      if (delay.Ticks <= 0L)
        m_DoneTime = -1f;
      else
        m_DoneTime = Time.unscaledTime + delay.FSeconds;
    }

    public bool MoveNext()
    {
      if (m_Payload is null)
      {
        return false;
      }

      if (Time.unscaledTime > m_DoneTime)
      {
        try
        {
          if (m_Condition is null || m_Condition.Invoke())
          {
            m_Payload.Invoke();
          }
        }
        finally
        {
          m_Payload = null;
        }

        return false;
      }

      return true;
    }

    void IEnumerator.Reset()
    {
      throw new System.InvalidOperationException();
    }

  } // end struct DeferringRoutine

}