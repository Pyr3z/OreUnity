/*! @file       Runtime/DeferringRoutine.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-08
**/

using IEnumerator = System.Collections.IEnumerator;
using Action      = System.Action;
using Condition   = System.Func<bool>;


namespace Ore
{

  public struct DeferringRoutine : IEnumerator
  {
    public object Current => null;

    Action    m_OnSatisfied;
    Condition m_Condition;
    int       m_FrameCountdown;

    public DeferringRoutine(Action onSatisfied, Condition condition = null, int inFrames = 1)
    {
      m_OnSatisfied    = onSatisfied;
      m_Condition      = condition;
      m_FrameCountdown = inFrames;
    }

    public bool MoveNext()
    {
      if (m_OnSatisfied is null)
      {
        return false;
      }

      if (m_FrameCountdown-- == 0)
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
      }

      return m_FrameCountdown >= 0;
    }

    void IEnumerator.Reset()
    {
      throw new System.InvalidOperationException();
    }

  } // end struct DeferringRoutine

}