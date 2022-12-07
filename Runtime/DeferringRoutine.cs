/*! @file       Runtime/DeferringRoutine.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-08
**/

using UnityEngine;
using IEnumerator = System.Collections.IEnumerator;
using Action      = System.Action;
using Condition   = System.Func<bool>;


namespace Ore
{

  public struct DeferringRoutine : IEnumerator
  {
    public object Current => null;

    Action             m_OnSatisfied;
    TimeInterval       m_Countdown;
    readonly Condition m_Condition;

    public DeferringRoutine(Action onSatisfied, Condition condition = null)
      : this(onSatisfied, TimeInterval.One, condition)
    {
    }

    public DeferringRoutine(Action onSatisfied, TimeInterval delay, Condition condition = null)
    {
      m_OnSatisfied = onSatisfied;
      m_Countdown   = delay;
      m_Condition   = condition;
    }

    public bool MoveNext()
    {
      if (m_OnSatisfied is null)
      {
        return false;
      }

      if (m_Countdown.Ticks <= 0L)
      {
        try
        {
          if (m_Condition is null || m_Condition.Invoke())
          {
            m_OnSatisfied.Invoke();
          }
        }
        finally
        {
          m_OnSatisfied = null;
        }

        return false;
      }

      m_Countdown.SubtractSeconds(Time.unscaledDeltaTime);

      return true;
    }

    void IEnumerator.Reset()
    {
      throw new System.InvalidOperationException();
    }

  } // end struct DeferringRoutine

}